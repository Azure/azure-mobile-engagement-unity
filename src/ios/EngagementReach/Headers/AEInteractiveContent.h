/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import <UIKit/UIKit.h>
#import "AEReachContent.h"

/**
 * Reach content behavior.
 */
typedef NS_ENUM (NSInteger, AEContentBehavior)
{
  /** A reach content that can be displayed at any time. */
  AEContentBehaviorAnyTime = 1,

  /** A reach content that can be displayed only when the application is in background. */
  AEContentBehaviorBackground = 2,

  /** A reach content that can be displayed during application session time. */
  AEContentBehaviorSession = 3,
};

/**
 * Abstract class for reach contents that can be displayed to the end-user.
 */
@interface AEInteractiveContent : AEReachContent
{
  @private
  NSString* _title;
  NSString* _actionLabel;
  NSString* _exitLabel;
  NSMutableArray* _allowedActivities;
  AEContentBehavior _behavior;
  NSString* _notificationTitle;
  NSString* _notificationMessage;
  BOOL _notificationIcon;
  BOOL _notificationCloseable;
  NSString* _notificationImageString;
  UIImage* _notificationImage;
  BOOL _notificationDisplayed;
  BOOL _notificationActioned;
  BOOL _contentDisplayed;
  BOOL _notifiedFromNativePush;
}

/**
 * Test if this content can be notified in the current UI context.
 * @param activity Current activity name, null if no current activity.
 * @result YES if this content can be notified in the current UI context.
 */
- (BOOL)canNotify:(NSString*)activity;

/** Report notification has been displayed */
- (void)displayNotification;

/**
 * Action the notification: this will display the announcement or poll, or will
 * launch the action URL associated to the notification, depending of the content kind.
 */
- (void)actionNotification;

/**
 * Action the notification: this will display the announcement or poll, or will
 * launch the action URL associated to the notification, depending of the content kind.
 * @param launchAction YES to launch associated action, NO to just report the notification action.
 */
- (void)actionNotification:(BOOL)launchAction;

/** Exit this notification. */
- (void)exitNotification;

/** Report content has been displayed */
- (void)displayContent;

/** Set is displayed value */
- (void)setDisplayed:(BOOL)displayed;

/** Set is actioned value */
- (void)setActioned:(BOOL)actioned;

/** Reach content's title */
@property(nonatomic, readonly) NSString* title;

/** The text label of the action button */
@property(nonatomic, readonly) NSString* actionLabel;

/** The text label of the exit button */
@property(nonatomic, readonly) NSString* exitLabel;

/** Reach content behavior (when to display the notification?) */
@property(readonly) AEContentBehavior behavior;

/** Notification's title */
@property(readonly) NSString* notificationTitle;

/** Notification's message */
@property(readonly) NSString* notificationMessage;

/** @result YES if the notification has a resource icon in notification content, NO otherwise */
@property(readonly) BOOL notificationIcon;

/** @result YES if the notification can be closed without looking at the content, NO otherwise */
@property(readonly) BOOL notificationCloseable;

/** @result notification image */
@property(readonly) UIImage* notificationImage;

/** @result YES if the content was notified from a native Apple Push Notification. */
@property BOOL notifiedFromNativePush;

@end