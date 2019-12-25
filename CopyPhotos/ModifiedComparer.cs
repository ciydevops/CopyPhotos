using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopyPhotos
{
    internal class ModifiedComparer : IEqualityComparer<FileEntry>
    {
        public bool Equals(FileEntry x, FileEntry y)
        {
            if (String.IsNullOrEmpty(x.Modified))
            {
                return false;
            }

            if (String.IsNullOrEmpty(y.Modified))
            {
                return false;
            }

            return String.Equals(x.Modified, y.Modified);
        }

        public int GetHashCode(FileEntry obj)
        {
            if (String.IsNullOrEmpty(obj.Modified))
            {
                return 0;
            }

            return obj.Modified.GetHashCode();
        }
    }
}
