/*
 * Copyright (c) Microsoft Corporation.  All rights reserved.
 * Licensed under the MIT license. See License.txt in the project root for license information.
 */

#include "EngagementUnity.h"

#define PLUGIN_VERSION @"1.0.0"
#define NATIVE_VERSION @"3.2.1"
#define SDK_NAME @"Unity"

DEFINE_NOTIFICATION(kUnityDidRegisterForRemoteNotificationsWithDeviceToken);
DEFINE_NOTIFICATION(kUnityDidFailToRegisterForRemoteNotificationsWithError);
DEFINE_NOTIFICATION(kUnityDidReceiveRemoteNotification);
DEFINE_NOTIFICATION(kUnityDidReceiveLocalNotification);
DEFINE_NOTIFICATION(kUnityOnOpenURL);

@implementation EngagementUnity


+ (EngagementUnity*)instance {
    static EngagementUnity *_instance = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        _instance = [self alloc];
    });
    return _instance;
}

- (void)didFailToRegisterForRemoteNotificationsWithError:(NSNotification*)notification
{
    NSError *error=  (NSError *)notification.userInfo;
    [[EngagementShared instance]  didFailToRegisterForRemoteNotificationsWithError:error ];
}

- (void)didReceiveRemoteNotification:(NSNotification*)notification
{
    NSDictionary* userInfo = ( NSDictionary*)notification.userInfo;
    [[EngagementShared instance] didReceiveRemoteNotification:userInfo ];
}

- (void)didRegisterForRemoteNotificationsWithDeviceToken: (NSNotification*)notification
{
    NSData* deviceToken = (NSData*)notification.userInfo;
    [[EngagementShared instance] didRegisterForRemoteNotificationsWithDeviceToken:deviceToken ];
}

- (void)onOpenURL: (NSNotification*)notification
{
    NSDictionary* dict = (NSDictionary*)notification.userInfo;
    NSURL* url = [dict objectForKey:@"url"];
    [[EngagementShared instance] handleOpenURL: [url absoluteString]];
}

+(void) load
{
    
    NSDictionary* infoDict = [[NSBundle mainBundle] infoDictionary];
    NSDictionary* engagementDict = [infoDict objectForKey:@"Engagement"];

    NSNumber* enablePluginLog = [engagementDict objectForKey:@"ENABLE_PLUGIN_LOG"];
    NSNumber* enableNativeLog = [engagementDict objectForKey:@"ENABLE_NATIVE_LOG"];

    [[EngagementShared instance] enablePluginLog:[enablePluginLog boolValue]];
    [[EngagementShared instance] enableNativeLog:[enableNativeLog boolValue]];
    
    [[EngagementShared instance] initSDK:SDK_NAME withPluginVersion:PLUGIN_VERSION withNativeVersion:NATIVE_VERSION];
    
    REGISTER_SELECTOR([EngagementUnity instance],
                      @selector(didRegisterForRemoteNotificationsWithDeviceToken:),
                      kUnityDidRegisterForRemoteNotificationsWithDeviceToken);
    REGISTER_SELECTOR([EngagementUnity instance],
                      @selector(didFailToRegisterForRemoteNotificationsWithError:),
                      kUnityDidFailToRegisterForRemoteNotificationsWithError);
    REGISTER_SELECTOR([EngagementUnity instance],
                      @selector(didReceiveRemoteNotification:),
                      kUnityDidReceiveRemoteNotification);

    REGISTER_SELECTOR([EngagementUnity instance],
                      @selector(onOpenURL:),
                      kUnityOnOpenURL);
}

// EngagementShared Delegates

- (void)didReceiveDataPush:(NSString*)_category withBody:(NSString*)_body isBase64:(NSNumber*)_isBase64
{
    NSDictionary* JSON = [NSDictionary dictionaryWithObjectsAndKeys:
                         _category, @"category",
                        _body, @"body",
                        _isBase64, @"isBase64",
                          nil];
    
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:JSON options:0 error:nil];
    NSString *result = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
        
    UnitySendMessage([EngagementUnity instance]->instanceObject, UNITY_METHOD_ONDATAPUSHRECEIVED, [result UTF8String]);
}

- (void)didReceiveURL:(NSString*)_url
{
    UnitySendMessage([EngagementUnity instance]->instanceObject, UNITY_METHOD_ONHANDLEURL, [_url UTF8String]);
}

-(void)registerApplication
{
    
    NSDictionary* infoDict = [[NSBundle mainBundle] infoDictionary];
    NSDictionary* engagementDict = [infoDict objectForKey:@"Engagement"];
    
    NSString* connectionString = [engagementDict objectForKey:@"IOS_CONNECTION_STRING"];
    NSString* reachIcon = [engagementDict objectForKey:@"IOS_REACH_ICON"];
    NSNumber* enableReach = [engagementDict objectForKey:@"ENABLE_REACH"];
    NSNumber* locationType = [engagementDict objectForKey:@"LOCATION_REPORTING_TYPE"];
    NSNumber* locationMode = [engagementDict objectForKey:@"LOCATION_REPORTING_MODE"];
    
    
    [[EngagementShared instance]    initialize:connectionString
                            withReachEnabled:enableReach
                            withReachIcon:reachIcon
                            withLocation:[locationType intValue]
                            backgroundReporting:[locationMode intValue]
                            withDelegate:[EngagementUnity instance]] ;  
}

@end

// Unity Interface

void initializeEngagement(const char* _instanceName)
{
    strncpy([EngagementUnity instance]->instanceObject,_instanceName,MAX_CHAR);
}

void initializeReach()
{
    [[EngagementShared instance] registerForPushNotification];
    [[EngagementShared instance] enablePush];
    [[EngagementShared instance] enableURL];
}

void startActivity(const char* _activityName, const char* _extraInfos)
{
    NSString* activityName = [NSString stringWithUTF8String:_activityName];
    NSString* extraInfos = [NSString stringWithUTF8String:_extraInfos];

    [[EngagementShared instance] startActivity:activityName withExtraInfos:extraInfos];
 }

void endActivity()
{
    [[EngagementShared instance] endActivity];
}

void sendEvent(const char* _eventName, const char* _extraInfos)
{
    NSString* eventName = [NSString stringWithUTF8String:_eventName];
    NSString* extraInfos = [NSString stringWithUTF8String:_extraInfos];
    
    [[EngagementShared instance] sendEvent:eventName withExtraInfos:extraInfos];
}

void startJob(const char* _jobName, const char* _extraInfos)
{
    NSString* jobName = [NSString stringWithUTF8String:_jobName];
    NSString* extraInfos = [NSString stringWithUTF8String:_extraInfos];
    
    [[EngagementShared instance] startJob:jobName withExtraInfos:extraInfos];
}

void endJob(const char* _jobName)
{
    NSString* jobName = [NSString stringWithUTF8String:_jobName];
    [[EngagementShared instance] endJob:jobName];
}

void sendAppInfo(const char* _extraInfos)
{
    NSString* extraInfos = [NSString stringWithUTF8String:_extraInfos];
    [[EngagementShared instance] sendAppInfo:extraInfos];
}

void getStatus()
{
    NSDictionary* status = [ [EngagementShared instance] getStatus ];
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:status options:0 error:nil];
    NSString *result = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
    
    UnitySendMessage([EngagementUnity instance]->instanceObject, UNITY_METHOD_ONSTATUSRECEIVED, [result UTF8String]);
}

void sendSessionEvent(const char* _eventName, const char* _extraInfos)
{
    NSString* extraInfos = [NSString stringWithUTF8String:_extraInfos];
    NSString* eventName = [NSString stringWithUTF8String:_eventName];

    [[EngagementShared instance] sendSessionEvent:eventName withExtraInfos:extraInfos];
}

void sendJobEvent(const char* _eventName, const char* _jobName, const char* _extraInfos)
{
    NSString* extraInfos = [NSString stringWithUTF8String:_extraInfos];
    NSString* eventName = [NSString stringWithUTF8String:_eventName];
    NSString* jobName = [NSString stringWithUTF8String:_jobName];
    [[EngagementShared instance] sendJobEvent:eventName inJob:jobName withExtraInfos:extraInfos];
}

void sendJobError(const char* _errorName, const char* _jobName, const char* _extraInfos)
{
    NSString* extraInfos = [NSString stringWithUTF8String:_extraInfos];
    NSString* errorName = [NSString stringWithUTF8String:_errorName];
    NSString* jobName = [NSString stringWithUTF8String:_jobName];
    [[EngagementShared instance] sendJobError:errorName inJob:jobName withExtraInfos:extraInfos];
}

void sendError(const char* _errorName, const char* _extraInfos)
{
    NSString* errorName = [NSString stringWithUTF8String:_errorName];
    NSString* extraInfos = [NSString stringWithUTF8String:_extraInfos];
    [[EngagementShared instance] sendError:errorName withExtraInfos:extraInfos];
}

void sendSessionError(const char* _errorName, const char* _extraInfos)
{
    NSString* errorName = [NSString stringWithUTF8String:_errorName];
    NSString* extraInfos = [NSString stringWithUTF8String:_extraInfos];
    [[EngagementShared instance] sendSessionError:errorName withExtraInfos:extraInfos];
}

void saveUserPreferences()
{
    [[EngagementShared instance] saveUserPreferences];
}

void restoreUserPreferences()
{
    [[EngagementShared instance] restoreUserPreferences];
}

void setEnabled(BOOL _enabled)
{
    [[EngagementShared instance] setEnabled:_enabled];
}


void onApplicationPause(bool _paused)
{
   // NSLog( @"%@onApplicationPause:%d",CDVAZME_TAG,_paused);
}


