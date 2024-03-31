#pragma once
#include "Common.h"
#include "Window.h"

typedef struct Screen
{
    int isPrimary;
    int width, height;
}Screen;

typedef struct Application
{
    struct wl_display* display;
    int running;
    int useClientDecorations;

    // windows
    int windowCount;
    struct Window** windows;

    // cursor
    struct wl_cursor_theme* cursorTheme;
    struct wl_surface *cursorSurface;

    // input devices
    struct wl_pointer *pointer;
    struct wl_keyboard* keyboard;

    // screens/displays capture
    int screenCaptureDone, screenCount;
    struct Screen* screens;

    // interfaces
    struct wl_compositor* compositor;
    struct wl_subcompositor* subCompositor;
    struct wl_seat* seat;
    struct wl_shm* shm;
    struct wl_output* output;
    struct xdg_wm_base* wmBase;
    struct zxdg_decoration_manager_v1* decorationManager;
}Application;

struct Application* Orbital_Host_Wayland_Application_Create();
int Orbital_Host_Wayland_Application_Init(struct Application* app);
void Orbital_Host_Wayland_Application_Shutdown(struct Application* app);
void Orbital_Host_Wayland_Application_Run(struct Application* app);
int Orbital_Host_Wayland_Application_RunEvents(struct Application* app);