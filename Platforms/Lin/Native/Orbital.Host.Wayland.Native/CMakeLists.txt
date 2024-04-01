cmake_minimum_required(VERSION 3.27)
project(Orbital_Host_Wayland_Native_Shared C)
project(Orbital_Host_Wayland_Native_Static C)

set(CMAKE_C_STANDARD 11)
set(CMAKE_CXX_COMPILER /bin/g++)
set(CMAKE_C_COMPILER /bin/gcc)

add_library(Orbital_Host_Wayland_Native_Shared SHARED
        xdg-shell-client-protocol.c
        xdg-decoration-unstable-v1.c
        content-type-v1.c
        Application.c
        Window.c
        Window.h
        Display.c
        Display.h
        Common.h
        Display.h)
set_property(TARGET Orbital_Host_Wayland_Native_Shared PROPERTY OUTPUT_NAME Orbital_Host_Wayland_Native)

add_library(Orbital_Host_Wayland_Native_Static STATIC
        xdg-shell-client-protocol.c
        xdg-decoration-unstable-v1.c
        content-type-v1.c
        Application.c
        Window.c
        Window.h
        Display.c
        Display.h
        Common.h)
set_property(TARGET Orbital_Host_Wayland_Native_Static PROPERTY OUTPUT_NAME Orbital_Host_Wayland_Native)

target_link_libraries(Orbital_Host_Wayland_Native_Shared wayland-client wayland-cursor m)# wayland-egl GL EGL
target_link_libraries(Orbital_Host_Wayland_Native_Static wayland-client wayland-cursor m)# wayland-egl GL EGL