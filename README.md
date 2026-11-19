
# Loopback Recorder

Loopback Recorder is a Windows WPF application for capturing audio from WASAPI playback and input devices. It can record system sound and a microphone at the same time, remove quiet audio, convert recordings, and transcribe each recording with Whisper.

## Features

- Record system playback as `render.wav` using WASAPI loopback capture
- Record from a selected capture device as `capture.wav`
- Select active playback and input devices independently
- Display live audio meter values for selected devices
- Remove samples below a configurable RMS silence threshold
- Convert recordings to AAC (`.mp4`), MP3 (`.mp3`), or WMA (`.wma`)
- Generate timestamped transcription text (`.txt`) with Whisper
- Persist settings between application sessions
- View recording and processing status in the application log

## Requirements

- Windows 10 or later
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
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

The application targets `net10.0-windows7.0` and uses Windows WASAPI APIs.

## Usage

1. Choose a **Render** device for system playback, or leave it set to **None**.
2. Choose a **Capture** device for microphone/input audio, or leave it set to **None**.
3. Click the red record button to start. At least one device must be selected.
4. Click the button again to stop recording.
5. Open **Settings** to configure silence removal, conversion, and transcription.

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

When enabled, audio chunks whose RMS level is below **Silence Threshold** are omitted. The default threshold is `0.01`; the UI supports values from `0` to `0.5`.

### Convert

When enabled, each generated WAV file is converted using Windows Media Foundation to:

- **Aac** - AAC audio stored in an `.mp4` container
- **Mp3** - MP3 audio
- **Wma** - Windows Media Audio

Conversion availability may depend on the codecs installed by Windows.

### Transcribe

When enabled, each generated WAV file is resampled to 16 kHz and transcribed with Whisper.net.

The **Model Name** setting is stored in the application settings and defaults to `ggml-base.bin`. If the named model is missing, the application downloads the base GGML model into a `models` folder next to the timestamped recording folder:

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
