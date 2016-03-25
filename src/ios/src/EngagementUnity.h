/*
 * Copyright (c) Microsoft Corporation.  All rights reserved.
 * Licensed under the MIT license. See License.txt in the project root for license information.
 */

#import <UIKit/UIKit.h>
#import "EngagementShared.h"

#define UNITY_METHOD_ONDATAPUSHRECEIVED "onDataPushReceived"
#define UNITY_METHOD_ONHANDLEURL        "onHandleURL"
#define UNITY_METHOD_ONSTATUSRECEIVED   "onStatusReceived"

#define DEFINE_NOTIFICATION(name) \
    __attribute__((visibility ("default"))) NSString* const name = @#name;
#define REGISTER_SELECTOR(observer,sel, notif_name)	[[NSNotificationCenter defaultCenter] \
    addObserver:observer selector:sel name:notif_name object:nil ]

@interface EngagementUnity : NSObject <EngagementDelegate>
{
@public
#define MAX_CHAR 256
    char instanceObject[MAX_CHAR];
}

// singleton
+ (EngagementUnity*)instance;
+ (void)load;

- (void)didFailToRegisterForRemoteNotificationsWithError:(NSNotification*)notification ;
- (void)didReceiveRemoteNotification:(NSNotification*)notification ;
- (void)didRegisterForRemoteNotificationsWithDeviceToken: (NSNotification*)notification ;
- (void)registerApplication;

@end

// Interface Unity -> Plugin
extern void initializeEngagement(const char* _instanceName);
extern void initializeReach() ;
extern void startActivity(const char* _activityName, const char* _extraInfos) ;
extern void endActivity();
extern void sendEvent(const char* _eventName, const char* _extraInfos);
extern void startJob(const char* _jobName, const char* _extraInfos);
extern void endJob(const char* _jobName);
extern void sendAppInfo(const char* _extraInfos);
extern void sendSessionEvent(const char* _eventName, const char* _extraInfos);
extern void sendJobEvent(const char* _eventName, const char* _jobName, const char* _extraInfos);
extern void sendJobError(const char* _errorName, const char* _jobName, const char* _extraInfos);
extern void sendError(const char* _errorName, const char* _extraInfos);
extern void sendSessionError(const char* _errorName, const char* _extraInfos);



extern void getStatus();


// Interface Plugin -> Unity
extern void UnitySendMessage(const char *, const char *, const char *);