/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import <Foundation/Foundation.h>
#import "AEAnnouncementViewController.h"
#import "AEWebAnnouncementJsBridge.h"

@class AEAnnouncementViewController;

/**
 * Default implementation of AEAnnouncementViewController.
 *
 * This view controller display an announcement using layout view named `AEDefaultAnnouncementView.xib`.<br>
 * You can change the xib file to your needs as long as you keep view identifier and types.
 *
 * This class is using the Javascript bridge AEWebAnnouncementJsBridge to perform actions when
 * a recognized Javascript function is called inside a web announcement.
 */
@interface AEDefaultAnnouncementViewController : AEAnnouncementViewController<AEWebAnnouncementActionDelegate>

/** Navigation bar displaying announcement's title */
@property(nonatomic, retain) IBOutlet UINavigationBar* titleBar;

/** Text announcement's content goes in this view. */
@property(nonatomic, retain) IBOutlet UITextView* textView;

/** Web announcement's content goes in this view. */
@property(nonatomic, retain) IBOutlet UIWebView* webView;

/** Toolbar containing action and exit buttons */
@property(nonatomic, retain) IBOutlet UIToolbar* toolbar;

/**
 * Javascript bridge
 */
@property(nonatomic, retain) AEWebAnnouncementJsBridge* jsBridge;

@end
