@echo off
call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\VC\Auxiliary\Build\vcvarsall.bat" x86_amd64
fxc /T vs_5_1 /Fo Shader_D3D12.vs Shader_D3D12.hlsl /E VSMain
fxc /T ps_5_1 /Fo Shader_D3D12.ps Shader_D3D12.hlsl /E PSMain