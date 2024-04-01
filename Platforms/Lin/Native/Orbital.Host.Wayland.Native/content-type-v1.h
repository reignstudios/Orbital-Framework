/* Generated by wayland-scanner 1.22.0 */

#ifndef CONTENT_TYPE_V1_CLIENT_PROTOCOL_H
#define CONTENT_TYPE_V1_CLIENT_PROTOCOL_H

#include <stdint.h>
#include <stddef.h>
#include "wayland-client.h"

#ifdef  __cplusplus
extern "C" {
#endif

/**
 * @page page_content_type_v1 The content_type_v1 protocol
 * @section page_ifaces_content_type_v1 Interfaces
 * - @subpage page_iface_wp_content_type_manager_v1 - surface content type manager
 * - @subpage page_iface_wp_content_type_v1 - content type object for a surface
 * @section page_copyright_content_type_v1 Copyright
 * <pre>
 *
 * Copyright © 2021 Emmanuel Gil Peyrot
 * Copyright © 2022 Xaver Hugl
 *
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice (including the next
 * paragraph) shall be included in all copies or substantial portions of the
 * Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 * </pre>
 */
struct wl_surface;
struct wp_content_type_manager_v1;
struct wp_content_type_v1;

#ifndef WP_CONTENT_TYPE_MANAGER_V1_INTERFACE
#define WP_CONTENT_TYPE_MANAGER_V1_INTERFACE
/**
 * @page page_iface_wp_content_type_manager_v1 wp_content_type_manager_v1
 * @section page_iface_wp_content_type_manager_v1_desc Description
 *
 * This interface allows a client to describe the kind of content a surface
 * will display, to allow the compositor to optimize its behavior for it.
 *
 * Warning! The protocol described in this file is currently in the testing
 * phase. Backward compatible changes may be added together with the
 * corresponding interface version bump. Backward incompatible changes can
 * only be done by creating a new major version of the extension.
 * @section page_iface_wp_content_type_manager_v1_api API
 * See @ref iface_wp_content_type_manager_v1.
 */
/**
 * @defgroup iface_wp_content_type_manager_v1 The wp_content_type_manager_v1 interface
 *
 * This interface allows a client to describe the kind of content a surface
 * will display, to allow the compositor to optimize its behavior for it.
 *
 * Warning! The protocol described in this file is currently in the testing
 * phase. Backward compatible changes may be added together with the
 * corresponding interface version bump. Backward incompatible changes can
 * only be done by creating a new major version of the extension.
 */
extern const struct wl_interface wp_content_type_manager_v1_interface;
#endif
#ifndef WP_CONTENT_TYPE_V1_INTERFACE
#define WP_CONTENT_TYPE_V1_INTERFACE
/**
 * @page page_iface_wp_content_type_v1 wp_content_type_v1
 * @section page_iface_wp_content_type_v1_desc Description
 *
 * The content type object allows the compositor to optimize for the kind
 * of content shown on the surface. A compositor may for example use it to
 * set relevant drm properties like "content type".
 *
 * The client may request to switch to another content type at any time.
 * When the associated surface gets destroyed, this object becomes inert and
 * the client should destroy it.
 * @section page_iface_wp_content_type_v1_api API
 * See @ref iface_wp_content_type_v1.
 */
/**
 * @defgroup iface_wp_content_type_v1 The wp_content_type_v1 interface
 *
 * The content type object allows the compositor to optimize for the kind
 * of content shown on the surface. A compositor may for example use it to
 * set relevant drm properties like "content type".
 *
 * The client may request to switch to another content type at any time.
 * When the associated surface gets destroyed, this object becomes inert and
 * the client should destroy it.
 */
extern const struct wl_interface wp_content_type_v1_interface;
#endif

#ifndef WP_CONTENT_TYPE_MANAGER_V1_ERROR_ENUM
#define WP_CONTENT_TYPE_MANAGER_V1_ERROR_ENUM
enum wp_content_type_manager_v1_error {
	/**
	 * wl_surface already has a content type object
	 */
	WP_CONTENT_TYPE_MANAGER_V1_ERROR_ALREADY_CONSTRUCTED = 0,
};
#endif /* WP_CONTENT_TYPE_MANAGER_V1_ERROR_ENUM */

#define WP_CONTENT_TYPE_MANAGER_V1_DESTROY 0
#define WP_CONTENT_TYPE_MANAGER_V1_GET_SURFACE_CONTENT_TYPE 1


/**
 * @ingroup iface_wp_content_type_manager_v1
 */
#define WP_CONTENT_TYPE_MANAGER_V1_DESTROY_SINCE_VERSION 1
/**
 * @ingroup iface_wp_content_type_manager_v1
 */
#define WP_CONTENT_TYPE_MANAGER_V1_GET_SURFACE_CONTENT_TYPE_SINCE_VERSION 1

/** @ingroup iface_wp_content_type_manager_v1 */
static inline void
wp_content_type_manager_v1_set_user_data(struct wp_content_type_manager_v1 *wp_content_type_manager_v1, void *user_data)
{
	wl_proxy_set_user_data((struct wl_proxy *) wp_content_type_manager_v1, user_data);
}

/** @ingroup iface_wp_content_type_manager_v1 */
static inline void *
wp_content_type_manager_v1_get_user_data(struct wp_content_type_manager_v1 *wp_content_type_manager_v1)
{
	return wl_proxy_get_user_data((struct wl_proxy *) wp_content_type_manager_v1);
}

static inline uint32_t
wp_content_type_manager_v1_get_version(struct wp_content_type_manager_v1 *wp_content_type_manager_v1)
{
	return wl_proxy_get_version((struct wl_proxy *) wp_content_type_manager_v1);
}

/**
 * @ingroup iface_wp_content_type_manager_v1
 *
 * Destroy the content type manager. This doesn't destroy objects created
 * with the manager.
 */
static inline void
wp_content_type_manager_v1_destroy(struct wp_content_type_manager_v1 *wp_content_type_manager_v1)
{
	wl_proxy_marshal_flags((struct wl_proxy *) wp_content_type_manager_v1,
			 WP_CONTENT_TYPE_MANAGER_V1_DESTROY, NULL, wl_proxy_get_version((struct wl_proxy *) wp_content_type_manager_v1), WL_MARSHAL_FLAG_DESTROY);
}

/**
 * @ingroup iface_wp_content_type_manager_v1
 *
 * Create a new content type object associated with the given surface.
 *
 * Creating a wp_content_type_v1 from a wl_surface which already has one
 * attached is a client error: already_constructed.
 */
static inline struct wp_content_type_v1 *
wp_content_type_manager_v1_get_surface_content_type(struct wp_content_type_manager_v1 *wp_content_type_manager_v1, struct wl_surface *surface)
{
	struct wl_proxy *id;

	id = wl_proxy_marshal_flags((struct wl_proxy *) wp_content_type_manager_v1,
			 WP_CONTENT_TYPE_MANAGER_V1_GET_SURFACE_CONTENT_TYPE, &wp_content_type_v1_interface, wl_proxy_get_version((struct wl_proxy *) wp_content_type_manager_v1), 0, NULL, surface);

	return (struct wp_content_type_v1 *) id;
}

#ifndef WP_CONTENT_TYPE_V1_TYPE_ENUM
#define WP_CONTENT_TYPE_V1_TYPE_ENUM
/**
 * @ingroup iface_wp_content_type_v1
 * possible content types
 *
 * These values describe the available content types for a surface.
 */
enum wp_content_type_v1_type {
	/**
	 * no content type applies
	 *
	 * The content type none means that either the application has no
	 * data about the content type, or that the content doesn't fit
	 * into one of the other categories.
	 */
	WP_CONTENT_TYPE_V1_TYPE_NONE = 0,
	/**
	 * photo content type
	 *
	 * The content type photo describes content derived from digital
	 * still pictures and may be presented with minimal processing.
	 */
	WP_CONTENT_TYPE_V1_TYPE_PHOTO = 1,
	/**
	 * video content type
	 *
	 * The content type video describes a video or animation and may
	 * be presented with more accurate timing to avoid stutter. Where
	 * scaling is needed, scaling methods more appropriate for video
	 * may be used.
	 */
	WP_CONTENT_TYPE_V1_TYPE_VIDEO = 2,
	/**
	 * game content type
	 *
	 * The content type game describes a running game. Its content
	 * may be presented with reduced latency.
	 */
	WP_CONTENT_TYPE_V1_TYPE_GAME = 3,
};
#endif /* WP_CONTENT_TYPE_V1_TYPE_ENUM */

#define WP_CONTENT_TYPE_V1_DESTROY 0
#define WP_CONTENT_TYPE_V1_SET_CONTENT_TYPE 1


/**
 * @ingroup iface_wp_content_type_v1
 */
#define WP_CONTENT_TYPE_V1_DESTROY_SINCE_VERSION 1
/**
 * @ingroup iface_wp_content_type_v1
 */
#define WP_CONTENT_TYPE_V1_SET_CONTENT_TYPE_SINCE_VERSION 1

/** @ingroup iface_wp_content_type_v1 */
static inline void
wp_content_type_v1_set_user_data(struct wp_content_type_v1 *wp_content_type_v1, void *user_data)
{
	wl_proxy_set_user_data((struct wl_proxy *) wp_content_type_v1, user_data);
}

/** @ingroup iface_wp_content_type_v1 */
static inline void *
wp_content_type_v1_get_user_data(struct wp_content_type_v1 *wp_content_type_v1)
{
	return wl_proxy_get_user_data((struct wl_proxy *) wp_content_type_v1);
}

static inline uint32_t
wp_content_type_v1_get_version(struct wp_content_type_v1 *wp_content_type_v1)
{
	return wl_proxy_get_version((struct wl_proxy *) wp_content_type_v1);
}

/**
 * @ingroup iface_wp_content_type_v1
 *
 * Switch back to not specifying the content type of this surface. This is
 * equivalent to setting the content type to none, including double
 * buffering semantics. See set_content_type for details.
 */
static inline void
wp_content_type_v1_destroy(struct wp_content_type_v1 *wp_content_type_v1)
{
	wl_proxy_marshal_flags((struct wl_proxy *) wp_content_type_v1,
			 WP_CONTENT_TYPE_V1_DESTROY, NULL, wl_proxy_get_version((struct wl_proxy *) wp_content_type_v1), WL_MARSHAL_FLAG_DESTROY);
}

/**
 * @ingroup iface_wp_content_type_v1
 *
 * Set the surface content type. This informs the compositor that the
 * client believes it is displaying buffers matching this content type.
 *
 * This is purely a hint for the compositor, which can be used to adjust
 * its behavior or hardware settings to fit the presented content best.
 *
 * The content type is double-buffered state, see wl_surface.commit for
 * details.
 */
static inline void
wp_content_type_v1_set_content_type(struct wp_content_type_v1 *wp_content_type_v1, uint32_t content_type)
{
	wl_proxy_marshal_flags((struct wl_proxy *) wp_content_type_v1,
			 WP_CONTENT_TYPE_V1_SET_CONTENT_TYPE, NULL, wl_proxy_get_version((struct wl_proxy *) wp_content_type_v1), 0, content_type);
}

#ifdef  __cplusplus
}
#endif

#endif