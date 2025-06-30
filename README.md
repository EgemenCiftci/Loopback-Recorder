# Loopback Recorder

A Windows desktop application for recording audio loopback using .NET 9, WPF, and NAudio.

## Features

- Record system audio (loopback) directly from your Windows device.
- Modern MVVM architecture using CommunityToolkit.Mvvm.
- Dependency injection with Microsoft.Extensions.DependencyInjection.
- Built with .NET 9 and WPF for a responsive desktop experience.

## Getting Started

### Prerequisites

- Windows 10 or later
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Building the Application

1. Clone the repository.
2. Open the solution in Visual Studio 2022.
3. Restore NuGet packages.
4. Build the solution.

### Running the Application

- Press `F5` in Visual Studio or run the built executable from `bin\Debug\net9.0-windows`.

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
