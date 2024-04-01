#include "Window.h"
#include "Application.h"

#define DECORATIONS_BAR_SIZE 8
#define DECORATIONS_TOPBAR_SIZE 38

Rect CreateRect(int x, int y, int width, int height)
{
    Rect rect;
    rect.x = x;
    rect.y = y;
    rect.width = width;
    rect.height = height;
    return rect;
}

int WithinRect(Rect rect, int x, int y)
{
    return x >= rect.x && x <= (rect.x + rect.width) && y >= rect.y && y <= (rect.y + rect.height);
}

uint32_t ToColor(char r, char g, char b, char a)
{
    uint32_t result = 0;
    char* c = (char*)&result;
    c[0] = b;
    c[1] = g;
    c[2] = r;
    c[3] = a;
    return result;
}

void BlitPoint(uint32_t* pixels, int pixelWidth, int pixelBufferSize, int x, int y, uint32_t color)
{
    int i = x + (y * pixelWidth);
    if (i >= 0 && i < pixelBufferSize) pixels[i] = color;
}

void BlitLine(uint32_t* pixels, int pixelWidth, int pixelBufferSize, int x, int y, int velX, int velY, int stepCount, uint32_t color)
{
    for (int i = 0; i != stepCount; ++i)
    {
        BlitPoint(pixels, pixelWidth, pixelBufferSize, x, y, color);
        x += velX;
        y += velY;
    }
}

void BlitRect(uint32_t* pixels, int pixelWidth, int pixelBufferSize, int x, int y, int width, int height, uint32_t color)
{
    int widthOffset = width + x;
    int heightOffset = height + y;
    for (int yi = y; yi < heightOffset; ++yi)
    {
        for (int xi = x; xi < widthOffset; ++xi)
        {
            BlitPoint(pixels, pixelWidth, pixelBufferSize, xi, yi, color);
        }
    }
}

void DrawButtons(struct Window* window)
{
    uint32_t* pixels = window->surfaceBuffer.pixels;
    int pixelWidth = window->surfaceBuffer.width;
    int pixelBufferSize = window->surfaceBuffer.size;
    uint32_t blackColor = ToColor(0, 0, 0, 255);

    Rect rect = window->clientRect_ButtonClose;
    BlitRect(pixels, pixelWidth, pixelBufferSize, rect.x, rect.y, rect.width, rect.height, ToColor(200, 0, 0, 255));
    BlitLine(pixels, pixelWidth, pixelBufferSize, rect.x + 4, rect.y + 4, 1, 1, 16, blackColor);// cross-right
    BlitLine(pixels, pixelWidth, pixelBufferSize, rect.x + 3, rect.y + 4, 1, 1, 16, blackColor);// cross-right 2
    BlitLine(pixels, pixelWidth, pixelBufferSize, rect.x + 5, rect.y + 4, 1, 1, 16, blackColor);// cross-right 3
    BlitLine(pixels, pixelWidth, pixelBufferSize, rect.x + 18, rect.y + 4, -1, 1, 16, blackColor);// cross-left
    BlitLine(pixels, pixelWidth, pixelBufferSize, rect.x + 17, rect.y + 4, -1, 1, 16, blackColor);// cross-left 2
    BlitLine(pixels, pixelWidth, pixelBufferSize, rect.x + 19, rect.y + 4, -1, 1, 16, blackColor);// cross-left 3

    if (window->type != WindowType_Tool)
    {
        rect = window->clientRect_ButtonMax;
        BlitRect(pixels, pixelWidth, pixelBufferSize, rect.x, rect.y, rect.width, rect.height, ToColor(200, 200, 200, 255));
        BlitLine(pixels, pixelWidth, pixelBufferSize, rect.x + 4, rect.y + 20, 1, 0, 16, blackColor);// bottom
        BlitLine(pixels, pixelWidth, pixelBufferSize, rect.x + 4, rect.y + 4, 1, 0, 16, blackColor);// top
        BlitLine(pixels, pixelWidth, pixelBufferSize, rect.x + 4, rect.y + 5, 1, 0, 16, blackColor);// top 2
        BlitLine(pixels, pixelWidth, pixelBufferSize, rect.x + 4, rect.y + 6, 1, 0, 16, blackColor);// top 3
        BlitLine(pixels, pixelWidth, pixelBufferSize, rect.x + 4, rect.y + 4, 0, 1, 16, blackColor);// left
        BlitLine(pixels, pixelWidth, pixelBufferSize, rect.x + 19, rect.y + 4, 0, 1, 16, blackColor);// right

        rect = window->clientRect_ButtonMin;
        BlitRect(pixels, pixelWidth, pixelBufferSize, rect.x, rect.y, rect.width, rect.height, ToColor(200, 200, 200, 255));
        BlitLine(pixels, pixelWidth, pixelBufferSize, rect.x + 4, rect.y + 20, 1, 0, 16, blackColor);// line
        BlitLine(pixels, pixelWidth, pixelBufferSize, rect.x + 4, rect.y + 19, 1, 0, 16, blackColor);// line 2
        BlitLine(pixels, pixelWidth, pixelBufferSize, rect.x + 4, rect.y + 18, 1, 0, 16, blackColor);// line 3
    }
}

int CreateSurfaceBuffer(struct wl_shm* shm, struct SurfaceBuffer* buffer, struct wl_surface* surface, char* name, uint32_t color)
{
    // get buffer sizes
    int oldSize = buffer->size;
    buffer->stride = buffer->width * sizeof(uint32_t);
    buffer->size = buffer->height * buffer->stride;

    // alloc name if needed
    if (name != NULL)
    {
        if (buffer->name != NULL) free(buffer->name);
        size_t nameSize = strlen(name);
        buffer->name = malloc(nameSize);
        memcpy(buffer->name, name, nameSize);
    }

    // only create new file if needed
    if (buffer->fd < 0)
    {
        buffer->fd = shm_open(buffer->name, O_RDWR | O_CREAT | O_TRUNC, 0600);
        if (buffer->fd < 0 || errno == EEXIST) return 0;
    }

    // set file size
    if (buffer->size > oldSize)// only increase file size or we can get buffer access violations in pool
    {
        int result = ftruncate(buffer->fd, buffer->size);
        if (result < 0 || errno == EINTR) return 0;
    }

    // map memory
    buffer->pixels = mmap(NULL, buffer->size, PROT_READ | PROT_WRITE, MAP_SHARED, buffer->fd, 0);
    shm_unlink(buffer->name);// call after mmap according to docs
    memset(buffer->pixels, color, buffer->width * buffer->height * sizeof(uint32_t));// clear to color

    // create pool
    buffer->pool = wl_shm_create_pool(shm, buffer->fd, buffer->size);
    buffer->buffer = wl_shm_pool_create_buffer(buffer->pool, 0, buffer->width, buffer->height, buffer->stride, WL_SHM_FORMAT_XRGB8888);
    buffer->color = color;

    wl_surface_attach(surface, buffer->buffer, 0, 0);
    return 1;
}

int ResizeSurfaceBuffer(struct wl_shm* shm, struct SurfaceBuffer* buffer, struct wl_surface* surface)
{
    // pre-dispose old buffers
    munmap(buffer->pixels, buffer->size);
    wl_shm_pool_destroy(buffer->pool);
    struct wl_buffer* oldBuffer = buffer->buffer;// dispose after new buffer is created

    // create new buffer
    int result = CreateSurfaceBuffer(shm, buffer, surface, NULL, buffer->color);

    // post-dispose old buffer
    wl_buffer_destroy(oldBuffer);
    return result;
}

void SetWindowSize(struct Window* window, int width, int height)
{
    window->width = width;
    window->height = height;
    if (window->useClientDecorations)
    {
        window->compositeWidth = window->width + (DECORATIONS_BAR_SIZE * 2);
        window->compositeHeight = window->height + (DECORATIONS_BAR_SIZE + DECORATIONS_TOPBAR_SIZE);
        window->surfaceBuffer.width = window->compositeWidth;
        window->surfaceBuffer.height = window->compositeHeight;
        window->clientSurfaceBuffer.width = width;
        window->clientSurfaceBuffer.height = height;

        // CSD rects
        window->clientRect_Drag_TopBar = CreateRect(0, 0, window->compositeWidth, DECORATIONS_TOPBAR_SIZE);

        window->clientRect_Resize_LeftBar = CreateRect(0, 0, DECORATIONS_BAR_SIZE, window->compositeHeight);
        window->clientRect_Resize_RightBar = CreateRect(window->compositeWidth - DECORATIONS_BAR_SIZE, 0, DECORATIONS_BAR_SIZE, window->compositeHeight);
        window->clientRect_Resize_BottomBar = CreateRect(0, window->compositeHeight - DECORATIONS_BAR_SIZE, window->compositeWidth, DECORATIONS_BAR_SIZE);
        window->clientRect_Resize_TopBar = CreateRect(0, 0, window->compositeWidth, DECORATIONS_BAR_SIZE);

        window->clientRect_Resize_TopLeft = CreateRect(0, 0, DECORATIONS_BAR_SIZE, DECORATIONS_BAR_SIZE);
        window->clientRect_Resize_TopRight = CreateRect(window->compositeWidth - DECORATIONS_BAR_SIZE, 0, DECORATIONS_BAR_SIZE, DECORATIONS_BAR_SIZE);
        window->clientRect_Resize_BottomLeft = CreateRect(0, window->compositeHeight - DECORATIONS_BAR_SIZE, DECORATIONS_BAR_SIZE, DECORATIONS_BAR_SIZE);
        window->clientRect_Resize_BottomRight = CreateRect(window->compositeWidth - DECORATIONS_BAR_SIZE, window->compositeHeight - DECORATIONS_BAR_SIZE, DECORATIONS_BAR_SIZE, DECORATIONS_BAR_SIZE);

        int x = window->compositeWidth - (24 + 8);
        int y = 8;
        window->clientRect_ButtonClose = CreateRect(x, y, 24, 24);
        x -= 24 + 4;
        window->clientRect_ButtonMax = CreateRect(x, y, 24, 24);
        x -= 24 + 4;
        window->clientRect_ButtonMin = CreateRect(x, y, 24, 24);
    }
    else
    {
        window->compositeWidth = width;
        window->compositeHeight = height;
        window->surfaceBuffer.width = width;
        window->surfaceBuffer.height = height;
        window->clientSurfaceBuffer.width = -1;
        window->clientSurfaceBuffer.height = -1;
    }
}

void SetMousePos(struct Window* window, wl_fixed_t x, wl_fixed_t y)
{
    if (window->useClientDecorations)
    {
        if (window->mouseHoverSurface == window->surface)
        {
            window->mouseX = wl_fixed_to_int(x);
            window->mouseY = wl_fixed_to_int(y);
        }
        else if (window->mouseHoverSurface == window->clientSurface)
        {
            window->mouseX_Client = wl_fixed_to_int(x);
            window->mouseY_Client = wl_fixed_to_int(y);
        }
    }
    else
    {
        window->mouseX = window->mouseX_Client = wl_fixed_to_int(x);
        window->mouseY = window->mouseY_Client = wl_fixed_to_int(y);
    }
}

void SetCursor(struct Window* window, struct wl_pointer *pointer, uint32_t serial, char* name)
{
    struct wl_cursor* cursor = wl_cursor_theme_get_cursor(window->app->cursorTheme, name);
    if (cursor != NULL)
    {
        struct wl_surface* cursorSurface = window->app->cursorSurface;
        struct wl_cursor_image* image = cursor->images[0];
        wl_pointer_set_cursor(pointer, serial, cursorSurface, image->hotspot_x, image->hotspot_y);
        wl_surface_attach(cursorSurface, wl_cursor_image_get_buffer(image), 0, 0);
        wl_surface_damage(cursorSurface, 0, 0, image->width, image->height);
        wl_surface_commit(cursorSurface);
    }
}

// ==========================================================
// Wayland callbacks
// ==========================================================
void window_pointer_enter(void *data, struct wl_pointer *pointer, uint32_t serial, struct wl_surface *surface, wl_fixed_t x, wl_fixed_t y)
{
    struct Window* window = (struct Window*)data;
    if (surface == window->surface || surface == window->clientSurface)
    {
        window->mouseHoverSurface = surface;
        window->mouseHoverSerial = serial;
        SetMousePos(window, x, y);
        SetCursor(window, pointer, serial, "left_ptr");
    }
}

void window_pointer_leave(void *data, struct wl_pointer *pointer, uint32_t serial, struct wl_surface *surface)
{
    struct Window* window = (struct Window*)data;
    if (surface == window->surface || surface == window->clientSurface)
    {
        window->mouseHoverSurface = NULL;
        window->mouseHoverSerial = -1;
    }
}

void window_pointer_motion(void *data, struct wl_pointer *pointer, uint32_t time, wl_fixed_t x, wl_fixed_t y)
{
    struct Window* window = (struct Window*)data;
    if (window->mouseHoverSurface == window->surface || window->mouseHoverSurface == window->clientSurface)
    {
        SetMousePos(window, x, y);
    }
}

void window_pointer_button(void *data, struct wl_pointer *pointer, uint32_t serial, uint32_t time, uint32_t button, uint32_t state)
{
    struct Window* window = (struct Window*)data;
    if (!window->useClientDecorations || window->mouseHoverSurface != window->surface) return;
    if (window->type == WindowType_Borderless || window->type == WindowType_Fullscreen) return;

    if (button == BTN_LEFT)
    {
        if (state == WL_POINTER_BUTTON_STATE_PRESSED)
        {
            int mouseX = window->mouseX;
            int mouseY = window->mouseY;
            struct wl_seat* seat = window->app->seat;

            // drag
            if
            (
                !WithinRect(window->clientRect_ButtonClose, mouseX, mouseY) && (window->type == WindowType_Tool || (!WithinRect(window->clientRect_ButtonMax, mouseX, mouseY) && !WithinRect(window->clientRect_ButtonMin, mouseX, mouseY))) &&
                !WithinRect(window->clientRect_Resize_BottomBar, mouseX, mouseY) && !WithinRect(window->clientRect_Resize_TopBar, mouseX, mouseY) && !WithinRect(window->clientRect_Resize_LeftBar, mouseX, mouseY) && !WithinRect(window->clientRect_Resize_RightBar, mouseX, mouseY) &&
                !WithinRect(window->clientRect_Resize_TopLeft, mouseX, mouseY) && !WithinRect(window->clientRect_Resize_TopRight, mouseX, mouseY) && !WithinRect(window->clientRect_Resize_BottomLeft, mouseX, mouseY) && !WithinRect(window->clientRect_Resize_BottomRight, mouseX, mouseY)
            )
            {
                if (WithinRect(window->clientRect_Drag_TopBar, mouseX, mouseY)) xdg_toplevel_move(window->xdgToplevel, seat, serial);
            }

            else if (window->type == WindowType_Standard)
            {
                // resize corners
                if (WithinRect(window->clientRect_Resize_TopLeft, mouseX, mouseY))
                {
                    xdg_toplevel_resize(window->xdgToplevel, seat, serial, XDG_TOPLEVEL_RESIZE_EDGE_TOP_LEFT);
                }
                else if (WithinRect(window->clientRect_Resize_TopRight, mouseX, mouseY))
                {
                    xdg_toplevel_resize(window->xdgToplevel, seat, serial, XDG_TOPLEVEL_RESIZE_EDGE_TOP_RIGHT);
                }
                else if (WithinRect(window->clientRect_Resize_BottomLeft, mouseX, mouseY))
                {
                    xdg_toplevel_resize(window->xdgToplevel, seat, serial, XDG_TOPLEVEL_RESIZE_EDGE_BOTTOM_LEFT);
                }
                else if (WithinRect(window->clientRect_Resize_BottomRight, mouseX, mouseY))
                {
                    xdg_toplevel_resize(window->xdgToplevel, seat, serial, XDG_TOPLEVEL_RESIZE_EDGE_BOTTOM_RIGHT);
                }

                // resize edges
                else if (WithinRect(window->clientRect_Resize_BottomBar, mouseX, mouseY))
                {
                    xdg_toplevel_resize(window->xdgToplevel, seat, serial, XDG_TOPLEVEL_RESIZE_EDGE_BOTTOM);
                }
                else if (WithinRect(window->clientRect_Resize_TopBar, mouseX, mouseY))
                {
                    xdg_toplevel_resize(window->xdgToplevel, seat, serial, XDG_TOPLEVEL_RESIZE_EDGE_TOP);
                }
                else if (WithinRect(window->clientRect_Resize_LeftBar, mouseX, mouseY))
                {
                    xdg_toplevel_resize(window->xdgToplevel, seat, serial, XDG_TOPLEVEL_RESIZE_EDGE_LEFT);
                }
                else if (WithinRect(window->clientRect_Resize_RightBar, mouseX, mouseY))
                {
                    xdg_toplevel_resize(window->xdgToplevel, seat, serial, XDG_TOPLEVEL_RESIZE_EDGE_RIGHT);
                }
            }
        }
        else if (state == WL_POINTER_BUTTON_STATE_RELEASED)
        {
            // buttons
            if (WithinRect(window->clientRect_ButtonClose, window->mouseX, window->mouseY))
            {
                window->isClosed = 1;
            }
            else if (window->type == WindowType_Standard)
            {
                if (WithinRect(window->clientRect_ButtonMax, window->mouseX, window->mouseY))
                {
                    if (!window->isMaximized)
                    {
                        xdg_toplevel_set_maximized(window->xdgToplevel);
                    }
                    else
                    {
                        xdg_toplevel_unset_maximized(window->xdgToplevel);
                    }
                }
                else if (WithinRect(window->clientRect_ButtonMin, window->mouseX, window->mouseY))
                {
                    xdg_toplevel_set_minimized(window->xdgToplevel);
                }
            }
        }
    }
}

void window_pointer_axis(void *data, struct wl_pointer *pointer, uint32_t time, uint32_t axis, wl_fixed_t value)
{
    // TODO
}

void xdg_surface_handle_configure(void *data, struct xdg_surface *xdg_surface, uint32_t serial)
{
    xdg_surface_ack_configure(xdg_surface, serial);

    // must commit here
    struct Window* window = (struct Window*)data;
    if (window->useClientDecorations) wl_surface_commit(window->clientSurface);
    wl_surface_commit(window->surface);
    wl_display_flush(window->app->display);
}

void xdg_toplevelconfigure_bounds(void *data, struct xdg_toplevel *xdg_toplevel, int32_t width, int32_t height)
{
    // do nothing...
}

void xdg_toplevel_handle_configure(void *data, struct xdg_toplevel *xdg_toplevel, int32_t width, int32_t height, struct wl_array *states)
{
    struct Window* window = (struct Window*)data;

    int activated = 0;
    int maximized = 0;
    int fullscreen = 0;
    int resizing = 0;
    int floating = 1;
    const uint32_t *state = NULL;
    wl_array_for_each(state, states)
    {
        switch (*state)
        {
            case XDG_TOPLEVEL_STATE_ACTIVATED: activated = 1; break;
            case XDG_TOPLEVEL_STATE_RESIZING: resizing = 1; break;
            case XDG_TOPLEVEL_STATE_MAXIMIZED: maximized = 1; break;
            case XDG_TOPLEVEL_STATE_FULLSCREEN: fullscreen = 1; break;

            case XDG_TOPLEVEL_STATE_TILED_LEFT:
            case XDG_TOPLEVEL_STATE_TILED_RIGHT:
            case XDG_TOPLEVEL_STATE_TILED_TOP:
            case XDG_TOPLEVEL_STATE_TILED_BOTTOM:
                floating = 0;
                break;
        }
    }

    // manage maximized state
    int currentMaximized = window->isMaximized;
    if (!window->isMaximized && maximized)
    {
        window->isMaximized = 1;
        printf("maximized\n");
    }
    else if (window->isMaximized && floating)
    {
        window->isMaximized = 0;
        printf("un-maximized\n");
    }

    // resize window
    if (activated || resizing || maximized || fullscreen || currentMaximized != window->isMaximized)
    {
        if (width >= 100 && height >= 100 && (window->compositeWidth != width || window->compositeHeight != height))
        {
            int clientWidth = width;
            int clientHeight = height;
            if (window->useClientDecorations)
            {
                clientWidth = width - (DECORATIONS_BAR_SIZE * 2);
                clientHeight = height - (DECORATIONS_BAR_SIZE + DECORATIONS_TOPBAR_SIZE);
            }
            SetWindowSize(window, clientWidth, clientHeight);

            if (window->useClientDecorations)
            {
                ResizeSurfaceBuffer(window->app->shm, &window->clientSurfaceBuffer, window->clientSurface);
                wl_surface_damage(window->clientSurface, 0, 0, window->clientSurfaceBuffer.width, window->clientSurfaceBuffer.height);
                wl_surface_commit(window->clientSurface);
            }

            ResizeSurfaceBuffer(window->app->shm, &window->surfaceBuffer, window->surface);
            if (window->useClientDecorations) DrawButtons(window);
            wl_surface_damage(window->surface, 0, 0, window->surfaceBuffer.width, window->surfaceBuffer.height);
            wl_surface_commit(window->surface);

            wl_display_flush(window->app->display);
        }
    }
}

void xdg_toplevel_handle_close(void *data, struct xdg_toplevel *xdg_toplevel)
{
    struct Window* window = (struct Window*)data;
    window->isClosed = 1;
}

void decoration_configure(void *data, struct zxdg_toplevel_decoration_v1 *decoration, enum zxdg_toplevel_decoration_v1_mode mode)
{
    printf("Orbital.Wayland DecorationMode: %d\n", mode);
    struct Window* window = (struct Window*)data;
    window->decorationMode = mode;
}

// ==========================================================
// Window
// ==========================================================
struct Window* Orbital_Host_Wayland_Window_Create(struct Application* app)
{
    struct Window* window = calloc(1, sizeof(Window));
    window->app = app;

    // copy app windows to new list
    struct Window** currentWindows = app->windows;
    app->windows = (struct Window**)malloc((app->windowCount + 1) * sizeof(struct Window*));
    for (int i = 0; i != app->windowCount; ++i)
    {
        app->windows[i] = currentWindows[i];
    }

    // free old window list
    if (currentWindows != NULL) free(currentWindows);

    // add new window
    app->windows[app->windowCount] = window;
    app->windowCount++;

    return window;
}

struct xdg_surface_listener xdg_surface_listener = {.configure = xdg_surface_handle_configure};
struct xdg_toplevel_listener xdg_toplevel_listener = {.configure_bounds = xdg_toplevelconfigure_bounds, .configure = xdg_toplevel_handle_configure, .close = xdg_toplevel_handle_close};
struct zxdg_toplevel_decoration_v1_listener decoration_listener = {.configure = decoration_configure};
int Orbital_Host_Wayland_Window_Init(struct Window* window, int width, int height, char* appID, enum WindowType type)
{
    window->type = type;
    window->useClientDecorations = window->app->useClientDecorations && (type == WindowType_Standard || type == WindowType_Tool);

    // configure buffers
    window->surfaceBuffer.fd = -1;
    window->clientSurfaceBuffer.fd = -1;
    SetWindowSize(window, width, height);

    // create window surface objects
    window->surface = wl_compositor_create_surface(window->app->compositor);

    window->xdgSurface = xdg_wm_base_get_xdg_surface(window->app->wmBase, window->surface);
    xdg_surface_add_listener(window->xdgSurface, &xdg_surface_listener, window);

    window->xdgToplevel = xdg_surface_get_toplevel(window->xdgSurface);
    xdg_toplevel_add_listener(window->xdgToplevel, &xdg_toplevel_listener, window);
    xdg_toplevel_set_app_id(window->xdgToplevel, appID);
    xdg_toplevel_set_min_size(window->xdgToplevel, 100, 100);// window should never go below 100

    // get server-side decorations
    if (!window->useClientDecorations && window->app->decorationManager != NULL)
    {
        window->decoration = zxdg_decoration_manager_v1_get_toplevel_decoration(window->app->decorationManager, window->xdgToplevel);
        zxdg_toplevel_decoration_v1_add_listener(window->decoration, &decoration_listener, window);
    }

    // create surface buffers
    uint32_t color = window->useClientDecorations ? ToColor(127, 127, 127, 255) : ToColor(255, 255, 255, 255);
    if (CreateSurfaceBuffer(window->app->shm, &window->surfaceBuffer, window->surface, "Orbital_Wayland_Surface", color) != 1) return 0;
    if (window->useClientDecorations)
    {
        window->clientSurface = wl_compositor_create_surface(window->app->compositor);
        window->clientSubSurface = wl_subcompositor_get_subsurface(window->app->subCompositor, window->clientSurface, window->surface);
        wl_subsurface_set_desync(window->clientSubSurface);
        wl_subsurface_set_position(window->clientSubSurface, DECORATIONS_BAR_SIZE, DECORATIONS_TOPBAR_SIZE);
        if (CreateSurfaceBuffer(window->app->shm, &window->clientSurfaceBuffer, window->clientSurface, "Orbital_Wayland_ClientSurface", ToColor(255, 255, 255, 255)) != 1) return 0;
        DrawButtons(window);
    }

    return 1;
}

void Orbital_Host_Wayland_Window_Dispose(struct Window* window)
{
    // dispose client surface buffer
    if (window->useClientDecorations)
    {
        munmap(window->clientSurfaceBuffer.pixels, window->clientSurfaceBuffer.size);
        wl_shm_pool_destroy(window->clientSurfaceBuffer.pool);
        wl_buffer_destroy(window->clientSurfaceBuffer.buffer);
    }

    // dispose surface buffer
    munmap(window->surfaceBuffer.pixels, window->surfaceBuffer.size);
    wl_shm_pool_destroy(window->surfaceBuffer.pool);
    wl_buffer_destroy(window->surfaceBuffer.buffer);

    // dipose xdg surfaces
    xdg_toplevel_destroy(window->xdgToplevel);
    xdg_surface_destroy(window->xdgSurface);

    // dispose surfaces
    if (window->clientSurface != NULL) wl_surface_destroy(window->clientSurface);
    if (window->clientSubSurface != NULL) wl_subsurface_destroy(window->clientSubSurface);
    wl_surface_destroy(window->surface);

    // dispose decoration
    if (window->decoration != NULL) zxdg_toplevel_decoration_v1_destroy(window->decoration);

    // finish
    free(window);
}

void Orbital_Host_Wayland_Window_SetTitle(struct Window* window, char* title)
{
    xdg_toplevel_set_title(window->xdgToplevel, title);
}

void Orbital_Host_Wayland_Window_Show(struct Window* window)
{
    // commit surface buffers
    if (window->useClientDecorations)
    {
        wl_surface_damage(window->clientSurface, 0, 0, window->clientSurfaceBuffer.width, window->clientSurfaceBuffer.height);
        wl_surface_commit(window->clientSurface);
    }
    wl_surface_damage(window->surface, 0, 0, window->surfaceBuffer.width, window->surfaceBuffer.height);
    wl_surface_commit(window->surface);
    wl_display_flush(window->app->display);
}

void Orbital_Host_Wayland_Window_GetSize(struct Window* window, int* width, int* height)
{
    *width = window->width;
    *height = window->height;
}

int Orbital_Host_Wayland_Window_IsClosed(struct Window* window)
{
    return window->isClosed;
}