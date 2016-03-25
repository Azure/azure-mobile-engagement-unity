/*
 * Copyright (c) Microsoft Corporation.  All rights reserved.
 * Licensed under the MIT license. See License.txt in the project root for license information.
 */

#include "EngagementShared.h"


const NSString* ENGAGEMENT_LOGTAG = @"[Engagement-Plugin] ";
const NSString* ENGAGEMENT_ERRORTAG =  @"[Engagement-Plugin] ERROR: ";

// helper

bool isStringNull(NSString*_string)
{
    return _string==(id) [NSNull null] || [_string length]==0 || [_string isEqualToString:@""];
}

@implementation EngagementShared


+ (EngagementShared*)instance {
    static EngagementShared *_instance = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        _instance = [self alloc];
        [_instance enablePluginLog:false];
    });
    return _instance;
}


-(void)initSDK:(NSString*)_sdkName withPluginVersion:(NSString*)_pluginVersion withNativeVersion:(NSString*)_nativeVersion
{
   
    pluginVersion = _pluginVersion;
    nativeVersion = _nativeVersion;
    sdkName = _sdkName ;
    readyForPush = false;
    readyForURL  = false;
    lastURL = nil;
    dataPushes  = [[NSMutableArray alloc] init];
    pendingNotifications = [[NSMutableArray alloc] init];
    
    if (enablePluginLog)
        NSLog( @"%@Plugin %@ v%@ (SDK Version %@)",ENGAGEMENT_LOGTAG,_sdkName,_pluginVersion,_nativeVersion);
    
}

-(void)enablePluginLog:(BOOL)_enablePluginLog
{
    enablePluginLog = _enablePluginLog;
    if (enablePluginLog)
        NSLog( @"%@Plugin log enabled!",ENGAGEMENT_LOGTAG);
    
}

-(void)enableNativeLog:(BOOL)_enableNativeLog
{
    if (enablePluginLog && _enableNativeLog)
        NSLog( @"%@Native Logs enabled!",ENGAGEMENT_LOGTAG);
    
    [EngagementAgent setTestLogEnabled:_enableNativeLog];
}

- (void)didFailToRegisterForRemoteNotificationsWithError:(NSError*)error
{
    NSLog( @"%@didFailToRegisterForRemoteNotificationsWithError %@", ENGAGEMENT_ERRORTAG,error);
}


-(void)didReceiveRemoteNotification:(NSDictionary *)userInfo fetchCompletionHandler:(void (^)(UIBackgroundFetchResult result))handler
{
    
    if (!readyForPush)
    {
        NSLog( @"%@Delaying notification until application is initialized",ENGAGEMENT_LOGTAG);

        NSArray* notif = [NSArray arrayWithObjects:userInfo,handler,nil];
        [pendingNotifications addObject:notif];
    }
    else
         NSLog( @"%@Processing notification",ENGAGEMENT_LOGTAG);
    
     [[EngagementAgent shared] applicationDidReceiveRemoteNotification:userInfo fetchCompletionHandler:handler];
}


- (void)didReceiveRemoteNotification:(NSDictionary *)userInfo
{
    
    if (!readyForPush)
    {
        NSLog( @"%@Delaying notification until application is initialized",ENGAGEMENT_LOGTAG);
        
        NSArray* notif = [NSArray arrayWithObjects:userInfo,nil,nil];
        [pendingNotifications addObject:notif];
    }
    else
        NSLog( @"%@Processing notification",ENGAGEMENT_LOGTAG);

    
    [[EngagementAgent shared] applicationDidReceiveRemoteNotification:userInfo fetchCompletionHandler:nil];
    
}

- (void)didRegisterForRemoteNotificationsWithDeviceToken:(NSData *)deviceToken
{
    if (enablePluginLog)
        NSLog( @"%@didRegisterForRemoteNotificationsWithDeviceToken:%@",ENGAGEMENT_LOGTAG,deviceToken);
    
    [[EngagementAgent shared] registerDeviceToken:deviceToken];
}




-(void)initialize: (NSString*)_connectionString  withReachEnabled:(NSNumber*)_enableReach  withReachIcon:(NSString*)_reachIcon withLocation:(locationReportingType)_locationReporting backgroundReporting:(backgroundReportingType)_backgroundReporting withDelegate:(id<EngagementDelegate>)_delegate
{
    
    delegate = _delegate;
    
    if (enablePluginLog)
        NSLog( @"%@Initializing EngagementAgent", ENGAGEMENT_LOGTAG);
    
    if ( isStringNull(_connectionString) )
        NSLog( @"%@ConnectionString is missing : cannot initialize agent",ENGAGEMENT_ERRORTAG);

    @try {
        
        AEReachModule* reach = nil;
        if ([_enableReach boolValue] == YES)
        {
            
            if (isStringNull(_reachIcon))
            {
                NSDictionary *infoPlist = [[NSBundle mainBundle] infoDictionary];
                _reachIcon = [[infoPlist valueForKeyPath:@"CFBundleIcons.CFBundlePrimaryIcon.CFBundleIconFiles"] lastObject];
                if (enablePluginLog)
                    NSLog(@"%@No icon specified : using the application icon for notification",ENGAGEMENT_LOGTAG);
            }
            
            if (enablePluginLog)
                NSLog( @"%@Preparing Reach Module with Icon :%@", ENGAGEMENT_LOGTAG,_reachIcon);
            
            UIImage* icon = [UIImage imageNamed:_reachIcon];
            if (icon == nil)
                NSLog( @"%@Icon '%@' missing (must be added in the XCode project)", ENGAGEMENT_ERRORTAG,_reachIcon);
            
            reach = [AEReachModule moduleWithNotificationIcon:icon];
            if (reach == nil)
                NSLog( @"%@Failed to initialize reach",ENGAGEMENT_ERRORTAG);
            else
            {
                [reach setAutoBadgeEnabled:YES];
                [reach setDataPushDelegate:self];

            #if TARGET_IPHONE_SIMULATOR
                NSLog( @"%@Running on iOS Simulator -- push notifications are disabled",ENGAGEMENT_LOGTAG);
            #endif

            }
        }
        else
        {
            if (enablePluginLog)
                NSLog( @"%@Reach module not enabled",ENGAGEMENT_LOGTAG);
            
        }

        [EngagementAgent init:_connectionString modules:reach, nil];

        switch(_locationReporting)
        {
            case LOCATIONREPORTING_NONE:
                break;
            case LOCATIONREPORTING_LAZY:
                [[EngagementAgent shared] setLazyAreaLocationReport:YES];
                if (enablePluginLog)
                    NSLog( @"%@Lazy Area Location enabled",ENGAGEMENT_LOGTAG);
                break;
            case LOCATIONREPORTING_REALTIME:
                [[EngagementAgent shared] setRealtimeLocationReport:YES];
                if (enablePluginLog)
                    NSLog( @"%@Real Time Location enabled",ENGAGEMENT_LOGTAG);
                break;
            case LOCATIONREPORTING_FINEREALTIME:
                [[EngagementAgent shared] setRealtimeLocationReport:YES];
                [[EngagementAgent shared] setFineRealtimeLocationReport:YES];
                if (enablePluginLog)
                    NSLog( @"%@Fine Real Time Location enabled",ENGAGEMENT_LOGTAG);
                break;
        }
       
        if ( _backgroundReporting == BACKGROUNDREPORTING_BACKGROUND )
        {
            if ( _locationReporting == LOCATIONREPORTING_REALTIME|| _locationReporting == LOCATIONREPORTING_FINEREALTIME )
            {
                [[EngagementAgent shared] setBackgroundRealtimeLocationReport:YES withLaunchOptions:nil];
                  if (enablePluginLog)
                    NSLog( @"%@Enabling Background Mode for realtime reporting",ENGAGEMENT_LOGTAG);
            }
            else
                 NSLog( @"%@Background mode requires realtime location",ENGAGEMENT_ERRORTAG);
        }
        else
        if ( _backgroundReporting == BACKGROUNDREPORTING_FOREGROUND )
        {
            if ( _locationReporting == LOCATIONREPORTING_NONE )
                 NSLog( @"%@Location required when using Foreground Location",ENGAGEMENT_ERRORTAG);
        }
        else
        {
            if ( _locationReporting != LOCATIONREPORTING_NONE )
            {
                NSLog( @"%@Foreground or Background required when using Location",ENGAGEMENT_ERRORTAG);
            }
        }
        
        NSDictionary *dict = [[NSDictionary alloc] initWithObjectsAndKeys:
                              pluginVersion, sdkName,  nil];
        
        [[EngagementAgent shared] sendAppInfo:dict];
        
    }
    @catch (NSException * e) {
        NSLog( @"%@Failed to initialize AZME, Exception: %@", ENGAGEMENT_ERRORTAG,e);
    }
}

-(void)processDataPush
{
    if (readyForPush == FALSE)
        return ;
    
    for (NSArray* push in dataPushes) {
     
        NSString* encodedCategory = push[0];
        NSString* encodedBody = push[1];
        NSNumber* isBase64 = push[2];

        if (enablePluginLog)
             NSLog( @"%@handling data push w/ category %@", ENGAGEMENT_LOGTAG,encodedCategory);

        [delegate didReceiveDataPush:encodedCategory withBody:encodedBody isBase64:isBase64];
    }
    
    [dataPushes removeAllObjects];
    
}

-(void)addDataPush:(NSString*)_category withBody:(NSString*)_body isBase64:(BOOL)_isBase64
{
    id  category ;
    if (_category==nil)
        category = [NSNull null];
    else
        category = _category;
    
    NSArray* push = [NSArray arrayWithObjects:category,_body,[NSNumber numberWithBool:_isBase64],nil];
    [dataPushes addObject:push];
    [self processDataPush];
    
}

-(BOOL)didReceiveStringDataPushWithCategory:(NSString*)category body:(NSString*)body
{
 
    if (enablePluginLog)
        NSLog( @"%@received string data push message w/ category: %@", ENGAGEMENT_LOGTAG,category);

    NSString* encodedCategory= [category stringByAddingPercentEscapesUsingEncoding:NSUTF8StringEncoding];
    NSString* encodedBody = [body stringByAddingPercentEscapesUsingEncoding:NSUTF8StringEncoding];
    [self addDataPush:encodedCategory withBody:encodedBody isBase64:false];
    
   return YES;
}

-(BOOL)didReceiveBase64DataPushWithCategory:(NSString*)category decodedBody:(NSData *)decodedBody encodedBody:(NSString *)encodedBody
{
  
    if (enablePluginLog)
         NSLog( @"%@received base64 data push message w/ category: %@",ENGAGEMENT_LOGTAG, category);

    NSString* encodedCategory= [category stringByAddingPercentEscapesUsingEncoding:NSUTF8StringEncoding];
    [self addDataPush:encodedCategory withBody:encodedBody isBase64:true];
    
   return YES;
}

- (void)startActivity:(NSString*)_activityName withExtraInfos:(NSString*)_extraInfos
{
  
    if (enablePluginLog)
        NSLog( @"%@startActivity:%@",ENGAGEMENT_LOGTAG,_activityName);
    
     NSDictionary* JSON = [NSJSONSerialization     JSONObjectWithData: [_extraInfos dataUsingEncoding:NSUTF8StringEncoding]
                                               options: NSJSONReadingMutableContainers
                                                 error: nil];
    [[EngagementAgent shared] startActivity:_activityName extras:JSON];
    
}

- (void)endActivity
{
    
    if (enablePluginLog)
        NSLog( @"%@endActivity",ENGAGEMENT_LOGTAG);
    
    [[EngagementAgent shared] endActivity];
}

- (void)sendEvent:(NSString*)_eventName withExtraInfos:(NSString*)_extraInfos
{
    if (enablePluginLog)
        NSLog( @"%@sendEvent:%@",ENGAGEMENT_LOGTAG,_eventName);
 
    
    NSDictionary* JSON = [NSJSONSerialization     JSONObjectWithData: [_extraInfos dataUsingEncoding:NSUTF8StringEncoding]
                                                             options: NSJSONReadingMutableContainers
                                                               error: nil];
    
    [[EngagementAgent shared] sendEvent:_eventName extras:JSON];

}

- (void)startJob:(NSString*)_jobName withExtraInfos:(NSString*)_extraInfos
{
    
    if (enablePluginLog)
        NSLog( @"%@startJob:%@",ENGAGEMENT_LOGTAG,_jobName);
    
    NSDictionary* JSON = [NSJSONSerialization     JSONObjectWithData: [_extraInfos dataUsingEncoding:NSUTF8StringEncoding]
                                                             options: NSJSONReadingMutableContainers
                                                               error: nil];
   
    [[EngagementAgent shared] startJob:_jobName extras:JSON];
    
}

- (void)endJob:(NSString*)_jobName
{
    
    if (enablePluginLog)
        NSLog( @"%@endJob:%@",ENGAGEMENT_LOGTAG,_jobName);
    
    [[EngagementAgent shared] endJob:_jobName];
    
}

- (void)sendAppInfo:(NSString*)_extraInfos
{
    
    if (enablePluginLog)
        NSLog( @"%@sendAppInfo:%@",ENGAGEMENT_LOGTAG,_extraInfos);
    
    NSDictionary* JSON = [NSJSONSerialization     JSONObjectWithData: [_extraInfos dataUsingEncoding:NSUTF8StringEncoding]
                                                             options: NSJSONReadingMutableContainers
                                                               error: nil];
    
    [[EngagementAgent shared] sendAppInfo:JSON];
    
}

- (void)sendSessionEvent:(NSString*)_eventName withExtraInfos:(NSString*)_extraInfos
{
    
    if (enablePluginLog)
        NSLog( @"%@sendSessionEvent:%@ %@",ENGAGEMENT_LOGTAG,_eventName,_extraInfos);
    
    NSDictionary* JSON = [NSJSONSerialization     JSONObjectWithData: [_extraInfos dataUsingEncoding:NSUTF8StringEncoding]
                                                             options: NSJSONReadingMutableContainers
                                                               error: nil];
    
    [[EngagementAgent shared] sendSessionEvent:_eventName extras:JSON];
}

- (void)sendSessionError:(NSString*)_errorName withExtraInfos:(NSString*)_extraInfos
{
    
    if (enablePluginLog)
        NSLog( @"%@sendSessionError:%@ %@",ENGAGEMENT_LOGTAG,_errorName,_extraInfos);
    
    NSDictionary* JSON = [NSJSONSerialization     JSONObjectWithData: [_extraInfos dataUsingEncoding:NSUTF8StringEncoding]
                                                             options: NSJSONReadingMutableContainers
                                                               error: nil];
    
    [[EngagementAgent shared] sendSessionError:_errorName extras:JSON];
}

- (void)sendError:(NSString*)_errorName withExtraInfos:(NSString*)_extraInfos
{
    
    if (enablePluginLog)
        NSLog( @"%@sendError:%@ %@",ENGAGEMENT_LOGTAG,_errorName,_extraInfos);
    
    NSDictionary* JSON = [NSJSONSerialization     JSONObjectWithData: [_extraInfos dataUsingEncoding:NSUTF8StringEncoding]
                                                             options: NSJSONReadingMutableContainers
                                                               error: nil];
    
    [[EngagementAgent shared] sendError:_errorName extras:JSON];
}



- (void)sendJobEvent:(NSString*)_eventName inJob:(NSString*)_jobName withExtraInfos:(NSString*)_extraInfos
{
    
    if (enablePluginLog)
        NSLog( @"%@sendJobEvent:%@ %@ %@",ENGAGEMENT_LOGTAG,_eventName,_jobName,_extraInfos);
    
    NSDictionary* JSON = [NSJSONSerialization     JSONObjectWithData: [_extraInfos dataUsingEncoding:NSUTF8StringEncoding]
                                                             options: NSJSONReadingMutableContainers
                                                               error: nil];
  
    [[EngagementAgent shared] sendJobEvent:_eventName jobName:_jobName extras:JSON];
}

- (void)sendJobError:(NSString*)_errorName inJob:(NSString*)_jobName withExtraInfos:(NSString*)_extraInfos
{
    
    if (enablePluginLog)
        NSLog( @"%@sendJobError:%@ %@ %@",ENGAGEMENT_LOGTAG,_errorName,_jobName,_extraInfos);
    
    NSDictionary* JSON = [NSJSONSerialization     JSONObjectWithData: [_extraInfos dataUsingEncoding:NSUTF8StringEncoding]
                                                             options: NSJSONReadingMutableContainers
                                                               error: nil];
    
    [[EngagementAgent shared] sendJobError:_errorName jobName:_jobName extras:JSON];
}




- (NSDictionary*)getStatus
{

    NSDictionary* dict = [NSDictionary dictionaryWithObjectsAndKeys:
                          pluginVersion, @"pluginVersion",
                          nativeVersion, @"nativeVersion",
                          [NSNumber numberWithBool: [[EngagementAgent shared] enabled] ],@"isEnabled",
                          [[EngagementAgent shared] deviceId], @"deviceId",
                          nil];
    
    return dict;
}


-(void)enablePush
{
    readyForPush = true;
    for (NSArray* notif in pendingNotifications) {
        
        NSDictionary* dict = notif[0];
        id handler = nil;
        if ([dict count]==2)
            handler = notif[1];
        
        if (enablePluginLog)
            NSLog( @"%@handling pending notification", ENGAGEMENT_LOGTAG);
        
         [[EngagementAgent shared] applicationDidReceiveRemoteNotification:dict fetchCompletionHandler:handler];
    }
    
    [pendingNotifications removeAllObjects];
    
  
    [self processDataPush];
}

-(void)enableURL
{
    readyForURL = true;
    [self processURL];
}

-(void)processURL
{
    if (readyForURL == FALSE || lastURL==nil)
        return ;
    
    if (enablePluginLog)
        NSLog( @"%@processing URL : %@",ENGAGEMENT_LOGTAG,lastURL);
    
    [delegate didReceiveURL:lastURL];
  
    lastURL = nil;
}

- (void)handleOpenURL:(NSString*)_url
{
  
    if (enablePluginLog)
        NSLog( @"%@handling URL :%@",ENGAGEMENT_LOGTAG,_url);
    
    lastURL = _url;
    [self processURL];
   
}

- (void)registerForPushNotification
{
    
    if (enablePluginLog)
        NSLog( @"%@register the application for Push notifications",ENGAGEMENT_LOGTAG);
    
    if ([[UIApplication sharedApplication] respondsToSelector:@selector(registerUserNotificationSettings:)]) {
        [[UIApplication sharedApplication] registerUserNotificationSettings:[UIUserNotificationSettings settingsForTypes:(UIUserNotificationTypeBadge | UIUserNotificationTypeSound | UIUserNotificationTypeAlert) categories:nil]];
        [[UIApplication sharedApplication] registerForRemoteNotifications];
    }
    else {
        [[UIApplication sharedApplication] registerForRemoteNotificationTypes:(UIRemoteNotificationTypeBadge | UIRemoteNotificationTypeSound | UIRemoteNotificationTypeAlert)];
    }
    
}

-(void) saveUserPreferences
{
    if (enablePluginLog)
        NSLog( @"%@Saving user preferences", ENGAGEMENT_LOGTAG);
    
    NSDictionary *parameters = [[NSUserDefaults standardUserDefaults] dictionaryRepresentation];
    NSMutableDictionary *engagementParameters = [[NSMutableDictionary alloc] init];
    [parameters enumerateKeysAndObjectsUsingBlock:^(NSString *key, id obj, BOOL *stop) {
        if ([[key lowercaseString] hasPrefix:@"engagement"]) {
            engagementParameters[key] = obj;
        }
    }];
    userPreferences = engagementParameters;
}

-(void) restoreUserPreferences
{
    if (enablePluginLog)
        NSLog( @"%@Restoring user preferences", ENGAGEMENT_LOGTAG);
    
    if (userPreferences == nil)
    {
        NSLog( @"%@Call saveUserPreferences first", ENGAGEMENT_ERRORTAG);
        return ;
    }
    
    [userPreferences enumerateKeysAndObjectsUsingBlock:^(id key, id obj, BOOL *stop) {
        [[NSUserDefaults standardUserDefaults] setObject:obj forKey:key];
    }];
    userPreferences = nil;
    
}

-(void) setEnabled:(BOOL)_enabled
{
    if (enablePluginLog)
        NSLog( @"%@setEnabled :%d", ENGAGEMENT_LOGTAG,_enabled);
    
    [[EngagementAgent shared] setEnabled:_enabled];
}


@end
