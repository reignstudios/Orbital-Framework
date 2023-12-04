#import "Window.h"

@implementation Window
- (void)initWindow:(int)width :(int)height :(WindowType)type :(WindowStartupPosition)startupPosition
{
    // create window
    window = [NSWindow new];
    [window setBackingType:NSBackingStoreBuffered];
    [window setDelegate:self];
    [window setReleasedWhenClosed:YES];
    
    // configure window type
    switch (type)
    {
        case WindowType_Standard:
            [window setStyleMask:(NSWindowStyleMaskTitled|NSWindowStyleMaskClosable|NSWindowStyleMaskMiniaturizable|NSWindowStyleMaskResizable)];
            break;
            
        case WindowType_Tool:
            [window setStyleMask:(NSWindowStyleMaskTitled|NSWindowStyleMaskClosable)];
            break;
            
        case WindowType_Borderless:
            [window setStyleMask:(NSWindowStyleMaskBorderless)];
            break;
            
        case WindowType_Fullscreen:
            [window setStyleMask:(NSWindowStyleMaskBorderless)];
            break;
    }
    
    // set window size
    [window setContentSize:NSMakeSize(width, height)];
    
    // set window position
    if (startupPosition == WindowStartupPosition_CenterScreen)
    {
        [window center];
    }
    else// default
    {
        NSRect screenFrame = [NSScreen.mainScreen frame];
        CGSize screenSize = screenFrame.size;
        [window setFrameTopLeftPoint:NSMakePoint(20, screenSize.height - 40)];
    }
}

- (void)windowWillClose:(NSNotification*)notification
{
    isClosed = true;
    WindowClosedCallback();
}
@end

// ----------------------------------
// C Link
// ----------------------------------
Window* Orbital_Host_Window_Create(void)
{
    return[Window new];
}

void Orbital_Host_Window_Dispose(Window* window)
{
    [window release];
}

void Orbital_Host_Window_Init(Window* window, int width, int height, WindowType type, WindowStartupPosition startupPosition)
{
    [window initWindow :width :height :type :startupPosition];
}

void Orbital_Host_Window_SetTitle(Window* window, unichar* title, int titleLength)
{
    NSString* string = [NSString stringWithCharacters:title length:titleLength];
    [window->window setTitle:string];
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

void Orbital_Host_Window_GetSize(Window* window, int* width, int* height)
{
    CGSize size = [[window->window contentView] frame].size;
    *width = size.width;
    *height = size.height;
}

void Orbital_Host_Window_SetWindowClosedCallback(Window* window, WindowClosedCallbackMethod funcPtr)
{
    window->WindowClosedCallback = funcPtr;
}
