#pragma once
#include "Common.h"
#include "Application.h"

typedef struct Rect
{
    int x, y, width, height;
}Rect;

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
    int isClosed, isMaximized;
    int compositeWidth, compositeHeight;

    struct Rect clientRect_Drag_TopBar;
    struct Rect clientRect_Resize_LeftBar, clientRect_Resize_RightBar, clientRect_Resize_BottomBar, clientRect_Resize_TopBar;
    struct Rect clientRect_Resize_BottomLeft, clientRect_Resize_BottomRight, clientRect_Resize_TopLeft, clientRect_Resize_TopRight;
    struct Rect clientRect_ButtonMin, clientRect_ButtonMax, clientRect_ButtonClose;

    struct xdg_toplevel* xdgToplevel;
    struct xdg_surface* xdgSurface;
    struct wl_surface* surface;
    struct SurfaceBuffer surfaceBuffer;

    struct wl_surface* clientSurface;
    struct SurfaceBuffer clientSurfaceBuffer;

    // mouse
    struct wl_surface* mouseHoverSurface;
    uint32_t mouseHoverSerial;
    int mouseX, mouseY;
    int mouseX_Client, mouseY_Client;
}Window;

struct Window* Orbital_Host_Wayland_Window_Create(struct Application* app);
void Orbital_Host_Wayland_Window_Shutdown(struct Window* window);
int Orbital_Host_Wayland_Window_Init(struct Window* window);