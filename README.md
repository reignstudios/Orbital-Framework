[![Gitter](https://badges.gitter.im/ReignStudios/Orbital-Framework.svg)](https://gitter.im/ReignStudios/Orbital-Framework?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

# Orbital-Framework
Graphics / Video, Audio and Input frameworks. (Agnostic / Portable / Easy / Powerful / Fast)

## Goals
This project will focus on an extremely portable, fast and powerful yet simple to understand agnostic set of frameworks for use in C# / .NET or CS2X runtimes.<br>

Simply put, the goal is to have a robust set of core frameworks that can be used as the building blocks for C# UI / XAML like systems, Game Engines and more... running on platforms ranging from Desktop, Mobile, TV, IoT, Web to other embedded or legacy devices.

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

### Audio / Sound
API Agnostic DSP effects can be written in C#<br>

* MS: DirectSound
* MS: XAudio2
* POSIX: OpenAL
* Apple: Cocoa
* Apple: Carbon

### Input / Mouse,Keyboard,GamePads,Touch
* MS: HID
* MS: XInput
* MS: DirectInput
* MS: Windows.Gaming.Input
* MS: WinRT XAML Input
* MS: Win16 / Win32 / WinForms / WPF Input
* POSIX: HID
* POSIX: X11
* POSIX: Wayland
* POSIX: Mir
* Apple: HID
* Apple: Cocoa
* Apple: Carbon

### Framework should compile with IL2X runtime (for portability & speed)
* A .NET IL => C89 (or other targets): https://github.com/reignstudios/IL2X

## Building
* Prerequisites (Depends on what you're targeting)
	* IDE: Visual Studios, Rider, VSCode, MonoDevelop, VS for Mac, etc
	* .NET Core, .NET Framework, Mono
* macOS
	* Microsoft Hosts require: ```sudo dotnet workload install macos```