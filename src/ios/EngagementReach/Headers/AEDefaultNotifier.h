/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "AENotifier.h"

#define NOTIFICATION_AREA_VIEW_TAG 36822491

#define NOTIFICATION_ICON_TAG 1
#define NOTIFICATION_TITLE_TAG 2
#define NOTIFICATION_MESSAGE_TAG 3
#define NOTIFICATION_IMAGE_TAG 4
#define NOTIFICATION_BUTTON_TAG 5
#define NOTIFICATION_CLOSE_TAG 6

/**
 *
 * This is the default notifier used by <AEReachModule>.
 *
 * It will display a banner notification inside the current application window or in a dedicated view tag if it exists.
 * By default the banner notification overlay will be presented at the bottom of the screen. If you prefer to display it
 * at the top of screen,
 * edit the provided _`AENotificationView.xib`_ and change the `AutoSizing` property of the main view so it can be kept
 * at the top of its superview.
 *
 * # Some examples of custom notifiers based on this class #
 *
 * ** Using a different nib file: **
 *
 *    #import "AEDefaultNotifier.h"
 *    @interface MyNotifier : AEDefaultNotifier
 *    @end
 *
 *    @implementation MyNotifier
 *    -(NSString*)nibNameForCategory:(NSString*)category
 *    {
 *      return "MyNotificationView";
 *    }
 *    @end
 *
 * ** Creating the notification view programmatically: **
 *
 *    #import "AEDefaultNotifier.h"
 *    @interface MyNotifier : AEDefaultNotifier
 *    @end
 *
 *    @implementation MyNotifier
 *
 *    -(UIView*)notificationViewForContent:(AEInteractiveContent*)content
 *    {
 *      UIView* notificationView = [[UIView alloc] initWithFrame:CGRectMake(0, 0, 250, 80)];
 *      notificationView.backgroundColor = [UIColor redColor];
 *
 *      // Title
 *      UILabel* title = [[UILabel alloc] initWithFrame:CGRectMake(10, 10, 230, 30)];
 *      title.text = content.notificationTitle;
 *      [notificationView addSubview:title];
 *      [title release];
 *
 *      // Notification button
 *      UIButton* button = [[UIButton alloc] initWithFrame:notificationView.frame];
 *      [button addTarget:self action:@selector(onNotificationActioned) forControlEvents:UIControlEventTouchDown];
 *      [notificationView addSubview:button];
 *      [button release];
 *
 *      return [notificationView autorelease];
 *    }
 *    @end
 */
@interface AEDefaultNotifier : NSObject<AENotifier>
{
  @private
  UIImage* _notificationIcon;
  AEInteractiveContent* _content;
  UIView* _containerView;
}

/**
 * Create a default notifier with a given notification icon.
 * The default notification view AENotificationView will be used to display notifications.
 * @param icon Notification icon.
 */
+ (id)notifierWithIcon:(UIImage*)icon;

/**
 * This method is called when a view is requested to display notifications.<br>
 * The default implementation of this class return `NotificationView`, the name of
 * the xib file provided with the reach library.<br>
 * Subclasses can reimplement this method to return another xib filename. The file must be inside the
 * main application bundle and should only contain one view.
 *
 * @param category Associated category. Can be ignored if this notifier handles only one category.
 * @result Nib name for the given category.
 */
- (NSString*)nibNameForCategory:(NSString*)category;

/**
 * This method is called when a new notification view is requested for the given content.
 * By default this method load the view from the nib returned by <nibNameForCategory:>,
 * and prepare it by calling the method <prepareNotificationView:forContent:>.<br>
 * Subclasses can override this method to create a custom view for the given content.
 *
 * @param content Content to be notified. You can use the methods <[AEInteractiveContent notificationTitle]>,
 * <[AEInteractiveContent notificationMessage]>, <[AEInteractiveContent notificationIcon]>,
 * <[AEInteractiveContent notificationCloseable]>, <[AEInteractiveContent notificationImage]> to prepare your view.
 * @result View displaying the notification.
 */
- (UIView*)notificationViewForContent:(AEInteractiveContent*)content;

/**
 * This function is called when the notification view must be prepared, e.g. change texts,
 * icon etc... based on the specified content. This is the responsibility of this method to
 * associate actions to the buttons. At this point the notification is not yet attached to the
 * view hierarchy, so you can not have access to its superview.
 *
 * The provided custom view must contains the following subviews:
 *
 *  - `UIImageView` with tag 1 to display the notification icon
 *  - `UILabel` with tag 2 to display the notification's title
 *  - `UILabel` with tag 3 to display the notification's message
 *  - `UIImageView` with tag 4 to display the additional notification image
 *  - `UIButton` with tag 5 used when the notification is 'actioned'
 *  - `UIButton` with tag 6 used when the notification is 'exited'
 *
 * @param content Content to be notified.
 * @param view View used to display the notification.
 */
- (void)prepareNotificationView:(UIView*)view forContent:(AEInteractiveContent*)content;

/**
 * This function is called when the notification view has been added to a window.
 * By default, this method do nothing but subclasses can override this method to perform additional tasks
 * associated with presenting the notification view.
 * @param view View containing the notification.
 */
- (void)notificationViewDidAppear:(UIView*)view;

/**
 * This function is called when the notification view has been removed from the view hierarchy.
 * By default, this method do nothing but subclasses can override this method to  perform additional tasks
 * associated with hiding the notification view.
 * @param view View containing the notification.
 */
- (void)notificationViewDidDisappear:(UIView*)view;

/**
 * Animate the area notification view when it appears.
 * Default implementation performs a fade-in animation.
 * Subclasses can override this method to customize the animation.
 * @param view View to animate.
 */
- (void)animateAreaNoticationView:(UIView*)view;

/**
 * Animate the overlay notification view when it appears.
 * Default implementation performs a vertical slide animation.
 * Subclasses can override this method to customize the animation.
 * @param view View to animate.
 */
- (void)animateOverlayNotificationView:(UIView*)view;

/**
 * Called when the notification is actioned (example : when the user clicks on the notification).
 * If you override the method <prepareNotificationView:forContent:>, be sure to map this method to one of your U.I.
 * controls.
 */
- (void)onNotificationActioned;

/**
 * Called when the notification is exited (example : when the close button is clicked).
 * If you override the method <prepareNotificationView:forContent:>, be sure to map this method to one of your U.I.
 * controls
 */
- (void)onNotificationExited;

@end
