
# Loopback Recorder

LoopbackRecorder is a WPF application for Windows that allows you to record audio from system playback (loopback) and input devices, with optional silence removal, format conversion, and speech-to-text transcription using OpenAI Whisper models.

## Features

- **Record System Audio**: Capture audio from playback (render) and input (capture) devices.
- **Silence Removal**: Optionally skip silent segments during recording.
- **Format Conversion**: Convert recorded WAV files to AAC (MP4), MP3, or WMA formats.
- **Transcription**: Automatically transcribe recordings to text using Whisper models.
- **Device Selection**: Choose from available audio devices for both playback and capture.
- **Settings UI**: Configure options such as silence threshold, output formats, and transcription model.

## Requirements

- Windows 10 or later
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [NAudio](https://github.com/naudio/NAudio) (included via NuGet)
- [Whisper.net](https://github.com/Const-me/Whisper) (included via NuGet)

## Getting Started

1. **Clone the repository**:

2. **Open in Visual Studio 2022**:
- Open `LoopbackRecorder.sln`.

3. **Restore NuGet packages**:
- Visual Studio will restore packages automatically on build.

4. **Build and Run**:
- Press `F5` to build and launch the application.

## Usage

1. **Select Devices**: Choose playback and/or capture devices from the dropdowns.
2. **Start Recording**: Click the record button to begin.
3. **Stop Recording**: Click again to stop. Files are saved with timestamps.
4. **Transcription**: If enabled, a `.txt` file with the transcription will be generated.
5. **Conversion**: If enabled, the recording will be converted to the selected format.

## Configuration

- **Settings**: Access the settings window to adjust silence threshold, enable/disable features, and select the Whisper model.
- **Model File**: The Whisper model file (e.g., `ggml-base.bin`) is downloaded automatically if not present.

## Project Structure

- `ViewModels/`: Application logic and state management.
- `Views/`: WPF UI components.
- `Helpers/`: Utility classes (e.g., transcription, logging).
- `Models/`: Data models (e.g., audio device abstraction).

## License

This project is licensed under the MIT License.  

## Acknowledgments

- [NAudio](https://github.com/naudio/NAudio)
- [Whisper.net](https://github.com/Const-me/Whisper)
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet)
