#pragma once
#include "Common.h"

typedef struct Application Application;

typedef struct Rect
{
    int x, y, width, height;
}Rect;

enum WindowType
{
    WindowType_Standard,
    WindowType_Tool,
    WindowType_Borderless,
    WindowType_Fullscreen,
    ENUM_BIT
};

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
    enum WindowType type;
    int useClientDecorations;
    int isClosed, isMaximized;
    int width, height;
    int compositeWidth, compositeHeight;

    // content type
    enum wp_content_type_v1_type contentTypeType;
    struct wp_content_type_v1* contentType;

    // primary surface data
    struct xdg_toplevel* xdgToplevel;
    struct xdg_surface* xdgSurface;
    struct wl_surface* surface;
    struct SurfaceBuffer surfaceBuffer;

    // CSD surface data
    struct wl_surface* clientSurface;
    struct wl_subsurface* clientSubSurface;
    struct SurfaceBuffer clientSurfaceBuffer;

    struct Rect clientRect_Drag_TopBar;
    struct Rect clientRect_Resize_LeftBar, clientRect_Resize_RightBar, clientRect_Resize_BottomBar, clientRect_Resize_TopBar;
    struct Rect clientRect_Resize_BottomLeft, clientRect_Resize_BottomRight, clientRect_Resize_TopLeft, clientRect_Resize_TopRight;
    struct Rect clientRect_ButtonMin, clientRect_ButtonMax, clientRect_ButtonClose;

    // SSD
    struct zxdg_toplevel_decoration_v1* decoration;
    enum zxdg_toplevel_decoration_v1_mode decorationMode;

    // mouse
    struct wl_surface* mouseHoverSurface;
    uint32_t mouseHoverSerial;
    int mouseX, mouseY;
    int mouseX_Client, mouseY_Client;
}Window;

void window_pointer_enter(void *data, struct wl_pointer *pointer, uint32_t serial, struct wl_surface *surface, wl_fixed_t x, wl_fixed_t y);
void window_pointer_leave(void *data, struct wl_pointer *pointer, uint32_t serial, struct wl_surface *surface);
void window_pointer_motion(void *data, struct wl_pointer *pointer, uint32_t time, wl_fixed_t x, wl_fixed_t y);
void window_pointer_button(void *data, struct wl_pointer *pointer, uint32_t serial, uint32_t time, uint32_t button, uint32_t state);
void window_pointer_axis(void *data, struct wl_pointer *pointer, uint32_t time, uint32_t axis, wl_fixed_t value);

struct Window* Orbital_Host_Wayland_Window_Create(struct Application* app);
int Orbital_Host_Wayland_Window_Init(struct Window* window, int width, int height, char* appID, enum WindowType type, enum wp_content_type_v1_type contentType);
void Orbital_Host_Wayland_Window_Dispose(struct Window* window);
void Orbital_Host_Wayland_Window_SetTitle(struct Window* window, char* title);
void Orbital_Host_Wayland_Window_Show(struct Window* window);
int Orbital_Host_Wayland_Window_IsClosed(struct Window* window);