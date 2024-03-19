#include "Application.h"

// ==========================================================
// Wayland callbacks
// ==========================================================
void registry_add_object(void *data, struct wl_registry *registry, uint32_t name, const char *interface, uint32_t version)
{
    Application* app = (Application*)data;
    if (!strcmp(interface,wl_compositor_interface.name))
    {
        app->compositor = (struct wl_compositor*)(wl_registry_bind (registry, name, &wl_compositor_interface, 1));
    }
    else if (!strcmp(interface,wl_seat_interface.name))
    {
        app->seat = (struct wl_seat*)(wl_registry_bind (registry, name, &wl_seat_interface, 1));
    }
    else if (strcmp(interface, wl_shm_interface.name) == 0)
    {
        app->shm = (struct wl_shm*)(wl_registry_bind(registry, name, &wl_shm_interface, 1));
    }
    else if (strcmp(interface, xdg_wm_base_interface.name) == 0)
    {
        app->wmBase = (struct xdg_wm_base*)wl_registry_bind(registry, name, &xdg_wm_base_interface, MIN(version, 2));
    }

    // required for CSD
    else if (strcmp(interface, wl_subcompositor_interface.name) == 0)
    {
        app->subCompositor = (struct wl_subcompositor*)(wl_registry_bind(registry, name, &wl_subcompositor_interface, 1));
    }

    // required for SSD
    else if (strcmp(interface, zxdg_decoration_manager_v1_interface.name) == 0)
    {
        app->decoration = wl_registry_bind(registry, name, &zxdg_decoration_manager_v1_interface, 1);
    }
}

void registry_remove_object(void *data, struct wl_registry *registry, uint32_t name)
{
    // do nothing...
}

// ==========================================================
// Application
// ==========================================================
struct Application* Orbital_Host_Wayland_Application_Create()
{
    return calloc(0, sizeof(Application));
}

void Orbital_Host_Wayland_Application_Shutdown(struct Application* app)
{
    if (app != NULL)
    {
        free(app);
    }
}

int Orbital_Host_Wayland_Application_Init(struct Application* app)
{
    // get display
    app->display = wl_display_connect(NULL);
    if(!app->display)
    {
        printf("Cannot connect to Wayland server");
        return 0;
    }

    // que registry
    struct wl_registry *registry = wl_display_get_registry(app->display);
    struct wl_registry_listener registry_listener = {&registry_add_object, &registry_remove_object};
    wl_registry_add_listener(registry, &registry_listener, app);
    wl_display_roundtrip(app->display);
    app->useClientDecorations = (app->decoration == NULL) ? 1 : 0;

    // validate required interfaces exist
    if (app->compositor == NULL || app->seat == NULL || app->shm == NULL || app->wmBase == NULL)
    {
        return 0;
    }

    // get cursor theme
    app->cursorTheme = wl_cursor_theme_load(NULL, 32, app->shm);
    app->cursorSurface = wl_compositor_create_surface(app->compositor);

    return 1;
}