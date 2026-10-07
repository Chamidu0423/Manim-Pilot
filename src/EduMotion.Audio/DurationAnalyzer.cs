using System.Text;

namespace EduMotion.Audio;

public static class DurationAnalyzer
{
    /// <summary>
    /// Reads exact duration from standard RIFF/WAV file header.
    /// Returns 0 if file does not exist or format is invalid.
    /// </summary>
    public static double GetWavDuration(string filePath)
    {
        if (!File.Exists(filePath)) return 0.0;

        try
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new BinaryReader(fs);

            // Read RIFF header
            string riff = new string(reader.ReadChars(4));
            if (riff != "RIFF") return 0.0;

            uint fileSize = reader.ReadUInt32();
            string wave = new string(reader.ReadChars(4));
            if (wave != "WAVE") return 0.0;

            uint byteRate = 0;
            uint dataSize = 0;

            while (fs.Position < fs.Length)
            {
                string chunkId = new string(reader.ReadChars(4));
                uint chunkSize = reader.ReadUInt32();

                if (chunkId == "fmt ")
                {
                    ushort audioFormat = reader.ReadUInt16();
                    ushort numChannels = reader.ReadUInt16();
                    uint sampleRate = reader.ReadUInt32();
                    byteRate = reader.ReadUInt32();
                    ushort blockAlign = reader.ReadUInt16();
                    ushort bitsPerSample = reader.ReadUInt16();

                    // Skip any extra bytes in fmt chunk
                    if (chunkSize > 16)
                    {
                        reader.BaseStream.Seek(chunkSize - 16, SeekOrigin.Current);
                    }
                }
                else if (chunkId == "data")
                {
                    dataSize = chunkSize;
                    break;
                }
                else
                {
                    // Skip other chunks (e.g. LIST, JUNK, etc.)
                    reader.BaseStream.Seek(chunkSize, SeekOrigin.Current);
                }
            }

            if (byteRate > 0 && dataSize > 0)
            {
                return (double)dataSize / byteRate;
            }
        }
        catch
        {
            // Ignore format reading errors and fallback
        }

        return 0.0;
    }

    /// <summary>
    /// Creates a valid synthetic PCM WAV file with specified duration and frequency (for testing/mocking).
    /// </summary>
    public static void CreateSineWaveWav(string outputPath, double durationSeconds, int sampleRate = 44100, short frequency = 440)
    {
        int numSamples = (int)(durationSeconds * sampleRate);
        short channels = 1;
        short bitsPerSample = 16;
        int byteRate = sampleRate * channels * (bitsPerSample / 8);
        short blockAlign = (short)(channels * (bitsPerSample / 8));
        int subChunk2Size = numSamples * channels * (bitsPerSample / 8);
        int chunkSize = 36 + subChunk2Size;

        var dir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        using var fs = new FileStream(outputPath, FileMode.Create);
        using var writer = new BinaryWriter(fs);

        writer.Write(Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(chunkSize);
        writer.Write(Encoding.ASCII.GetBytes("WAVE"));

        // fmt subchunk
        writer.Write(Encoding.ASCII.GetBytes("fmt "));
        writer.Write(16); // subchunk1size for PCM
        writer.Write((short)1); // AudioFormat PCM = 1
        writer.Write(channels);
        writer.Write(sampleRate);
        writer.Write(byteRate);
        writer.Write(blockAlign);
        writer.Write(bitsPerSample);

        // data subchunk
        writer.Write(Encoding.ASCII.GetBytes("data"));
        writer.Write(subChunk2Size);

        // write sine wave samples
        double theta = 2.0 * Math.PI * frequency / sampleRate;
        for (int i = 0; i < numSamples; i++)
        {
            short sample = (short)(Math.Sin(i * theta) * (short.MaxValue * 0.3));
            writer.Write(sample);
        }
    }
}
