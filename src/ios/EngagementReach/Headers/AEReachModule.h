/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "AEModule.h"
#import "AEReachDataPushDelegate.h"
#import "AENotifier.h"

@class AEStorage;
@class AEContentViewController;

/* Export module name */
extern NSString* const kAEReachModuleName;

/* Export reach xml namespace */
extern NSString* const kAEReachNamespace;

/* Export reach default category */
extern NSString* const kAEReachDefaultCategory;

/* Reach module state */
typedef enum _AEReachModuleState
{
  AEReachModuleStateIdle = 1,
  AEReachModuleStateNotifying = 2,
  AEReachModuleStateLoading = 3,
  AEReachModuleStateShowing = 4
} AEReachModuleState;

/**
 * The Reach Engagement module
 *
 * This is the module that manage reach functionalities. It listens push messages thanks to
 * <[AEModule pushMessageReceived:]> and <[AEModule displayPushMessageNotification:]> and notify the user
 * about announcements and polls.<br>
 * You usually create the module using the method moduleWithNotificationIcon: and pass it when you initialize Engagement
 * (using method <[EngagementAgent init:modules:]>)
 *
 * *Example of a basic integration:*
 *
 *  - (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions {
 *      AEReachModule* reach = [AEReachModule moduleWithNotificationIcon:[UIImage imageNamed:@"icon.png"]];
 *      [EngagementAgent init:@"Endpoint={YOUR_APP_COLLECTION.DOMAIN};SdkKey={YOUR_APPID};AppId={YOUR_APPID}"
 *modules:reach, nil];
 *
 *      ...
 *
 *      return YES;
 *  }
 *
 */
@interface AEReachModule : NSObject<AEModule>
{
  @private
  /* Storage */
  AEStorage* _db;
  AEStorage* _feedbackDb;
  
  /* Background/Inactive state tracking */
  BOOL _didEnterBackground;
  BOOL _willResignActive;
  BOOL _willEnterForeground;
  BOOL _applicationDidLaunch;
  BOOL _applicationLaunchedViaSystemPush;

  /* Set of DLCs that are pending download (key = localId) */
  NSMutableIndexSet* _pendingDlcs;

  /* Cached Reach Values */
  NSDictionary* _cachedReachValues;

  /* Entries scheduled to be removed */
  NSMutableIndexSet* _trash;

  /* Scanning db storage context */
  BOOL _scanning;

  /* Current state */
  AEReachModuleState _state;

  /** The message identifier being processed */
  NSInteger _processingId;

  /* Current activity */
  NSString* _currentActivity;

  /* Announcement controller classes by category */
  NSMutableDictionary* _announcementControllers;

  /* Poll controller classes by category */
  NSMutableDictionary* _pollControllers;

  /* Notification handlers by category */
  NSMutableDictionary* _notifiers;

  /* Special parameters to inject in announcement's action url and body */
  NSDictionary* _params;

  /* Remember if the agent has been started or not */
  BOOL _isStarted;

  /* Data push delegate */
  id<AEReachDataPushDelegate> _dataPushDelegate;

  /* Current displayed controller */
  AEContentViewController* _displayedController;

  /* Auto badge */
  BOOL _autoBadgeEnabled;
  BOOL _badgeNotificationReceived;

  /* Maximum number of contents */
  NSUInteger _maxContents;
}

/**
 * Instantiate a new reach Engagement module.
 * @param icon The image to use as the notification icon
 */
+ (id)moduleWithNotificationIcon:(UIImage*)icon;

/**
 * Enable or disable automatic control of the badge value. If enabled, the Reach module will automatically
 * clear the application badge and also reset the value stored by Engagement every time the application
 * is started or foregrounded.
 * @param enabled YES to enable auto badge, NO otherwise (Disabled by default).
 */
- (void)setAutoBadgeEnabled:(BOOL)enabled;

/**
 * Set the maximum number of in-app campaigns that can be displayed.
 * @param maxCampaigns The maximum number of in-app campaigns that can be displayed (0 to disable in-app campaigns).
 */
- (void)setMaxInAppCampaigns:(NSUInteger)maxCampaigns;

/**
 * Register an announcement category.
 * @param category The name of the category to map.
 * @param clazz The associated view controller class to instantiate when an announcement of the given
 * category is received. The controller class should inherit from <AEAnnouncementViewController>.
 */
- (void)registerAnnouncementController:(Class)clazz forCategory:(NSString*)category;

/**
 * Register a poll category.
 * @param category The name of the category to map.
 * @param clazz The associated view controller class to instantiate when an poll of the given
 * category is received. The controller class should inherit from <AEPollViewController>.
 */
- (void)registerPollController:(Class)clazz forCategory:(NSString*)category;

/**
 * Register a notifier for a given category.
 * @param notifier Notifier to register for a category.
 * @param category The name of the category.
 */
- (void)registerNotifier:(id<AENotifier>)notifier forCategory:(NSString*)category;

/**
 * Mark given content processed. It will be removed from cache,
 * and any other waiting contents will be displayed to the user.
 * @param content The content to mark as processed.
 */
- (void)markContentProcessed:(AEReachContent*)content;

/**
 * Called when loading page is dismissed by user.
 */
- (void)onLoadingViewDismissed;

/**
 * Called when a notification is actioned.
 * @param content The content associated to the notification.
 */
- (void)onNotificationActioned:(AEReachContent*)content;

/**
 * Indicate if feedback with status code should be sent for a push message.
 * This method queries the db and updates the db at the same time.
 * @param The flag indicating if feedback should be sent or not.
 */
- (BOOL)shouldSendFeedback:(NSString*)status forPushId:(NSString*)pushId;

/** The delegate that will handle data pushes.  */
@property(nonatomic, retain) id<AEReachDataPushDelegate> dataPushDelegate;

@end
