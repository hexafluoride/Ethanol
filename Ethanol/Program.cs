using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ethanol
{
    class Program
    {
        static void Main(string[] args)
        {
            WaveGenerator gen = new WaveGenerator() { Amplitude = 0.25 };
            WaveFile file = new WaveFile("./test.pcm");

            if(args.Length < 4)
            {
                Console.WriteLine("Usage: ethanol <input file name> <baud> <low bit freq> <high bit freq>");
                return;
            }

            DataReader reader = new DataReader(File.ReadAllBytes(args[0]));

            int data_rate = int.Parse(args[1]);
            int low_freq = int.Parse(args[2]);
            int high_freq = int.Parse(args[3]);

            int min_samples = 44100 / low_freq;
            int max_rate = 44100 / min_samples;

            if (data_rate > max_rate)
            {
                Console.WriteLine("{0} Hz can encode {1} baud at most. Limiting data rate to {1} baud.", low_freq, max_rate);
                data_rate = max_rate;
                Console.WriteLine("Hint: to encode data at X baud, you need at least X Hz on the low bit frequency.");
            }

            reader.DataRate = data_rate;

            Console.WriteLine("Data density: {0} samples per bit({1} bits/s)", reader.SamplesPerBit, reader.SampleRate / reader.SamplesPerBit);
            Console.WriteLine("Going to write {0} samples({1:0.00} seconds) of audio.", reader.SamplesPerBit * 8 * reader.Data.Length, (reader.SamplesPerBit * 8 * reader.Data.Length) / 44100d);

            int x = Console.CursorLeft;
            int y = Console.CursorTop;

            for(int i = 0; reader.WrapCount < 1; i++)
            {
                gen.Frequency = reader.Step() == 1 ? high_freq : low_freq;
                file.WriteFrame(gen.Step());
                if (i % 44100 == 0)
                {
                    Console.WriteLine("{0} seconds...", i / 44100);
                    Console.SetCursorPosition(x, y);
                }
            }

            file.Save();

            //Process.Start("ffmpeg", "-f f64le -ar 44100 -ac 1 -i test.pcm -y out.wav").WaitForExit();

            Console.WriteLine("\ndone!");
            Console.WriteLine("You can run ffmpeg -f f64le -ar 44100 -ac 1 -i test.pcm output.wav to convert the raw PCM file to a WAV file.");
            //Console.ReadKey();
        }
    }
}