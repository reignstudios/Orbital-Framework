# Orbital-Framework (Prototyping faze)
Graphics / Video, Audio and Input frameworks. (Agnostic / Portable / Easy / Powerful / Fast)

## Cloning source
### Clone new repo
* git clone --recursive https://github.com/reignstudios/Orbital-Framework.git<br>

### Updating existing cloned repos:
* git pull
* git submodule update --init --recursive

## Project structure layout
* "Platforms": <b>OS / API targets</b>
* "Platforms/Shared": <b>Code shared accross platforms</b>
* "Platforms/\<Platform\>": <b>C# projects specifically setup for different host enviroments</b>
* "Platforms/\<Platform\>/NetCore": <b>.NET Core runtime targets</b>
* "Platforms/\<Platform\>/NetFramework": <b>.NET/Mono Framework runtime targets</b>
* "Platforms/\<Platform\>/CS2X": <b>CS2X runtime targets</b>

## Goals
This project will focus on an extremely portable, fast and powerful yet simple to understand agnostic set of frameworks for use in C# / .NET or CS2X runtimes.<br>

Simply put, the goal is to have a robust set of core frameworks that can be used as the building blocks for C# UI / XAML like systems, Game Engines and more... running on platforms ranging from Desktop, Mobile, TV, IoT, Web to embedded or legacy devices.

### Platform target overview
This list will be adjusted and more specific as things progress.<br>
* Win10, 8, 7, Vista, XP, 2000, 98, 95, 3.1, WinCE
* macOSX, macOS9, 8, 7, 6, etc...
* Linux, BSD
* Android, iOS, Tizen, BB10, WP7, WP8, WP10
* WASM, asm.js, PNACL, JSIL
* N64, Xbox, Xbox 360, Xbox One, PS2, PS3, PS4, Dreamcast, etc...
* Others legacy...

### Video / Graphics
Agnostic custom shader support will be provided via CS2X: https://github.com/reignstudios/CS2X<br>
This means all shaders can be writen in C# for portability.<br>
Writing shaders will not be required as many shading presets will be provided and fixed rendering pipelines are supported.<br>

* D3D (Fixed-Pipeline) 5, 6, 7, etc
* D3D 8 - 12
* OpenGL 1-4
* GLES 1-3
* Vulkan
* Metal
* QuickDraw / QuickDraw3D
* PSGL
* Software
* Gameduino 1-2
* N64
* Other legacy...

### Audio
API Agnostic DSP effects can be written in C#<br>

* MS: DirectSound
* MS: XAudio2
* POSIX: OpenAL
* Apple: Cocoa
* Apple: Carbon

### Input
* MS: DirectInput
* MS: XInput
* MS: Win16 / Win32 / WinForms / WPF Input
* MS: WinRT XAML Input
* POSIX: HID
* POSIX: X11
* POSIX: Wayland
* POSIX: Mir
* Apple: HID
* Apple: Cocoa
* Apple: Carbon

## Building
* Prerequisites
	* Visual Studios 2019 (VS for Mac / MonoDevelop or VSCode have not yet been tested)
	* .NET Core, .NET Framework or CS2X. Depends on what you're targeting.
* Debugging CS2X Analyzer errors: Open projects after running "CS2X.Ayalizer.Vsix"