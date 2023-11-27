#import "Common.h"

typedef enum WindowType
{
    WindowType_Standard,
    WindowType_Tool,
    WindowType_Borderless
}WindowType;

typedef enum WindowStartupPosition
{
    WindowStartupPosition_Default,
    WindowStartupPosition_CenterScreen
}WindowStartupPosition;

@interface Window : NSObject <NSWindowDelegate>
{
    @public NSWindow* window;
    @public bool isClosed;
}

- (void)initWindow:(int)width :(int)height :(WindowType)type :(WindowStartupPosition)startupPosition;

// NSWindowDelegate methods
- (void)windowWillClose:(NSNotification*)notification;
@end
