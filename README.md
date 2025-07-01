# Loopback Recorder

LoopbackRecorder.Desktop is a WPF application for Windows that allows you to record audio from system playback devices (loopback) and capture devices (microphones), with real-time peak monitoring and output to various audio formats.

## Features

- **Record from Playback (Render) and Capture Devices:**  
  Select any active audio device for loopback (system audio) or capture (microphone) recording.

- **Real-Time Peak Monitoring:**  
  Visualize the master peak value for both render and capture devices.

- **Format Conversion:**  
  Recordings are saved as WAV files and can be automatically converted to AAC, MP3, or WMA formats using Media Foundation.

- **Silence Detection:**  
  Silent audio is automatically filtered out based on a configurable threshold.

- **Logging:**  
  All actions and errors are logged with timestamps for easy troubleshooting.

## Requirements

- **Windows 10/11**
- **.NET 9.0**
- **WPF Desktop Runtime**
- **Media Foundation (for format conversion)**
- **NAudio** (included via NuGet)

## Getting Started

1. **Clone the repository:**  git clone https://github.com/EgemenCiftci/Loopback-Recorder.git
2. **Open the solution:**  
   Open the solution file in Visual Studio 2022.

3. **Restore NuGet packages:**  
   Restore the required NuGet packages.

4. **Build the solution:**  
   Build the solution in Visual Studio.

5. **Run the application:**  
   Press `F5` in Visual Studio or run the built executable from `bin\Debug\net9.0-windows`.

## Dependencies

- [CommunityToolkit.Mvvm](https://www.nuget.org/packages/CommunityToolkit.Mvvm)
- [Microsoft.Extensions.DependencyInjection](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection)
- [NAudio](https://www.nuget.org/packages/NAudio)

## Project Structure

- `Views/` - WPF UI components.
- `ViewModels/` - MVVM view models.
- `Models/` - Data models.

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License

This project is licensed under the MIT License.
