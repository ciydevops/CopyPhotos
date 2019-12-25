using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopyPhotos
{
    internal class FileEntryComparer : IEqualityComparer<FileEntry>
    {
        public bool Equals(FileEntry x, FileEntry y)
        {
            var name = new NameComparer();
            if (!name.Equals(x, y))
            {
                return false;
            }

            var hash = new HashComparer();
            if (!hash.Equals(x, y))
            {
                return false;
            }

            return true;
        }

        public int GetHashCode(FileEntry obj)
        {
            int result = 0;

            var name = new NameComparer();
            result += name.GetHashCode(obj);

            var hash = new HashComparer();
            result += hash.GetHashCode(obj);

            return result;
        }
    }
}
