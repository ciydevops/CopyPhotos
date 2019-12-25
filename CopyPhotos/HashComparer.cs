using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopyPhotos
{
    internal class HashComparer : IEqualityComparer<FileEntry>
    {
        public bool Equals(FileEntry x, FileEntry y)
        {
            if (String.IsNullOrEmpty(x.Hash))
            {
                return false;
            }

            if (String.IsNullOrEmpty(y.Hash))
            {
                return false;
            }

            return String.Equals(x.Hash, y.Hash);
        }

        public int GetHashCode(FileEntry obj)
        {
            if (String.IsNullOrEmpty(obj.Hash))
            {
                return 0;
            }

            return obj.Hash.GetHashCode();
        }
    }
}
