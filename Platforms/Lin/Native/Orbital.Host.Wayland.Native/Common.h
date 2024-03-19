#pragma once

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>

#include <sys/mman.h>
#include <fcntl.h>
#include <errno.h>

#include <wayland-util.h>
#include <wayland-client-core.h>
#include <wayland-client-protocol.h>
#include <wayland-client.h>
//#include <wayland-egl.h>
#include <wayland-cursor.h>
#include "xdg-shell-client-protocol.h"
#include "xdg-decoration-unstable-v1.h"
#include <linux/input.h>
#include <math.h>

#define MIN(a, b) (((a) < (b)) ? (a) : (b))