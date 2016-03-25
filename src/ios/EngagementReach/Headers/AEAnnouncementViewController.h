/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "AEContentViewController.h"
#import "AEReachAnnouncement.h"

@class AEReachAnnouncement;

/**
 * Abstract view controller displaying a Engagement announcement.
 *
 * By inheriting from this class you can create your own view controller to display announcements.
 * If you plan to display Web announcements using this controller, make sure to use an object of type
 * AEWebAnnouncementJsBridge
 * as a delegate to your `UIWebView`.
 */
@interface AEAnnouncementViewController : AEContentViewController

/**
 * Init the view controller with the given announcement.
 * Subclasses should re-implement this method.
 * @param anAnnouncement Announcement to display
 */
- (instancetype)initWithAnnouncement:(AEReachAnnouncement*)anAnnouncement;

/**
 * Report the announcement as actioned and dismiss this view controller.
 * Should be called by subclasses when the user clicks on the 'action' button associated to
 * the announcement.
 */
- (void)action;

/**
 * Use this property to store announcement information when the <initWithAnnouncement:>
 * method is called.
 * Subclasses should also read this property to init their subviews.
 */
@property(nonatomic, retain) AEReachAnnouncement* announcement;

@end
