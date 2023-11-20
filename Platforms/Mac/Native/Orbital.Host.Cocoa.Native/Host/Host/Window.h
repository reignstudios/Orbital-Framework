#import "Common.h"

@interface Window : NSObject <NSWindowDelegate>
{
    @public NSWindow* window;
    @public bool isClosed;
}

- (void)initWindow:(int)x :(int)y :(int)width :(int)height :(bool)center;

// NSWindowDelegate methods
- (void)windowWillClose:(NSNotification*)notification;
@end
