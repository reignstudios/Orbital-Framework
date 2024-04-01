#include "Application.h"

// ==========================================================
// Wayland callbacks
// ==========================================================
void registry_add_object(void *data, struct wl_registry *registry, uint32_t name, const char *interface, uint32_t version)
{
    Application* app = (Application*)data;
    if (!strcmp(interface,wl_compositor_interface.name))
    {
        app->compositor = (struct wl_compositor*)(wl_registry_bind(registry, name, &wl_compositor_interface, 1));
    }
    else if (!strcmp(interface,wl_seat_interface.name))
    {
        app->seat = (struct wl_seat*)(wl_registry_bind(registry, name, &wl_seat_interface, 1));
    }
    else if (strcmp(interface, wl_shm_interface.name) == 0)
    {
        app->shm = (struct wl_shm*)(wl_registry_bind(registry, name, &wl_shm_interface, 1));
    }
    else if (strcmp(interface, wl_output_interface.name) == 0)
    {
        if (version >= 2) app->output = (struct wl_output*)(wl_registry_bind(registry, name, &wl_output_interface, 2));
    }
    else if (strcmp(interface, xdg_wm_base_interface.name) == 0)
    {
        app->wmBase = (struct xdg_wm_base*)wl_registry_bind(registry, name, &xdg_wm_base_interface, 1);
    }

    // required for CSD
    else if (strcmp(interface, wl_subcompositor_interface.name) == 0)
    {
        app->subCompositor = (struct wl_subcompositor*)(wl_registry_bind(registry, name, &wl_subcompositor_interface, 1));
    }

    // required for SSD
    else if (strcmp(interface, zxdg_decoration_manager_v1_interface.name) == 0)
    {
        app->decorationManager = (struct zxdg_decoration_manager_v1*)wl_registry_bind(registry, name, &zxdg_decoration_manager_v1_interface, 1);
    }

    // content type
    else if (strcmp(interface, wp_content_type_manager_v1_interface.name) == 0)
    {
        app->contentTypeManager = (struct wp_content_type_manager_v1*)wl_registry_bind(registry, name, &wp_content_type_manager_v1_interface, 1);
    }
}

void registry_remove_object(void *data, struct wl_registry *registry, uint32_t name)
{
    // do nothing...
}

void pointer_enter(void *data, struct wl_pointer *pointer, uint32_t serial, struct wl_surface *surface, wl_fixed_t x, wl_fixed_t y)
{
    struct Application* app = (struct Application*)data;
    for (int i = 0; i != app->windowCount; ++i)
    {
        window_pointer_enter(app->windows[i], pointer, serial, surface, x, y);
    }
}

void pointer_leave(void *data, struct wl_pointer *pointer, uint32_t serial, struct wl_surface *surface)
{
    struct Application* app = (struct Application*)data;
    for (int i = 0; i != app->windowCount; ++i)
    {
        window_pointer_leave(app->windows[i], pointer, serial, surface);
    }
}

void pointer_motion(void *data, struct wl_pointer *pointer, uint32_t time, wl_fixed_t x, wl_fixed_t y)
{
    struct Application* app = (struct Application*)data;
    for (int i = 0; i != app->windowCount; ++i)
    {
        window_pointer_motion(app->windows[i], pointer, time, x, y);
    }
}

void pointer_button(void *data, struct wl_pointer *pointer, uint32_t serial, uint32_t time, uint32_t button, uint32_t state)
{
    struct Application* app = (struct Application*)data;
    for (int i = 0; i != app->windowCount; ++i)
    {
        window_pointer_button(app->windows[i], pointer, serial, time, button, state);
    }
}

void pointer_axis(void *data, struct wl_pointer *pointer, uint32_t time, uint32_t axis, wl_fixed_t value)
{
    struct Application* app = (struct Application*)data;
    for (int i = 0; i != app->windowCount; ++i)
    {
        window_pointer_axis(app->windows[i], pointer, time, axis, value);
    }
}

struct wl_pointer_listener pointer_listener = {&pointer_enter, &pointer_leave, &pointer_motion, &pointer_button, &pointer_axis};
void seat_capabilities(void *data, struct wl_seat *seat, uint32_t capabilities)
{
    struct Application* app = (struct Application*)data;

    if (capabilities & WL_SEAT_CAPABILITY_POINTER)
    {
        app->pointer = wl_seat_get_pointer(app->seat);
        wl_pointer_add_listener(app->pointer, &pointer_listener, data);
    }

    if (capabilities & WL_SEAT_CAPABILITY_KEYBOARD)
    {
        app->keyboard = wl_seat_get_keyboard(app->seat);
        //wl_keyboard_add_listener(app->keyboard, &keyboard_listener, data);// TODO
    }
}

void screen_geometry(void *data, struct wl_output *wl_output, int32_t x, int32_t y, int32_t physical_width, int32_t physical_height, int32_t subpixel, const char *make, const char *model, int32_t transform)
{
    // do nothing...
}

void screen_mode(void *data, struct wl_output *wl_output, uint32_t flags, int32_t width, int32_t height, int32_t refresh)
{
    struct Application* app = (struct Application*)data;

    // reset screen capture
    if (app->screenCaptureDone)
    {
        app->screenCaptureDone = 0;
        app->screenCount = 0;
        if (app->screens != NULL)
        {
            free(app->screens);
            app->screens = NULL;
        }
    }

    // capture screen properties
    struct Screen screen;
    screen.isPrimary = app->screenCount == 0;
    screen.width = width;
    screen.height = height;
    app->screenCount++;
    if (app->screens == NULL) app->screens = malloc(app->screenCount * sizeof(struct Screen));
    else app->screens = realloc(app->screens, app->screenCount * sizeof(struct Screen));
    app->screens[app->screenCount - 1] = screen;
}

void screen_done(void *data, struct wl_output *wl_output)
{
    struct Application* app = (struct Application*)data;
    app->screenCaptureDone = 1;
}

void screen_scale(void *data, struct wl_output *wl_output, int32_t factor)
{
    // do nothing...
}

void xdg_wm_base_ping(void *data, struct xdg_wm_base *base, uint32_t serial)
{
    xdg_wm_base_pong(base, serial);
}

// ==========================================================
// Application
// ==========================================================
struct Application* Orbital_Host_Wayland_Application_Create()
{
    return calloc(1, sizeof(Application));
}

struct wl_registry_listener registry_listener = {.global = &registry_add_object, .global_remove = &registry_remove_object};
struct wl_seat_listener seat_listener = {.capabilities = &seat_capabilities};
struct wl_output_listener output_listener = {.geometry = screen_geometry, .mode = &screen_mode, .done = &screen_done, .scale = screen_scale};
struct xdg_wm_base_listener xdg_wm_base_listener = {.ping = xdg_wm_base_ping};
int Orbital_Host_Wayland_Application_Init(struct Application* app)
{
    // get display
    app->display = wl_display_connect(NULL);
    if(!app->display)
    {
        printf("Cannot connect to Wayland server\n");
        return 0;
    }

    // que registry
    struct wl_registry *registry = wl_display_get_registry(app->display);
    wl_registry_add_listener(registry, &registry_listener, app);
    wl_display_roundtrip(app->display);

    // check for SSD
    app->useClientDecorations = (app->decorationManager == NULL) ? 1 : 0;

    // validate required interfaces exist
    if (app->compositor == NULL || app->seat == NULL || app->shm == NULL || app->wmBase == NULL)
    {
        return 0;
    }

    // get cursor theme
    app->cursorTheme = wl_cursor_theme_load(NULL, 32, app->shm);
    app->cursorSurface = wl_compositor_create_surface(app->compositor);

    // add seat listener
    wl_seat_add_listener(app->seat, &seat_listener, app);

    // add screen/output listener
    if (app->output != NULL) wl_output_add_listener(app->output, &output_listener, app);

    // add window manager base listener
    xdg_wm_base_add_listener(app->wmBase, &xdg_wm_base_listener, app);

    // finish
    wl_display_flush(app->display);
    wl_display_dispatch(app->display);// make sure callbacks fire here
    return 1;
}

void Orbital_Host_Wayland_Application_Shutdown(struct Application* app)
{
    // disconnect display
    wl_display_disconnect(app->display);

    // interfaces
    if (app->contentTypeManager != NULL) wp_content_type_manager_v1_destroy(app->contentTypeManager);

    // finish
    app->running = 0;
    free(app);
}

void ProcessClosedWindows(struct Application* app)
{
    for (int i = 0; i < app->windowCount; ++i)
    {
        if (app->windows[i]->isClosed)
        {
            // shift buffer down
            for (int i2 = i; i2 < app->windowCount - 1; ++i2)
            {
                for (int i3 = i2 + 1; i3 < app->windowCount; ++i3)
                {
                    app->windows[i2] = app->windows[i3];
                }
            }

            // decrease size
            --app->windowCount;
            app->windows = (struct Window**)realloc(app->windows, app->windowCount);
        }
    }
}

void Orbital_Host_Wayland_Application_Run(struct Application* app)
{
    app->running = 1;
    while (app->running && app->windowCount != 0)
    {
        if (wl_display_dispatch(app->display) < 0) break;
        ProcessClosedWindows(app);
    }
    ProcessClosedWindows(app);// ensure any cleanup
}

int Orbital_Host_Wayland_Application_RunEvents(struct Application* app)
{
    int result = wl_display_dispatch(app->display);
    ProcessClosedWindows(app);
    return result;
}