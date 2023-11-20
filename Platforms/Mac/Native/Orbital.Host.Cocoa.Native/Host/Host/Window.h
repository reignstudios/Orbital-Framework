#import "Common.h"

@interface Window : NSObject <NSWindowDelegate>
{
    @public NSWindow* window;
    @public bool isClosed;
}

- (void)initWindow;

// NSWindowDelegate methods
- (void)windowWillClose:(NSNotification*)notification;
@end
