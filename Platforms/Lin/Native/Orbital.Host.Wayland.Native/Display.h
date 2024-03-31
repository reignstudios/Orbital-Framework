#pragma once
#include "Application.h"

void Orbital_Host_Wayland_Display_GetPrimaryDisplay(struct Application* app, struct Screen* screen);
void Orbital_Host_Wayland_Display_GetDisplayCount(struct Application* app, int* screenCount);
void Orbital_Host_Wayland_Display_GetDisplays(struct Application* app, struct Screen** screens);

