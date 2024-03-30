#include "Window.h"

#define DECORATIONS_BAR_SIZE 8
#define DECORATIONS_TOPBAR_SIZE 32
#define DECORATIONS_BUTTON_SIZE 32

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

inline void BlitPoint(uint32_t* pixels, int pixelWidth, int pixelBufferSize, int x, int y, uint32_t color)
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

    int x = window->compositeWidth - (24 + 4);
    Rect rect = window->clientRect_ButtonClose;
    BlitRect(pixels, pixelWidth, pixelBufferSize, rect.x, rect.y, rect.width, rect.height, ToColor(255, 0, 0, 255));
    BlitLine(pixels, pixelWidth, pixelBufferSize, rect.x + 4, rect.y + 4, 1, 1, 16, ToColor(0, 0, 0, 255));// cross-right
    BlitLine(pixels, pixelWidth, pixelBufferSize, rect.x + 3, rect.y + 4, 1, 1, 16, ToColor(0, 0, 0, 255));// cross-right 2
    BlitLine(pixels, pixelWidth, pixelBufferSize, rect.x + 5, rect.y + 4, 1, 1, 16, ToColor(0, 0, 0, 255));// cross-right 3
    BlitLine(pixels, pixelWidth, pixelBufferSize, rect.x + 18, rect.y + 4, -1, 1, 16, ToColor(0, 0, 0, 255));// cross-left
    BlitLine(pixels, pixelWidth, pixelBufferSize, rect.x + 17, rect.y + 4, -1, 1, 16, ToColor(0, 0, 0, 255));// cross-left 2
    BlitLine(pixels, pixelWidth, pixelBufferSize, rect.x + 19, rect.y + 4, -1, 1, 16, ToColor(0, 0, 0, 255));// cross-left 3

    x -= 24 + 4;
    rect = window->clientRect_ButtonMax;
    BlitRect(pixels, pixelWidth, pixelBufferSize, rect.x, rect.y, rect.width, rect.height, ToColor(0, 255, 0, 255));
    BlitLine(pixels, pixelWidth, pixelBufferSize, rect.x + 4, rect.y + 20, 1, 0, 16, ToColor(0, 0, 0, 255));// bottom
    BlitLine(pixels, pixelWidth, pixelBufferSize, rect.x + 4, rect.y + 4, 1, 0, 16, ToColor(0, 0, 0, 255));// top
    BlitLine(pixels, pixelWidth, pixelBufferSize, rect.x + 4, rect.y + 5, 1, 0, 16, ToColor(0, 0, 0, 255));// top 2
    BlitLine(pixels, pixelWidth, pixelBufferSize, rect.x + 4, rect.y + 6, 1, 0, 16, ToColor(0, 0, 0, 255));// top 3
    BlitLine(pixels, pixelWidth, pixelBufferSize, rect.x + 4, rect.y + 4, 0, 1, 16, ToColor(0, 0, 0, 255));// left
    BlitLine(pixels, pixelWidth, pixelBufferSize, rect.x + 19, rect.y + 4, 0, 1, 16, ToColor(0, 0, 0, 255));// right

    x -= 24 + 4;
    rect = window->clientRect_ButtonMin;
    BlitRect(pixels, pixelWidth, pixelBufferSize, rect.x, rect.y, rect.width, rect.height, ToColor(0, 0, 255, 255));
    BlitLine(pixels, pixelWidth, pixelBufferSize, rect.x + 4, rect.y + 20, 1, 0, 16, ToColor(0, 0, 0, 255));// line
    BlitLine(pixels, pixelWidth, pixelBufferSize, rect.x + 4, rect.y + 19, 1, 0, 16, ToColor(0, 0, 0, 255));// line 2
    BlitLine(pixels, pixelWidth, pixelBufferSize, rect.x + 4, rect.y + 18, 1, 0, 16, ToColor(0, 0, 0, 255));// line 3
}

void SetMousePos(struct Window* window, wl_fixed_t x, wl_fixed_t y)
{
    if (window->app->useClientDecorations)
    {
        if (window->mouseHoverSurface == NULL) return;
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
void pointer_enter(void *data, struct wl_pointer *pointer, uint32_t serial, struct wl_surface *surface, wl_fixed_t x, wl_fixed_t y)
{
    struct Window* window = (struct Window*)data;
    window->mouseHoverSurface = surface;
    window->mouseHoverSerial = serial;
    SetMousePos(window, x, y);
    SetCursor(window, pointer, serial, "left_ptr");
}

void pointer_leave(void *data, struct wl_pointer *pointer, uint32_t serial, struct wl_surface *surface)
{
    struct Window* window = (struct Window*)data;
    window->mouseHoverSurface = NULL;
    window->mouseHoverSerial = -1;
}

void pointer_motion(void *data, struct wl_pointer *pointer, uint32_t time, wl_fixed_t x, wl_fixed_t y)
{
    struct Window* window = (struct Window*)data;
    SetMousePos(window, x, y);
}

void pointer_button(void *data, struct wl_pointer *pointer, uint32_t serial, uint32_t time, uint32_t button, uint32_t state)
{
    struct Window* window = (struct Window*)data;
    if (!window->app->useClientDecorations) return;

    if (button == BTN_LEFT)
    {
        if (window->mouseHoverSurface == window->surface)
        {
            if (state == WL_POINTER_BUTTON_STATE_RELEASED)
            {
                // buttons
                if (WithinRect(window->clientRect_ButtonClose, window->mouseX, window->mouseY))
                {
                    window->isClosed = 1;
                }
                else if (WithinRect(window->clientRect_ButtonMax, window->mouseX, window->mouseY))
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
            else if (state == WL_POINTER_BUTTON_STATE_PRESSED)
            {
                int mouseX = window->mouseX;
                int mouseY = window->mouseY;
                struct wl_seat* seat = window->app->seat;

                // drag
                if
                (
                    !WithinRect(window->clientRect_ButtonClose, mouseX, mouseY) && !WithinRect(window->clientRect_ButtonMax, mouseX, mouseY) && !WithinRect(window->clientRect_ButtonMin, mouseX, mouseY) &&
                    !WithinRect(window->clientRect_Resize_BottomBar, mouseX, mouseY) && !WithinRect(window->clientRect_Resize_TopBar, mouseX, mouseY) && !WithinRect(window->clientRect_Resize_LeftBar, mouseX, mouseY) && !WithinRect(window->clientRect_Resize_RightBar, mouseX, mouseY) &&
                    !WithinRect(window->clientRect_Resize_TopLeft, mouseX, mouseY) && !WithinRect(window->clientRect_Resize_TopRight, mouseX, mouseY) && !WithinRect(window->clientRect_Resize_BottomLeft, mouseX, mouseY) && !WithinRect(window->clientRect_Resize_BottomRight, mouseX, mouseY)
                )
                {
                    if (WithinRect(window->clientRect_Drag_TopBar, mouseX, mouseY)) xdg_toplevel_move(window->xdgToplevel, seat, serial);
                }

                // resize corners
                else if (WithinRect(window->clientRect_Resize_TopLeft, mouseX, mouseY))
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
    }
}

void pointer_axis(void *data, struct wl_pointer *pointer, uint32_t time, uint32_t axis, wl_fixed_t value)
{
    // TODO
}

void seat_capabilities(void *data, struct wl_seat *seat, uint32_t capabilities)
{
    Application* app = (Application*)data;
    if (capabilities & WL_SEAT_CAPABILITY_POINTER)
    {
        struct wl_pointer *pointer = wl_seat_get_pointer(seat);
        struct wl_pointer_listener pointer_listener = {&pointer_enter, &pointer_leave, &pointer_motion, &pointer_button, &pointer_axis};
        wl_pointer_add_listener(pointer, &pointer_listener, data);
    }

    /*if (capabilities & WL_SEAT_CAPABILITY_KEYBOARD)
    {
        struct wl_keyboard *keyboard = wl_seat_get_keyboard(seat);
        wl_keyboard_add_listener(keyboard, &keyboard_listener, data);
    }*/
}

// ==========================================================
// Window
// ==========================================================
struct Window* Orbital_Host_Wayland_Window_Create(struct Application* app)
{
    struct Window* window = calloc(0, sizeof(Window));
    window->app = app;
    return window;
}

void Orbital_Host_Wayland_Window_Shutdown(struct Window* window)
{
    if (window != NULL)
    {
        // dispose client surface buffer
        if (window->app->useClientDecorations)
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
        if (window->app->useClientDecorations) wl_surface_destroy(window->clientSurface);
        wl_surface_destroy(window->surface);

        // finish
        free(window);
    }
}

int Orbital_Host_Wayland_Window_Init(struct Window* window)
{
    // add seat listener
    struct wl_seat_listener seat_listener = {&seat_capabilities};
    wl_seat_add_listener(window->app->seat, &seat_listener, window);

    return 1;
}