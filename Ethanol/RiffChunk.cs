using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ethanol
{
    public class RiffChunk
    {
        public string Tag { get; set; }
        public byte[] Data { get; set; }

        public RiffChunk(string tag, byte[] data)
        {
            Tag = tag;
            Data = data;
        }

        public static RiffChunk Parse(Stream stream)
        {
            byte[] tag_buf = new byte[4];
            byte[] length_buf = new byte[4];

            stream.Read(tag_buf, 0, tag_buf.Length);
            stream.Read(length_buf, 0, length_buf.Length);

            string tag = Encoding.ASCII.GetString(tag_buf);
            uint length = BitConverter.ToUInt32(length_buf, 0);

            byte[] data = new byte[length];

            stream.Read(data, 0, data.Length);

            return new RiffChunk(tag, data);
        }

        public byte[] Serialize()
        {
            MemoryStream ms = new MemoryStream();

            ms.Write(Encoding.ASCII.GetBytes(Tag), 0, 4);
            ms.Write(BitConverter.GetBytes((uint)Data.Length), 0, 4);
            ms.Write(Data, 0, Data.Length);

            return ms.ToArray();
        }
    }
}
