
# Loopback Recorder

Loopback Recorder is a Windows WPF application for recording audio from WASAPI playback and input devices. Record system audio, a microphone or other input device, or both in parallel. Optional post-processing can suppress quiet audio buffers, convert the recordings, and create Whisper transcriptions.

## Features

- Capture system playback as `render.wav` using WASAPI loopback
- Record a selected input device as `capture.wav`
- Select playback and input devices independently, with live audio meters
- Optionally omit audio buffers below a configurable RMS silence threshold
- Convert recordings to AAC (`.mp4`), MP3 (`.mp3`), or WMA (`.wma`)
- Generate timestamped Whisper transcriptions in `.txt` files
- Save settings between application sessions and show recording/processing status in the app log

## Requirements

- Windows 10 or later
- [.NET 11 SDK](https://dotnet.microsoft.com/download/dotnet/11.0)
- Visual Studio 2026 or another Windows-compatible IDE for building and running WPF applications

## Build and Run

### Visual Studio

1. Clone the repository.
2. Open `LoopbackRecorder.sln`.
3. Restore the NuGet packages.
4. Build and start the `LoopbackRecorder` project (or press `F5`).

### Command line

```powershell
dotnet restore
dotnet build LoopbackRecorder.sln --configuration Release
dotnet run --project LoopbackRecorder/LoopbackRecorder.csproj
```

The application targets `net11.0-windows7.0` and uses Windows WASAPI APIs.

## Usage

1. Choose an **Output / Speakers** device to record system playback, or leave it set to **None**.
2. Choose an input device to record microphone/input audio, or leave it set to **None**.
3. Click the record button to start; select at least one device.
4. Click the button again to stop recording and begin enabled post-processing.
5. Open **Settings** to configure silence suppression, conversion, and transcription. These options are enabled by default.

A successful recording creates a folder named with a `yyyyMMddHHmmss` timestamp under the application base directory. It may contain:

```text
20261119214453/
├── render.wav
├── render.mp4
├── render.txt
├── capture.wav
├── capture.mp4
└── capture.txt
```

The exact files depend on the selected devices and enabled post-processing options.

## Settings

### Remove Silence

When enabled, each incoming audio buffer is omitted if its calculated RMS level is below **Silence Threshold**. This suppresses quiet buffers during recording (it is not a post-recording trim pass) and can also omit quiet sounds. The default threshold is `0.01`; the UI supports values from `0` to `0.5`.

### Convert

When enabled, each generated WAV file is converted using Windows Media Foundation to:

- **Aac** - AAC audio stored in an `.mp4` container
- **Mp3** - MP3 audio
- **Wma** - Windows Media Audio

Conversion availability may depend on the codecs installed by Windows.

### Transcribe

When enabled, each generated WAV file is resampled to 16 kHz and transcribed with Whisper.net, using automatic language detection.

The **Model Name** setting is stored in the application settings and defaults to `ggml-base.bin`. If the named model file is missing, the application downloads the base GGML model into a `models` folder under the application directory (next to the timestamped recording folders):

```text
application-directory/
├── models/
│   └── ggml-base.bin
└── 20261119214453/
```

The initial model download requires an internet connection and may take time to complete.

## Testing

The solution includes an MSTest project for unit tests:

```powershell
dotnet test LoopbackRecorder.sln --configuration Release
```

## Project Structure

- `LoopbackRecorder/` - WPF application project
  - `ViewModels/` - Application state and commands
  - `Views/` - XAML windows, views, and UI behaviors
  - `Helpers/` - Whisper transcription, conversion, and logging services
  - `Models/` - Device abstractions
  - `Enums/` - Supported conversion formats
  - `Properties/` - Application settings
- `LoopbackRecorder.Tests/` - MSTest unit tests
- `.github/workflows/dotnet.yml` - Windows build and self-contained publish workflow

## Dependencies

- [NAudio](https://github.com/naudio/NAudio) - WASAPI device capture and audio processing
- [Whisper.net](https://github.com/sandrohanea/whisper.net) - Whisper transcription for .NET
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) - MVVM properties and commands
- [MahApps.Metro](https://github.com/MahApps/MahApps.Metro) - WPF UI components
- [Microsoft.Extensions.DependencyInjection](https://learn.microsoft.com/dotnet/core/extensions/dependency-injection) - Application service registration

## License

MIT License.
