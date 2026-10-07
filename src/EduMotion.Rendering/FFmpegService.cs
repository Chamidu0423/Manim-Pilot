using System.Diagnostics;
using System.Text;
using EduMotion.Core.Models.Timeline;

namespace EduMotion.Rendering;

public class FFmpegMuxOptions
{
    public string VideoFilePath { get; set; } = string.Empty;
    public List<(string AudioPath, double DelaySeconds)> AudioTracks { get; set; } = new();
    public string OutputFilePath { get; set; } = string.Empty;
}

public interface IFFmpegService
{
    bool IsFFmpegAvailable();
    string BuildMuxCommand(FFmpegMuxOptions options);
    Task<bool> MuxAudioVideoAsync(FFmpegMuxOptions options, CancellationToken ct = default);
}

public class FFmpegService : IFFmpegService
{
    private readonly string _ffmpegBinary;

    public FFmpegService(string ffmpegBinary = "ffmpeg")
    {
        _ffmpegBinary = ffmpegBinary;
    }

    public bool IsFFmpegAvailable()
    {
        try
        {
            using var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _ffmpegBinary,
                    Arguments = "-version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            proc.WaitForExit(2000);
            return proc.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public string BuildMuxCommand(FFmpegMuxOptions options)
    {
        var sb = new StringBuilder();
        sb.Append($"{_ffmpegBinary} -y -i \"{options.VideoFilePath}\"");

        for (int i = 0; i < options.AudioTracks.Count; i++)
        {
            sb.Append($" -i \"{options.AudioTracks[i].AudioPath}\"");
        }

        if (options.AudioTracks.Count == 0)
        {
            // Just copy video
            sb.Append($" -c copy \"{options.OutputFilePath}\"");
            return sb.ToString();
        }

        if (options.AudioTracks.Count == 1)
        {
            int delayMs = (int)(options.AudioTracks[0].DelaySeconds * 1000);
            sb.Append($" -filter_complex \"[1:a]adelay={delayMs}|{delayMs}[aout]\" -map 0:v -map \"[aout]\" -c:v copy -c:a aac \"{options.OutputFilePath}\"");
            return sb.ToString();
        }

        // Multi-track delay and amix
        sb.Append(" -filter_complex \"");
        var mixedLabels = new List<string>();
        for (int i = 0; i < options.AudioTracks.Count; i++)
        {
            int inputIndex = i + 1;
            int delayMs = (int)(options.AudioTracks[i].DelaySeconds * 1000);
            sb.Append($"[{inputIndex}:a]adelay={delayMs}|{delayMs}[a{i}];");
            mixedLabels.Add($"[a{i}]");
        }

        sb.Append($"{string.Join("", mixedLabels)}amix=inputs={options.AudioTracks.Count}:normalize=0[aout]\"");
        sb.Append($" -map 0:v -map \"[aout]\" -c:v copy -c:a aac \"{options.OutputFilePath}\"");

        return sb.ToString();
    }

    public async Task<bool> MuxAudioVideoAsync(FFmpegMuxOptions options, CancellationToken ct = default)
    {
        if (!IsFFmpegAvailable())
        {
            return false;
        }

        string cmd = BuildMuxCommand(options);
        // Extract args
        string args = cmd.Substring(_ffmpegBinary.Length).Trim();

        using var proc = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _ffmpegBinary,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        proc.Start();
        await proc.WaitForExitAsync(ct);
        return proc.ExitCode == 0;
    }
}
