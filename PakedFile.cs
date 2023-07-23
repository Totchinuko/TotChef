using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TotChef
{
    public struct PakedFile
    {
        public string pakName;
        public string path;
        public string sha;
        public long size;

        public override string ToString()
        {
            return $"{sha} - {path}";
        }
    }
}
