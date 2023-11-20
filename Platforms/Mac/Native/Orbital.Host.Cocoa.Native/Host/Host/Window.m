#import "Window.h"

@implementation Window
- (void)initWindow:(int)x :(int)y :(int)width :(int)height :(bool)center
{
    //window = [[NSWindow alloc] initWithContentRect:NSMakeRect(x, y, width, height) styleMask:(NSWindowStyleMaskTitled|NSWindowStyleMaskClosable|NSWindowStyleMaskMiniaturizable|NSWindowStyleMaskResizable) backing:NSBackingStoreBuffered defer:NO];
    
    window = [NSWindow new];
    
    NSRect screenFrame = [NSScreen.mainScreen frame];
    CGSize screenSize = [NSScreen.mainScreen frame].size;
    [window setContentSize:NSMakeSize(width, height)];
    [window setFrameTopLeftPoint:NSMakePoint(x, screenSize.height - y)];
    
    [window setStyleMask:(NSWindowStyleMaskTitled|NSWindowStyleMaskClosable|NSWindowStyleMaskMiniaturizable|NSWindowStyleMaskResizable)];
    
    [window setBackingType:NSBackingStoreBuffered];
    
    [window setDelegate:self];
    [window setReleasedWhenClosed:YES];
    
    //if (center) [window center];
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

void Orbital_Host_Window_Dispose(Window* window)
{
    [window release];
}

void Orbital_Host_Window_Init(Window* window, int x, int y, int width, int height, int center)
{
    [window initWindow: x :y :width :height :(center != 0 ? true : false)];
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
