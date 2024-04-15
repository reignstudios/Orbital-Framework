#import "Display.h"

void Orbital_Host_Display_GetMainScreen(int* width, int* height)
{
    NSRect frame = [NSScreen.mainScreen frame];
    CGSize size = frame.size;
    *width = size.width;
    *height = size.height;
}

int Orbital_Host_Display_GetAllScreensCount(void)
{
    NSArray<NSScreen*>* screens = NSScreen.screens;
    return (int)screens.count;
}

void Orbital_Host_Display_GetAllScreens(int** widths, int** heights, int count)
{
    NSArray<NSScreen*>* screens = NSScreen.screens;
    NSUInteger screenCount = screens.count;
    if (count > screenCount) count = (int)screenCount;
    for (int i = 0; i < count; ++i)
    {
        NSRect frame = screens[i].frame;
        CGSize size = frame.size;
        *widths[i] = size.width;
        *heights[i] = size.height;
    }
}
