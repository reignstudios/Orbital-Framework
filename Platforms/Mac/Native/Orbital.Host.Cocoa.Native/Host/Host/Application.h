#import "Common.h"

@interface Application : NSObject <NSApplicationDelegate>
{
    @public NSApplication* app;
    @public bool isQuit;
}

- (void)initApplication;
- (void)quitCallback: (id)sender;

// NSApplicationDelegate methods
- (NSApplicationTerminateReply)applicationShouldTerminate:(NSApplication *)sender;
- (void)applicationWillTerminate:(NSNotification *)notification;
@end
