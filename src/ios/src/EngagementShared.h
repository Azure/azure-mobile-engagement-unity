/*
 * Copyright (c) Microsoft Corporation.  All rights reserved.
 * Licensed under the MIT license. See License.txt in the project root for license information.
 */

#import <UIKit/UIKit.h>
#import "EngagementAgent.h"
#import "AEReachDataPushDelegate.h"
#import "AEReachModule.h"

extern const NSString* ENGAGEMENT_LOGTAG ;
extern const NSString* ENGAGEMENT_ERRORTAG ;

typedef enum {
    LOCATIONREPORTING_NONE=100,
    LOCATIONREPORTING_LAZY=101,
    LOCATIONREPORTING_REALTIME=102,
    LOCATIONREPORTING_FINEREALTIME=103
} locationReportingType;

typedef enum {
    BACKGROUNDREPORTING_NONE=200,
    BACKGROUNDREPORTING_FOREGROUND=201,
    BACKGROUNDREPORTING_BACKGROUND=202
} backgroundReportingType;

@protocol EngagementDelegate

- (void)didReceiveDataPush:(NSString*)_category withBody:(NSString*)_body isBase64:(NSNumber*)_isBase64;
- (void)didReceiveURL:(NSString*)_url;

@end

@interface EngagementShared : NSObject <AEReachDataPushDelegate>
{
    BOOL readyForPush;
    BOOL enablePluginLog;
    NSMutableArray*  pendingNotifications ;
    NSMutableArray*  dataPushes ;
    BOOL readyForURL;
    NSString*  lastURL ;
    NSString* pluginVersion;
    NSString* nativeVersion;
    NSString* sdkName;
    NSDictionary *userPreferences;
    
    id<EngagementDelegate> delegate;
}

// singleton
+ (EngagementShared*)instance;

// private
- (void)processDataPush;
- (void)addDataPush:(NSString*)_category withBody:(NSString*)_body isBase64:(BOOL)_isBase64;
- (BOOL)didReceiveStringDataPushWithCategory:(NSString*)category body:(NSString*)body;
- (BOOL)didReceiveBase64DataPushWithCategory:(NSString*)category decodedBody:(NSData *)decodedBody encodedBody:(NSString *)encodedBody;

// public

-(void)initSDK:(NSString*)_sdkName withPluginVersion:(NSString*)_pluginVersion withNativeVersion:(NSString*)_nativeVersion ;
-(void)enablePluginLog:(BOOL)_enablePluginLog ;
-(void)enableNativeLog:(BOOL)_enableNativeLog ;

-(void)initialize: (NSString*)_connectionString   withReachEnabled:(NSNumber*)_enableReach withReachIcon:(NSString*)_reachIcon withLocation:(locationReportingType)_locationReporting backgroundReporting:(backgroundReportingType)_backgroundReporting withDelegate:(id<EngagementDelegate>)_delegate;

-(void)enablePush;
-(void)enableURL;

- (void)didFailToRegisterForRemoteNotificationsWithError:(NSError*)error;
- (void)didReceiveRemoteNotification:(NSDictionary *)userInfo fetchCompletionHandler:(void (^)(UIBackgroundFetchResult result))handler;
- (void)didReceiveRemoteNotification:(NSDictionary *)userInfo;
- (void)didRegisterForRemoteNotificationsWithDeviceToken:(NSData *)deviceToken;

- (void)startActivity:(NSString*)_activityName withExtraInfos:(NSString*)_extraInfos;
- (void)endActivity;
- (void)sendEvent:(NSString*)_eventName withExtraInfos:(NSString*)_extraInfos;
- (void)startJob:(NSString*)_jobName withExtraInfos:(NSString*)_extraInfos;
- (void)endJob:(NSString*)_jobName;
- (void)sendAppInfo:(NSString*)_extraInfos;

- (void)sendSessionEvent:(NSString*)_eventName withExtraInfos:(NSString*)_extraInfos;
- (void)sendSessionError:(NSString*)_errorName withExtraInfos:(NSString*)_extraInfos;
- (void)sendError:(NSString*)_errorName withExtraInfos:(NSString*)_extraInfos;
- (void)sendJobEvent:(NSString*)_eventName inJob:(NSString*)_jobName withExtraInfos:(NSString*)_extraInfos;
- (void)sendJobError:(NSString*)_errorName inJob:(NSString*)_jobName withExtraInfos:(NSString*)_extraInfos;

- (NSDictionary*)getStatus ;
- (void)handleOpenURL:(NSString*)_url;
- (void)registerForPushNotification;

-(void)saveUserPreferences;
-(void)restoreUserPreferences;
-(void)setEnabled:(BOOL)_enabled;


@end


