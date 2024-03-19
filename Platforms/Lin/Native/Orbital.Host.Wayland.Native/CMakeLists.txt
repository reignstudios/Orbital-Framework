cmake_minimum_required(VERSION 3.27)
project(Orbital_Host_Wayland_Native C)

set(CMAKE_C_STANDARD 11)
set(CMAKE_CXX_COMPILER /bin/g++)
set(CMAKE_C_COMPILER /bin/gcc)

add_library(Orbital_Host_Wayland_Native SHARED
        xdg-shell-client-protocol.c
        xdg-decoration-unstable-v1.c
        Application.c
        Window.c
        Window.h
        Display.c
        Display.h
        Common.h)

target_link_libraries(Orbital_Host_Wayland_Native wayland-client wayland-cursor m)# wayland-egl GL EGL