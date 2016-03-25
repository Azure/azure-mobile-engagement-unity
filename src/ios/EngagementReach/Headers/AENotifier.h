/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import <Foundation/Foundation.h>
#import "AEInteractiveContent.h"

/**
 * Custom notifier specification.
 *
 * You can define how a content notification is done for a set of categories by implementing this
 * protocol and registering your instances by calling <[AEReachModule registerNotifier:forCategory:]><br/>
 * It is recommended to extend the default implementation: <AEDefaultNotifier> which
 * performs most of the work and has convenient callbacks.
 */
@protocol AENotifier <NSObject>

@required

/**
 * Handle a notification for a content.
 * @param content content to be notified.
 * @result YES to accept the content, NO to postpone the content (like overlay disabled in a
 *         specific context).
 */
- (BOOL)handleNotification:(AEInteractiveContent*)content;

/**
 * Reach module needs to control notification appearance.
 * When this method is called the notifier should clear any displayed notification for the given category.
 * @param category the category to clear. This parameter can be ignored if the notifier handles only one kind of
 * category.
 */
- (void)clearNotification:(NSString*)category;

@end
