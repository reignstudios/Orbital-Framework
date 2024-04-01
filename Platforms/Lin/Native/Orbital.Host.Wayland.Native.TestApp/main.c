#include <stdio.h>
#include "Application.h"
#include "Display.h"

int main()
{
    printf("Hello, World!\n");

    // init app
    struct Application* app = Orbital_Host_Wayland_Application_Create();
    if (!Orbital_Host_Wayland_Application_Init(app))
    {
        printf("Failed: Orbital_Host_Wayland_Application_Init");
        return 0;
    }

    // init window
    struct Window* window = Orbital_Host_Wayland_Window_Create(app);
    if (!Orbital_Host_Wayland_Window_Init(window, 320, 240, "com.Reign-Studios.Orbital", WindowType_Standard, WP_CONTENT_TYPE_V1_TYPE_NONE))
    {
        printf("Failed: Orbital_Host_Wayland_Window_Init");
        return 0;
    }
    Orbital_Host_Wayland_Window_SetTitle(window, "Test Window");
    Orbital_Host_Wayland_Window_Show(window);

    // get displays
    struct Screen screen;
    Orbital_Host_Wayland_Display_GetPrimaryDisplay(app, &screen);

    int screenCount;
    Orbital_Host_Wayland_Display_GetDisplayCount(app, &screenCount);
    struct Screen* screens = malloc(screenCount * sizeof(struct Screen));
    Orbital_Host_Wayland_Display_GetDisplays(app, &screens);

    // run
    Orbital_Host_Wayland_Application_Run(app);

    // shutdown
    Orbital_Host_Wayland_Window_Dispose(window);
    Orbital_Host_Wayland_Application_Shutdown(app);

    return 0;
}
