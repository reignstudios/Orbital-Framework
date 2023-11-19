#import "Application.h"

@implementation AppDelegate
- (BOOL)applicationShouldTerminateAfterLastWindowClosed:(NSApplication*)sender;
{
    return YES;
}

- (NSApplicationTerminateReply)applicationShouldTerminate:(NSApplication *)sender
{
    isQuit = true;
    return NSTerminateNow;
}
@end

@implementation Application
- (void)initApplication
{
    // configure base
    app = [NSApplication sharedApplication];
    appDelegate = [AppDelegate new];
    [NSApp setDelegate:appDelegate];
    [NSApp setActivationPolicy:NSApplicationActivationPolicyRegular];
    
    // add quit menu
    id menubar = [NSMenu new];
    id appMenuItem = [NSMenuItem new];
    [menubar addItem:appMenuItem];
    [NSApp setMainMenu:menubar];
    id appMenu = [NSMenu new];
    id quitMenuItem = [[NSMenuItem alloc] initWithTitle:@"Quit" action:@selector(stop:) keyEquivalent:@"q"];
    [appMenu addItem:quitMenuItem];
    [appMenuItem setSubmenu:appMenu];
    
    // activate
    [NSApp activateIgnoringOtherApps:NO];
    [NSApp finishLaunching];
}
@end

// ----------------------------------
// C Link
// ----------------------------------
Application* Orbital_Host_Application_Create(void)
{
    return[Application new];
}

void Orbital_Host_Application_Init(Application* application)
{
    [application initApplication];
}

void Orbital_Host_Application_Run(void)
{
    [NSApp run];
}

int Orbital_Host_Application_RunEvents(Application* application)
{
    //if (application->appDelegate->isQuit) return 0;
    
    NSEvent *event = [NSApp nextEventMatchingMask:NSEventMaskAny untilDate:nil inMode:NSDefaultRunLoopMode dequeue:YES];
    if (event == nil) return 0;
    [NSApp sendEvent:event];
    return 1;
}

void Orbital_Host_Application_Quit(void)
{
    [NSApp terminate:nil];
}
