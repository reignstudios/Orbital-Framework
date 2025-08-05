@echo off
call "C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvarsall.bat" x86_amd64

fxc /T vs_5_0 /Fo Shader_D3D12.vs Shader_D3D12.hlsl /E VSMain
fxc /T ps_5_0 /Fo Shader_D3D12.ps Shader_D3D12.hlsl /E PSMain

fxc /T vs_5_0 /Fo Triangle_D3D12.vs Triangle_D3D12.hlsl /E VSMain
fxc /T ps_5_0 /Fo Triangle_D3D12.ps Triangle_D3D12.hlsl /E PSMain

fxc /T cs_5_0 /Fo Compute_D3D12.cs Compute_D3D12.hlsl /E CSMain