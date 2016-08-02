using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ethanol
{
    public class DataReader
    {
        public byte[] Data;
        public int SampleRate { get; set; }
        public int DataRate { get; set; }
        public int WrapCount { get; set; }
        
        internal int SamplesPerBit
        {
            get
            {
                return SampleRate / DataRate;
            }
        }

        private double Time = 0;

        public DataReader(byte[] data)
        {
            SampleRate = 44100;
            DataRate = 1200;
            Data = data;
        }

        public double Step()
        {
            Time++;

            int bit_index = (int)(Time / SamplesPerBit);
            int byte_index = (bit_index / 8) % Data.Length;
            WrapCount = (bit_index / 8) / Data.Length;
            bit_index %= 8;
            bit_index = 7 - bit_index;

            byte b = Data[byte_index];
            int ret = (b >> bit_index) % 2 == 0 ? 0 : 1;
            return ret;
        }
    }
}
