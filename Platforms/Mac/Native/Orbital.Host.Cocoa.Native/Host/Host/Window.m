#import "Window.h"

@implementation Window
- (void)initWindow
{
    window = [[NSWindow alloc] initWithContentRect:NSMakeRect(100, 100, 320, 240) styleMask:(NSWindowStyleMaskTitled|NSWindowStyleMaskClosable|NSWindowStyleMaskMiniaturizable|NSWindowStyleMaskResizable) backing:NSBackingStoreBuffered defer:NO];
    
    [window setDelegate:self];
    [window cascadeTopLeftFromPoint:NSMakePoint(20,20)];
    [window setTitle: @"TODO Title"];
    [window setReleasedWhenClosed:YES];
}

- (void)windowWillClose:(NSNotification*)notification
{
    isClosed = true;
}
@end

// ----------------------------------
// C Link
// ----------------------------------
Window* Orbital_Host_Window_Create(void)
{
    return[Window new];
}

void Orbital_Host_Window_Init(Window* window)
{
    [window initWindow];
}

void Orbital_Host_Window_Dispose(Window* window)
{
    [window release];
}

void Orbital_Host_Window_Show(Window* window)
{
    [window->window makeKeyAndOrderFront:nil];
}

void Orbital_Host_Window_Close(Window* window)
{
    [window->window close];
}

int Orbital_Host_Window_IsClosed(Window* window)
{
    return window->isClosed ? 1 : 0;
}
