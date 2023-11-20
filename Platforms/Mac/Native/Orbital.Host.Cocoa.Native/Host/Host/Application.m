#import "Application.h"

@implementation AppDelegate
/*- (BOOL)applicationShouldTerminateAfterLastWindowClosed:(NSApplication*)sender;
{
    return YES;
}*/

/*- (NSApplicationTerminateReply)applicationShouldTerminate:(NSApplication *)sender
{
    isQuit = true;
    return NSTerminateNow;
}*/
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
    id quitMenuItem = [[NSMenuItem alloc] initWithTitle:@"Quit" action:@selector(quitCallback:) keyEquivalent:@"q"];
    [quitMenuItem setTarget:self];
    [appMenu addItem:quitMenuItem];
    [appMenuItem setSubmenu:appMenu];
    
    // activate
    [NSApp activateIgnoringOtherApps:NO];
    [NSApp finishLaunching];
}

- (void)quitCallback: (id)sender
{
    [NSApp stop:app];
    //[NSApp terminate:app];
    
    //CGEventRef cgEvent = CGEventCreateScrollWheelEvent(NULL, kCGScrollEventUnitLine, 2, 0, 0);
    //CGEventPost(kCGHIDEventTap, cgEvent);
    
    //id e = [NSEvent otherEventWithType:NSEventTypeSystemDefined location:NSMakePoint(0., 0.) modifierFlags:0 timestamp:0 windowNumber:0 context:nil subtype:0 data1:0 data2:0];
    //id e = [NSEvent new];
    //e.type =NSEventTypeSystemDefined;
    //[e type:NSEventTypeSystemDefined];
    //[app postEvent:e atStart:YES];
    
    //[[NSApplication sharedApplication] stop:nil];
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
