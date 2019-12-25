using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace CopyPhotos
{
    class Program
    {
        private const string pattern = "*.jpg";

        static void Main(string[] args)
        {
            var photosPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            var fileDbPath = Path.Combine(photosPath, "Photos.xml");

            var fileDb = LoadOrCreateEmptyFileDb(fileDbPath);
            UpdateFileDb(photosPath, ref fileDb);
            SaveFileDb(fileDbPath, fileDb);

            foreach (var drive in GetAllRemovableDrives())
            {
                if (!drive.IsReady)
                {
                    continue;
                }

                var newDbPath = Path.Combine(drive.RootDirectory.FullName, "Photos.xml");
                var newDb = LoadOrCreateEmptyFileDb(newDbPath);

                var dcimPath = Path.Combine(drive.RootDirectory.FullName, "DCIM");
                UpdateFileDb(dcimPath, ref newDb);
                SaveFileDb(newDbPath, newDb);

                FetchNewPhotos(newDb, fileDb, photosPath);
            }
        }

        private static IEnumerable<DriveInfo> GetAllRemovableDrives()
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType == DriveType.Removable)
                {
                    yield return drive;
                }
            }
        }

        private static void FetchNewPhotos(FileDb newDb, FileDb fileDb, string target)
        {
            foreach (var copyFile in newDb.Except<FileEntry>(fileDb, new FileEntryComparer()))
            {
                Console.WriteLine("Copying: {0}", copyFile.Path);

                var file = new FileInfo(copyFile.Path);
                var targetSub = new DirectoryInfo(Path.Combine(target, GetModifiedText(copyFile)));
                if (!targetSub.Exists)
                {
                    targetSub.Create();
                }

                var targetFile = new FileInfo(Path.Combine(targetSub.FullName, file.Name));
                if (!targetFile.Exists)
                {
                    file.CopyTo(targetFile.FullName);
                }
            }
        }

        private static string GetModifiedText(FileEntry entry)
        {
            var modified = DateTime.Parse(entry.Modified, null, DateTimeStyles.RoundtripKind);
            return modified.ToString("yyyy-MM-dd");
        }

        private static void UpdateFileDb(string photosPath, ref FileDb fileDb)
        {
            var oldFileDb = fileDb;
            var newFiles = GetAllFileEntriesRecursive(photosPath, pattern);
            var newFileDb = new FileDb();

            foreach (FileEntry newEntry in newFiles)
            {
                FileEntry foundOldEntry = null;
                foreach(FileEntry oldEntry in oldFileDb)
                {
                    if (oldEntry.Path == newEntry.Path)
                    {
                        foundOldEntry = oldEntry;
                        break;
                    }
                }

                if (foundOldEntry == null)
                {
                    Console.WriteLine("Adding: {0}", newEntry.Path);

                    CreateHash(newEntry);
                    if (!string.IsNullOrEmpty(newEntry.Hash))
                    {
                        newFileDb.Add(newEntry);
                    }
                    continue;
                }

                if (foundOldEntry.Created != newEntry.Created || foundOldEntry.Modified != newEntry.Modified)
                {
                    Console.WriteLine("Updating: {0}", newEntry.Path);

                    CreateHash(newEntry);
                    if (!string.IsNullOrEmpty(newEntry.Hash))
                    {
                        newFileDb.Add(newEntry);
                    }
                    continue;
                }

                Console.WriteLine("Pending: {0}", foundOldEntry.Path);
                newFileDb.Add(foundOldEntry);
            }

            fileDb = newFileDb;
        }


        // GOOD BELOW HERE

        private static void SaveFileDb(string filePath, FileDb fileDb)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            XmlSerializer writer = new XmlSerializer(typeof(FileDb));
            using (FileStream file = System.IO.File.Create(filePath))
            {
                writer.Serialize(file, fileDb);
                file.Close();
            }

            File.SetAttributes(filePath, FileAttributes.Hidden);
        }

        private static string GetOrCreateAppFolder(string appName)
        {
            string path = Application.LocalUserAppDataPath;

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        private static void CreateHash(FileEntry entry)
        {
            Console.WriteLine("Hashing: {0}", entry.Path);

            RIPEMD160 myRIPEMD160 = RIPEMD160Managed.Create();
            byte[] hashValue;

            try
            {
                var info = new FileInfo(entry.Path);
                using (FileStream fileStream = info.Open(FileMode.Open))
                {
                    fileStream.Position = 0;
                    hashValue = myRIPEMD160.ComputeHash(fileStream);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Hash was not possible.");
                return;
            }

            entry.Hash = GetTextFromHash(hashValue);
        }

        public static string GetTextFromHash(byte[] hash)
        {
            var builder = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
            {
                builder.AppendFormat("{0:X2}", hash[i]);
            }

            return builder.ToString();
        }

        private static FileDb LoadOrCreateEmptyFileDb(string fileDbPath)
        {
            if (!File.Exists(fileDbPath))
            {
                return new FileDb();
            }

            try
            {
                XmlSerializer reader = new XmlSerializer(typeof(FileDb));
                FileStream file = File.OpenRead(fileDbPath);
                var fileDb = (FileDb)reader.Deserialize(file);
                file.Close();
                return fileDb;
            }
            catch
            {
                return new FileDb();
            }
        }

        private static IEnumerable<FileEntry> GetAllFileEntriesRecursive(string directory, string pattern)
        {
            foreach (FileInfo file in GetAllFilesRecursive(directory, pattern))
            {
                var entry = new FileEntry();

                try
                {
                    entry.Path = file.FullName;
                    entry.Created = file.CreationTimeUtc.ToString("O");
                    entry.Modified = file.LastWriteTimeUtc.ToString("O");
                }
                catch
                {
                    continue;
                }

                yield return entry;
            }
        }

        private static IEnumerable<FileInfo> GetAllFilesRecursive(string directory, string pattern)
        {
            foreach (string file in Directory.GetFiles(directory, pattern, SearchOption.AllDirectories))
            {
                yield return new FileInfo(file);
            }
        }
    }
}
