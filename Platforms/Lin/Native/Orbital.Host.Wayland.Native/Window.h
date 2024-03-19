#pragma once
#include "Common.h"
#include "Application.h"

typedef struct SurfaceBuffer
{
    int width, height;
    uint32_t color;
    char* name;
    int fd;
    int stride, size;
    struct wl_shm_pool *pool;
    struct wl_buffer *buffer;
    uint32_t *pixels;
}SurfaceBuffer;

typedef struct Window
{
    struct Application* app;
    struct wl_surface* surface;
    struct wl_surface* clientSurface;

    // mouse
    struct wl_surface* mouseHoverSurface;
    uint32_t mouseHoverSerial;
    int mouseX, mouseY;
    int mouseX_Client, mouseY_Client;
}Window;

struct Window* Orbital_Host_Wayland_Window_Create(struct Application* app);
void Orbital_Host_Wayland_Window_Shutdown(struct Window* window);
int Orbital_Host_Wayland_Window_Init(struct Window* window);