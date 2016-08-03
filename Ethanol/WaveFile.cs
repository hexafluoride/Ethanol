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
        public RiffFile File;

        public WaveFormat Format = WaveFormat.FloatingPoint;
        public uint SampleRate = 44100;
        public uint ChannelCount = 1;
        public uint BitDepth = 64;

        public MemoryStream FrameBuffer = new MemoryStream();
        
        public WaveFile(string path)
        {
            File = new RiffFile(path);

            if (File.File.Length > 0)
            {
                File.ReadHeader();
                File.ReadChunks();

                var fmt = File.GetChunk("fmt ");

                if (fmt == null)
                    throw new Exception("Couldn't find format chunk");

                Format = (WaveFormat)BitConverter.ToUInt16(fmt.Data, 0);
                ChannelCount = BitConverter.ToUInt16(fmt.Data, 2);
                SampleRate = BitConverter.ToUInt32(fmt.Data, 4);
                BitDepth = BitConverter.ToUInt16(fmt.Data, 14);

                var data = File.GetChunk("data");

                if (data == null)
                    throw new Exception("Couldn't find data chunk");

                FrameBuffer.Write(data.Data, 0, data.Data.Length);
                FrameBuffer.Seek(0, SeekOrigin.Begin);
            }
            else
                File.FileType = "WAVE";
        }

        public void WriteHeader()
        {
            MemoryStream buf = new MemoryStream();

            buf.Write(BitConverter.GetBytes((ushort)Format), 0, 2);
            buf.Write(BitConverter.GetBytes((ushort)ChannelCount), 0, 2);
            buf.Write(BitConverter.GetBytes(SampleRate), 0, 4);
            buf.Write(BitConverter.GetBytes(SampleRate * ChannelCount * BitDepth / 8), 0, 4);
            buf.Write(BitConverter.GetBytes((ushort)(ChannelCount * BitDepth / 8)), 0, 2);
            buf.Write(BitConverter.GetBytes((ushort)BitDepth), 0, 2);

            RiffChunk chunk = new RiffChunk("fmt ", buf.ToArray());
            File.WriteHeader();
            File.WriteChunk(chunk);
        }

        public void WriteData()
        {
            RiffChunk chunk = new RiffChunk("data", FrameBuffer.ToArray());
            File.WriteChunk(chunk);
        }

        public void WriteFrame(double data)
        {
            var buf = ConvertToFormat(data);
            FrameBuffer.Write(buf, 0, buf.Length);
        }

        public double ReadFrame()
        {
            var buf = new byte[BitDepth / 8];
            FrameBuffer.Read(buf, 0, buf.Length);
            return ConvertFromFormat(buf);
        }

        public byte[] ConvertToFormat(double data)
        {
            switch(Format)
            {
                case WaveFormat.FloatingPoint:
                    if (BitDepth == 32)
                        return BitConverter.GetBytes((float)data);
                    else if (BitDepth == 64)
                        return BitConverter.GetBytes(data);
                    break;
                case WaveFormat.PCM:
                    if(BitDepth == 8)
                    {
                        // 8 bits-per-sample is unsigned
                        return new byte[] { (byte)(data * 127 + 128) };
                    }
                    else if(BitDepth == 16)
                    {
                        data *= short.MaxValue;
                        return BitConverter.GetBytes((short)data);
                    }
                    else if(BitDepth == 32)
                    {
                        data *= int.MaxValue;
                        return BitConverter.GetBytes((int)data);
                    }
                    else if(BitDepth == 64)
                    {
                        data *= long.MaxValue;
                        return BitConverter.GetBytes((long)data);
                    }
                    break;
            }

            throw new Exception("Unsupported bits-per-sample or WAV format");
        }

        public double ConvertFromFormat(byte[] data)
        {
            switch(Format)
            {
                case WaveFormat.FloatingPoint:
                    if (BitDepth == 32)
                        return BitConverter.ToSingle(data, 0);
                    else if (BitDepth == 64)
                        return BitConverter.ToDouble(data, 0);
                    break;
                case WaveFormat.PCM:
                    if(BitDepth == 8)
                    {
                        return (data[0] - 128) / 127d;
                    }
                    else if(BitDepth == 16)
                    {
                        short tmp = BitConverter.ToInt16(data, 0);
                        return tmp / (double)short.MaxValue;
                    }
                    else if(BitDepth == 32)
                    {
                        int tmp = BitConverter.ToInt32(data, 0);
                        return tmp / (double)int.MaxValue;
                    }
                    else if(BitDepth == 64)
                    {
                        long tmp = BitConverter.ToInt64(data, 0);
                        return tmp / (double)long.MaxValue;
                    }
                    break;
            }

            throw new Exception("Unsupported bits-per-sample or WAV format");
        }

        public void Save()
        {
            File.File.SetLength(0);

            WriteHeader();
            WriteData();
            File.WriteFileSize();
        }
    }

    public enum WaveFormat
    {
        PCM = 0x0001,
        FloatingPoint = 0x0003,
        ALaw = 0x0006,
        MuLaw = 0x0007,
        Extended = 0xFFFE
    }
}
