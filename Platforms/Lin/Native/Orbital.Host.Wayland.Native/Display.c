#include "Display.h"

void Orbital_Host_Wayland_Display_GetPrimaryDisplay(struct Application* app, struct Screen* screen)
{
    if (app->screenCount != 0)
    {
        *screen = app->screens[0];
    }
    else
    {
        memset(screen, 0, sizeof(struct Screen));
    }
}

void Orbital_Host_Wayland_Display_GetDisplayCount(struct Application* app, int* screenCount)
{
    *screenCount = app->screenCount;
}

void Orbital_Host_Wayland_Display_GetDisplays(struct Application* app, struct Screen** screens)
{
    for (int i = 0; i != app->screenCount; ++i)
    {
        *screens[i] = app->screens[i];
    }
}