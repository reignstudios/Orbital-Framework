cmake_minimum_required(VERSION 3.27)
project(Orbital_Host_Wayland_Native_TestApp C)

set(CMAKE_C_STANDARD 11)

include_directories("../Orbital.Host.Wayland.Native")
link_directories("../Orbital.Host.Wayland.Native/cmake-build-debug")

add_executable(Orbital_Host_Wayland_Native_TestApp main.c)

set(StaticLibPath "libOrbital_Host_Wayland_Native.a")
target_link_libraries(Orbital_Host_Wayland_Native_TestApp PUBLIC ${StaticLibPath} wayland-client wayland-cursor m)
