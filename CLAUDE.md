# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Drobit PPK** (Post-Processing Kit) — a WPF desktop application for GNSS post-processing of drone survey data. It integrates camera offset calculations with RTK positioning data, using embedded RTKLIB tools for the actual GNSS computations.

The project is transitioning to open source (branch `Ready_for_OSS`).

## Build Commands

```bash
# Build the full solution (requires MSBuild / Visual Studio with .NET Framework 4.8 targeting pack)
msbuild ControlCenter.sln /p:Configuration=Release /p:Platform="Any CPU"

# Build just the main application
msbuild ControlCenter/ControlCenter.csproj /p:Configuration=Release
```

No CI/CD pipelines, no test projects, no automated test commands exist in this repo.

## Solution Architecture

**11 projects** in `ControlCenter.sln`, mixing .NET Framework 4.5.2–4.8 and .NET Standard 2.0:

### Core Application
- **ControlCenter** — Main WPF app (.NET 4.8, `WinExe`). Assembly name: "Drobit PPK". Entry point is `MainWindow.xaml`.

### Libraries
- **GNSSProcessingLibrary** (.NET 4.5.2) — Core GNSS processing: CAM file parsing (Drobit v1.2/v2.0 formats), RTK output parsing, RINEX conversion, coordinate transformation, spline interpolation
- **Communications** / **CommsDataTypes** (netstandard2.0 + net462) — Communication protocols and data types using Newtonsoft.Json
- **DrobitDataModel** (netstandard2.0) — Shared data model, references PresentationFramework
- **WExtraControlLibrary** (.NET 4.8) — Custom WPF controls: CircularProgressBar, LED indicator, Log viewer, Timeline, Map controls
- **DrobitExtras** (.NET 4.8) — Utilities: CMD execution, masking, enum binding helpers
- **ExifManipulationLibrary** (.NET 4.8) — EXIF metadata via Magick.NET

### Supporting
- **AutoUpdater.NET** (.NET 4.6.1) — Self-update mechanism
- **ZipExtractor** (.NET 4.6.1, `WinExe`) — Standalone ZIP extraction utility, outputs into AutoUpdater.NET resources
- **MobileControlCenter** (Xamarin) — Mobile companion app (Android/iOS/UWP)

## MVVM Pattern

The app follows MVVM with these key classes:

- **ViewModel**: `ProcessingViewModel` extends `BindableModelBase` (custom `INotifyPropertyChanged`). Uses `ICommand` for job management (Add, Save, Execute).
- **Model**: `ProcessJobProject` holds an `ObservableCollection<IWorkJob>`. Job types: `GNSSPostProcessingJob`, `CAMProcessingJob`. Parameters implement `IWorkParameter`.
- **Serialization**: Projects serialize to `.prj` files via Newtonsoft.Json with `TypeNameHandling.Objects` for polymorphic job/parameter types.

## Processing Engine

Located in `ControlCenter/GNSSProcessingEngine/`:
- `CAMFileParser` — Parses Drobit CAM formats (v1.2, v2.0) and DJI files
- `RTKOutputFileParser` — Parses RTK position solutions
- `RINEXConverter` — RINEX format conversion
- `CoordinateChanger` — Coordinate system transformations
- `SplineInterpolator` / `SplineInterpolatorB` — Position interpolation
- `TEQCManager` — RINEX quality control

External executables in `Utils/`: `rnx2rtkp.exe`, `rtkplot.exe`, `crx2rnx.exe`, `convbin.exe` (RTKLIB tools), invoked via `CMDExecutor` class in DrobitExtras.

## Key Dependencies

| Package | Purpose |
|---------|---------|
| MaterialDesignThemes/Colors | WPF Material Design theming |
| Dragablz | Draggable tab UI for job containers |
| Extended.Wpf.Toolkit (Xceed) | AvalonDock, DataGrid controls |
| SharpKml.Core | KML file generation |
| Newtonsoft.Json | JSON serialization (polymorphic) |
| SSH.NET | SSH/SFTP remote operations |
| Magick.NET-Q16-AnyCPU | Image EXIF manipulation |
| Microsoft.Maps.MapControl.WPF | Bing Maps integration |

## Post-Build Behavior

The ControlCenter post-build event copies `Utils/` to the output directory. In Release builds, it also strips debug symbols (`.pdb`), XML docs, and unused localization folders.

## Important Conventions

- Custom licensing system with anti-tamper time checking (recently changed key to differentiate from legacy ControlCenter)
- Value converters are used extensively for XAML binding (`ValueConverterGroup`, `AssemblyVersionConverter`, etc.)
- UI controls in `UserControls/PostProcessingControls/` directory: `GNSSPostProcessingControl`, `CameraProcessingControl`, `JobProjectContainerDragablz`, `JobPlayStopControl`
