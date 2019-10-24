# Orbital-Framework (Prototyping faze)
Graphics / Video, Audio and Input frameworks. (Agnostic / Portable / Easy / Powerful / Fast)

## Goals
This project will focus on an extremely portable, fast and powerful yet simple to understand agnostic set of frameworks for use in C# / .NET or CS2X runtimes.<br>

Simply put, the goal is to have a robust set of core frameworks that can be used as the building blocks for C# UI / XAML like systems, Game Engines and more... running on platforms ranging from Desktop, Mobile, TV, IoT, Web to embedded or legacy devices.

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
* Apple: Core Audio Kit

### Input
* MS: DirectInput
* MS: XInput
* MS: Win32 / WinForms / WPF Input
* MS: WinRT XAML Input
* POSIX: HID
* POSIX: X11
* POSIX: Wayland
* POSIX: Mir
* Apple: HID
* Apple: Cocoa