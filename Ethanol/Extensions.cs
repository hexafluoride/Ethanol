using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ethanol
{
    public static class Extensions
    {
        public static byte[] Read(this Stream stream, int length)
        {
            byte[] buf = new byte[length];
            stream.Read(buf, 0, length);
            return buf;
        }
    }
}
