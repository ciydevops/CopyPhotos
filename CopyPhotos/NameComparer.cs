using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopyPhotos
{
    internal class NameComparer : IEqualityComparer<FileEntry>
    {
        public bool Equals(FileEntry x, FileEntry y)
        {
            if (String.IsNullOrEmpty(x.Path))
            {
                return false;
            }

            if (String.IsNullOrEmpty(y.Path))
            {
                return false;
            }

            return String.Equals(GetName(x), GetName(y));
        }

        public int GetHashCode(FileEntry obj)
        {
            if (String.IsNullOrEmpty(obj.Path))
            {
                return 0;
            }

            return GetName(obj).GetHashCode();
        }

        private string GetName(FileEntry obj)
        {
            var info = new FileInfo(obj.Path);
            return info.Name;
        }
    }
}
