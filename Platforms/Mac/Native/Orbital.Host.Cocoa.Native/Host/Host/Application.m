#import "Application.h"

@implementation Application
- (void)initApplication
{
    // configure base
    app = [NSApplication sharedApplication];
    [NSApp setDelegate:self];
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
    isQuit = true;
    [NSApp stop:app];
}

- (NSApplicationTerminateReply)applicationShouldTerminate:(NSApplication *)sender
{
    isQuit = true;
    return NSTerminateCancel;
}

- (void)applicationWillTerminate:(NSNotification *)notification
{
    isQuit = true;
}
@end

// ----------------------------------
// C Link
// ----------------------------------
Application* Orbital_Host_Application_Create(void)
{
    return[Application new];
}

void Orbital_Host_Application_Dispose(Application* application)
{
    [application release];
}

void Orbital_Host_Application_Init(Application* application)
{
    [application initApplication];
}

void Orbital_Host_Application_Run(void)
{
    [NSApp run];
}

int Orbital_Host_Application_RunEvent(void)
{
    NSEvent *event = [NSApp nextEventMatchingMask:NSEventMaskAny untilDate:[NSDate distantFuture] inMode:NSDefaultRunLoopMode dequeue:YES];
    if (event == nil) return 0;
    [NSApp sendEvent:event];
    return 1;
}

void Orbital_Host_Application_Quit(Application* application)
{
    application->isQuit = true;
    [NSApp stop:nil];
}

int Orbital_Host_Application_IsQuit(Application* application)
{
    return application->isQuit ? 1 : 0;
}
