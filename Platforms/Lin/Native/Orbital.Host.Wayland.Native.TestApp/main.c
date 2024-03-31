#include <stdio.h>
#include "Window.h"
#include "Application.h"

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
    if (!Orbital_Host_Wayland_Window_Init(window, 320, 240, "com.Reign-Studios.Orbital"))
    {
        printf("Failed: Orbital_Host_Wayland_Window_Init");
        return 0;
    }
    Orbital_Host_Wayland_Window_SetTitle(window, "Test Window");

    // run
    Orbital_Host_Wayland_Application_Run(app);
    //while (1) sleep(1);

    // shutdown
    Orbital_Host_Wayland_Window_Dispose(window);
    Orbital_Host_Wayland_Application_Shutdown(app);

    return 0;
}
