#import "Common.h"

typedef enum WindowType
{
    WindowType_Standard,
    WindowType_Tool,
    WindowType_Borderless,
    WindowType_Fullscreen
}WindowType;

typedef enum WindowStartupPosition
{
    WindowStartupPosition_Default,
    WindowStartupPosition_CenterScreen
}WindowStartupPosition;

typedef void (*WindowClosedCallbackMethod)(void);

@interface Window : NSObject <NSWindowDelegate>
{
    @public NSWindow* window;
    @public bool isClosed;
    @public WindowClosedCallbackMethod WindowClosedCallback;
}

- (void)initWindow:(int)width :(int)height :(WindowType)type :(bool)fullscreenOverlay :(WindowStartupPosition)startupPosition;

// NSWindowDelegate methods
- (void)windowWillClose:(NSNotification*)notification;
@end
