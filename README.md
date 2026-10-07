# ManimPilot Studio (ManimPilot)

> **AI-Powered Mathematical Animation Creator**
> Generates educational Manim animations with deterministic timing, verified mathematics, and voice synchronization (Sinhala & English).

---

## 🌟 Overview

ManimPilot Studio bridges the gap between educational lesson intent and mathematical animation. Rather than letting AI directly emit fragile, arbitrary Python or Manim code, ManimPilot uses a deterministic multi-stage pipeline:

```text
Problem / User Input
        │
        ▼
Mathematical Proof (Deterministic & Verified)
        │
        ▼
Structured Lesson Plan (JSON Schema)
        │
        ▼
Animation IR (Intermediate Representation)
        │
        ▼
Narration Scheduler (Overlap Prevention & Audio Sync)
        │
        ▼
Deterministic Manim Scene Generator
        │
        ▼
Render Pipeline (Quick Preview / 1080p Export / Audio Mux)
```

---

## 🏗️ Architecture

The solution is divided into modular enterprise .NET 8 libraries:

| Project | Role |
|---|---|
| **`ManimPilot.Core`** | Domain models (`LessonModel`, `TimelineModel`, `AnimationIR`, `.ManimPilot` project format) and multi-stage validation rules. |
| **`ManimPilot.Math`** | Deterministic binary/decimal conversion, place-value breakdown, arithmetic evaluator, and anti-hallucination verification. |
| **`ManimPilot.Audio`** | Pluggable `ITtsProvider` abstraction, Gemini TTS integration, WAV header duration analyzer, and overlap-free `NarrationScheduler`. |
| **`ManimPilot.AI`** | Structured Gemini Lesson Planner, AI Copilot, and `SinhalaNaturalizer` for spoken educational phrasing. |
| **`ManimPilot.Rendering`**| Manim Community scene script generator, FFmpeg multiplexer with audio delay filters, and asynchronous render queue. |
| **`ManimPilot.App`** | CLI runner, interactive demo, validation pipeline executor, and timeline inspector. |
| **`ManimPilot.Tests`** | Comprehensive MSTest test suite verifying math proofs, overlap prevention, AI hallucination detection, and serialization. |

---

## 🚀 Quick Start

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- Optional: Python 3 + [Manim Community](https://www.manim.community/) (`pip install manim`)
- Optional: [FFmpeg](https://ffmpeg.org/) (for video & multi-track audio muxing)

### Build and Run Demo

Run the end-to-end binary-to-decimal lesson generator:

```bash
dotnet run --project src/ManimPilot.App/ManimPilot.App.csproj -- demo 10110110
```

### Run Tests

```bash
dotnet test tests/ManimPilot.Tests/ManimPilot.Tests.csproj --no-restore
```

### CLI Commands

```bash
# Generate lesson for any binary number with Sinhala narration
dotnet run --project src/ManimPilot.App/ManimPilot.App.csproj -- new 10110110 si-LK

# Validate an existing .ManimPilot project file
dotnet run --project src/ManimPilot.App/ManimPilot.App.csproj -- validate output/lesson.ManimPilot

# Export generated Manim script and assets
dotnet run --project src/ManimPilot.App/ManimPilot.App.csproj -- export output/lesson.ManimPilot ./renders
```

---

## 🔒 Key Design Guarantees

1. **Zero Mathematical Hallucinations**: Every step is cross-referenced with `BinaryMathService`. If the AI claims `128 + 32 + 16 + 4 + 2 = 181`, the build pipeline immediately halts before rendering with error `M003`.
2. **Audio Overlap Elimination**: The `NarrationScheduler` calculates true speech length and auto-pads visual wait intervals so narrations never speak over one another.
3. **Visual Language vs Spoken Language**: Separates visual LaTeX/equations (`10 + 20 = 30`) from natural spoken educational phrasing (`"දහයයි විස්සයි එකතු කළාම තිහක් ලැබෙනවා"`).
4. **Code Safety**: Manim scripts are emitted deterministically by `ManimGenerator` using safe animation primitives (`FadeIn`, `Transform`, `Write`, `Circumscribe`), prohibiting arbitrary system execution.
