using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ethanol
{
    public class WaveGenerator
    {
        public double Amplitude { get; set; }
        public double Phase { get; set; }
        public double Frequency { get; set; }
        public int SampleRate { get; set; }
        public double FreqCounter { get; set; }

        //private double Time = 0;

        public WaveGenerator()
        {
            Amplitude = 1.0;
            Phase = 0;
            Frequency = 1000;
            SampleRate = 44100;
        }

        public double Step()
        {
            FreqCounter += (Frequency / SampleRate);
            var ret =  Math.Sin(
                    //((((Time++) / SampleRate) * ((Frequency + NextFrequency) / 2d)) + Phase) * (2 * Math.PI)
                    ((FreqCounter) + Phase) * (2 * Math.PI)
                ) * Amplitude;
            return ret;
        }

        public void Reset()
        {
            FreqCounter = 0;
        }
    }
}
