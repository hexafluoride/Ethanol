using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ethanol
{
    public class RiffFile
    {
        public string FileType { get; set; }
        public uint FileSize { get; set; }

        public List<RiffChunk> chunks = new List<RiffChunk>();

        public FileStream File;

        public RiffFile(string path)
        {
            File = new FileStream(path, FileMode.OpenOrCreate);
        }

        public void WriteHeader()
        {
            File.Seek(0, SeekOrigin.Begin);

            File.Write(Encoding.ASCII.GetBytes("RIFF"), 0, 4);
            File.Write(new byte[4], 0, 4); // hold off on writing filesize until we're done
            File.Write(Encoding.ASCII.GetBytes(FileType.Substring(0, 4)), 0, 4);
        }

        public void WriteFileSize()
        {
            long pos = File.Position;

            File.Seek(4, SeekOrigin.Begin);
            File.Write(BitConverter.GetBytes(FileSize), 0, 4);
            File.Seek(pos, SeekOrigin.Begin);
        }

        public void ReadHeader()
        {
            File.Seek(0, SeekOrigin.Begin);

            byte[] magic = File.Read(4);
            byte[] length = File.Read(4);
            byte[] type = File.Read(4);

            if (!magic.SequenceEqual(Encoding.ASCII.GetBytes("RIFF")))
                throw new Exception("Invalid RIFF magic string");

            FileSize = BitConverter.ToUInt32(length, 0);
            FileType = Encoding.ASCII.GetString(type);
        }

        public void ReadChunks()
        {
            RiffChunk buf;

            while ((buf = ReadNextChunk()) != null)
                chunks.Add(buf);
        }

        public RiffChunk GetChunk(string name)
        {
            var chunk = chunks.First(c => c.Tag == name);

            return chunk;
        }

        public RiffChunk ReadNextChunk()
        {
            if (File.Position >= File.Length - 1)
                return null;

            return RiffChunk.Parse(File);
        }

        public void WriteChunk(RiffChunk chunk)
        {
            var buf = chunk.Serialize();
            File.Write(buf, 0, buf.Length);
        }
    }
}
