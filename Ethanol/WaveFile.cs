using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ethanol
{
    public class WaveFile
    {
        public FileStream FileStream;
        public BinaryWriter BinaryWriter;

        public WaveFile(string filename)
        {
            FileStream = new FileStream(filename, FileMode.OpenOrCreate);
            BinaryWriter = new BinaryWriter(FileStream);
        }

        public void WriteFrame(double data)
        {
            BinaryWriter.Write(data);
        }

        public void Save()
        {
            BinaryWriter.Close();
        }
    }
}
