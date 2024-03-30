#pragma once
#include "Common.h"

typedef struct Application
{
    struct wl_display* display;
    int running;
    int useClientDecorations;

    // cursor
    struct wl_cursor_theme* cursorTheme;
    struct wl_surface *cursorSurface;

    // interfaces
    struct wl_compositor* compositor;
    struct wl_subcompositor* subCompositor;
    struct wl_seat* seat;
    struct wl_shm* shm;
    struct xdg_wm_base* wmBase;
    struct zxdg_decoration_manager_v1* decoration;
}Application;

struct Application* Orbital_Host_Wayland_Application_Create();
void Orbital_Host_Wayland_Application_Shutdown(struct Application* application);
int Orbital_Host_Wayland_Application_Init(struct Application* application);