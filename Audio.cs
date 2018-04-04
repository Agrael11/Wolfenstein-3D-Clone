using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DTest
{
    class Audio
    {
        private bool Looping = false;
        private string File = "";
        public float Volume = 1;

        public Audio(string file, bool looping)
        {
            Looping = looping;
            File = file;
        }

        public void Play()
        {
            NAudio.Wave.AudioFileReader reader = new NAudio.Wave.AudioFileReader(File);
            NAudio.Wave.WaveOutEvent output = new NAudio.Wave.WaveOutEvent();
            output.Volume = Volume;
            output.Init(reader);
            if (Looping)
                output.PlaybackStopped += stopped;
            output.Play();
        }

        private void stopped(object sender, EventArgs e)
        {
            NAudio.Wave.AudioFileReader reader = new NAudio.Wave.AudioFileReader(File);
            NAudio.Wave.WaveOutEvent output = (NAudio.Wave.WaveOutEvent)sender;
            output.Init(reader);
            output.Play();
        }
    }
}
