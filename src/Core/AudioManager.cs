using System;
using System.IO;
using System.Media;

namespace HotVirtualKeyboard.Core
{
    public class AudioManager
    {
        private static readonly Lazy<AudioManager> _instance = new Lazy<AudioManager>(CreateInstance);
        private static AudioManager CreateInstance()
        {
            return new AudioManager();
        }

        public static AudioManager Instance
        {
            get { return _instance.Value; }
        }

        public bool IsSoundEnabled { get; set; }
        public int Volume { get; set; }

        private SoundPlayer _clickPlayer;

        public AudioManager()
        {
            IsSoundEnabled = true;
            Volume = 40; // Default gentle, low volume

            try
            {
                // Soft, subtle, low modern tick (~440Hz, 6ms, fast exponential decay)
                byte[] wavBytes = GenerateSoftClickWav(440, 6);
                _clickPlayer = new SoundPlayer(new MemoryStream(wavBytes));
                _clickPlayer.Load();
            }
            catch
            {
                _clickPlayer = null;
            }
        }

        public void PlayKeySound()
        {
            if (!IsSoundEnabled) return;

            try
            {
                if (_clickPlayer != null)
                {
                    _clickPlayer.Play();
                }
            }
            catch { }
        }

        /// <summary>
        /// Synthesizes a very soft, subtle, low-volume modern keyboard tick.
        /// </summary>
        private static byte[] GenerateSoftClickWav(int frequencyHz, int durationMs)
        {
            int sampleRate = 44100;
            short channels = 1;
            short bitsPerSample = 16;
            int numSamples = (sampleRate * durationMs) / 1000;
            int dataSize = numSamples * channels * (bitsPerSample / 8);

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                // RIFF header
                bw.Write(new char[4] { 'R', 'I', 'F', 'F' });
                bw.Write(36 + dataSize);
                bw.Write(new char[4] { 'W', 'A', 'V', 'E' });

                // fmt chunk
                bw.Write(new char[4] { 'f', 'm', 't', ' ' });
                bw.Write(16);
                bw.Write((short)1); // PCM
                bw.Write(channels);
                bw.Write(sampleRate);
                bw.Write(sampleRate * channels * (bitsPerSample / 8));
                bw.Write((short)(channels * (bitsPerSample / 8)));
                bw.Write(bitsPerSample);

                // data chunk
                bw.Write(new char[4] { 'd', 'a', 't', 'a' });
                bw.Write(dataSize);

                // Low amplitude (~2100 out of 32767) for very quiet, non-punchy tap tick
                double amplitude = 2100;
                for (int i = 0; i < numSamples; i++)
                {
                    double t = (double)i / sampleRate;
                    // Steep exponential decay creates a gentle, subtle tap sound
                    double envelope = Math.Exp(-650.0 * t);
                    double sample = amplitude * envelope * Math.Sin(2.0 * Math.PI * frequencyHz * t);
                    bw.Write((short)sample);
                }

                bw.Flush();
                return ms.ToArray();
            }
        }
    }
}
