/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

@protocol AEWebAnnouncementActionDelegate;

/**
 *
 * Javascript bridge used by web announcements.
 *
 * Create a bridge between javascript methods embed inside a web announcement and objective c methods
 * defined in <AEWebAnnouncementActionDelegate>.<br>
 * The mapping is as follow:
 *  - *engagementReachContent.actionContent()* is mapped to <[AEWebAnnouncementActionDelegate action]>
 *  - *engagementReachContent.exitContent()* is mapped to <[AEWebAnnouncementActionDelegate exit]>
 *
 * This class must be used in association with a `UIWebView`:
 *
 *  [webView setDelegate:[AEWebAnnouncementJsBridge jsBridgeWithDelegate:self]];
 *
 */
@interface AEWebAnnouncementJsBridge : NSObject<UIWebViewDelegate>
{
  @private
  id<AEWebAnnouncementActionDelegate> _delegate;
}

/**
 * Create a bridge between Javascript functions and Objective C methods.<br>
 * Used the returned object as a delegate to an existing `UIWebView`.
 * @param delegate The delegate that will receive reach actions each time a recognized Javascript function is called.
 */
+ (id)jsBridgeWithDelegate:(id<AEWebAnnouncementActionDelegate>)delegate;

@end

/**
 * The `AEWebAnnouncementActionDelegate` protocol defines the methods a delegate of a <AEWebAnnouncementJsBridge> object
 * should implement.
 *
 * Each time a recognized Javascript method is called, the corresponding delegate method will be called.
 * See methods definition for the list of recognized actions.
 */
@protocol AEWebAnnouncementActionDelegate <NSObject>

/**
 * Mapped to the javascript function *engagementReachContent.actionContent()*<br>
 * The announcement has been actioned.
 */
- (void)action;

/**
 * Mapped to the javascript function *engagementReachContent.exitContent()*<br>
 * The announcement has been exited.
 */
- (void)exit;

@end
