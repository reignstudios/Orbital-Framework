#import "Common.h"

@interface AppDelegate : NSObject <NSApplicationDelegate>
{
    @public bool isQuit;
}

- (BOOL)applicationShouldTerminateAfterLastWindowClosed:(NSApplication *)sender;
- (NSApplicationTerminateReply)applicationShouldTerminate:(NSApplication *)sender;
@end

@interface Application : NSObject
{
    @public AppDelegate* appDelegate;
    @public NSApplication* app;
}

- (void)initApplication;
@end
