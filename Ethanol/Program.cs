using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using NDesk.Options;
using System.Runtime.InteropServices;

namespace Ethanol
{
    class Program
    {
        static void Main(string[] args)
        {
            int sample_rate = 44100;
            int bit_depth = 64;
            WaveFormat format = WaveFormat.FloatingPoint;

            int data_rate = 1200;
            int low_freq = 1200;
            int high_freq = 2200;

            double amplitude = 0.25;

            string input_filename = "";

            OptionSet set = null;
            set = new OptionSet()
            {
                {"h|?|help", "Show help for arguments.", a => ShowHelp(set) },
                {"sr=|sample-rate=", "Output sample rate.", s => sample_rate = int.Parse(s) },
                {"b=|bit-depth=", "Output bit depth(8, 16, 32 or 64).", b => bit_depth = int.Parse(b) },
                {"f=|wave-format=", "Output wave format(pcm or floatingpoint).", f => format = (WaveFormat)Enum.Parse(typeof(WaveFormat), f, true) },
                {"r=|baud=", "Data rate to modulate the carrier wave at.", r => data_rate = int.Parse(r) },
                {"lf=|low-freq=", "Frequency to encode for a low bit(0).", l => low_freq = int.Parse(l) },
                {"hf=|high-freq=", "Frequency to encode for a high bit(1).", h => high_freq = int.Parse(h) },
                {"a=|amplitude=", "Output waveform's amplitude(0.0-1.0).", a => amplitude = int.Parse(a) },
                {"i=|input=", "Input file for modulation.", i => input_filename = i }
            };

            string output_filename = set.Parse(args).LastOrDefault();

            if(string.IsNullOrWhiteSpace(output_filename) || string.IsNullOrWhiteSpace(input_filename))
            {
                if (string.IsNullOrWhiteSpace(output_filename))
                    Console.WriteLine("Invalid filename {0}.", output_filename);
                if (string.IsNullOrWhiteSpace(input_filename))
                    Console.WriteLine("Invalid filename {0}.", input_filename);

                return;
            }

            WaveGenerator gen = new WaveGenerator() { Amplitude = amplitude };
            WaveFile file = new WaveFile(output_filename);
            file.FrameBuffer.SetLength(0);

            file.Format = format;
            file.SampleRate = (uint)sample_rate;
            file.BitDepth = (uint)bit_depth;

            DataReader reader = new DataReader(File.ReadAllBytes(input_filename));

            if (data_rate > low_freq)
            {
                Console.WriteLine("{0} Hz can encode {0} baud at most. Limiting data rate to {0} baud.", low_freq);
                data_rate = low_freq;
                Console.WriteLine("Hint: to encode data at X baud, you need at least X Hz on the low bit frequency.");
            }

            reader.DataRate = data_rate;

            Console.WriteLine("Data density: {0} samples per bit({1} bits/s)", reader.SamplesPerBit, reader.SampleRate / reader.SamplesPerBit);
            Console.WriteLine("Going to write {0} samples({1:0.00} seconds) of audio.", reader.SamplesPerBit * 8 * reader.Data.Length, (reader.SamplesPerBit * 8 * reader.Data.Length) / (double)sample_rate);

            int x = Console.CursorLeft;
            int y = Console.CursorTop;

            for(int i = 0; reader.WrapCount < 1; i++)
            {
                gen.Frequency = reader.Step() == 1 ? high_freq : low_freq;
                file.WriteFrame(gen.Step());
                if (i % sample_rate == 0)
                {
                    Console.WriteLine("{0} seconds...", i / sample_rate);
                    Console.SetCursorPosition(x, y);
                }
            }

            file.Save();
            Console.WriteLine("\ndone!");
        }

        static void ShowHelp(OptionSet set)
        {
            set.WriteOptionDescriptions(Console.Out);
            Environment.Exit(0);
        }
    }
}