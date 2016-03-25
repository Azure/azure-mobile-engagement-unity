/*
 * Copyright (c) Microsoft Corporation.  All rights reserved.
 * Licensed under the MIT license. See License.txt in the project root for license information.
 */

#include <sys/types.h>
#include <sys/sysctl.h>
#import <objc/runtime.h>
#import <objc/message.h>

#if ENGAGEMENT_UNITY != 1
#error message("EngagementPostBuild.CS has not been executed -  check your Engagement.package installation")
#endif

#include "../Classes/UnityAppController.h"

@implementation UnityAppController(Engagement)

#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Warc-performSelector-leaks"

- (void)application:(UIApplication *)application  engagementDidReceiveRemoteNotification:(NSDictionary *)userInfo fetchCompletionHandler:(void (^)(UIBackgroundFetchResult result))handler
{
    id instance =[NSClassFromString(@"EngagementShared") performSelector:NSSelectorFromString(@"instance")];
    [instance performSelector:NSSelectorFromString(@"didReceiveRemoteNotification:fetchCompletionHandler:") withObject:userInfo withObject:handler];
        
    // call the previous implementation (and not itself!)
    [self application:application engagementDidReceiveRemoteNotification:userInfo fetchCompletionHandler:handler];
}

- (BOOL)application:(UIApplication*)application engagementDidFinishLaunchingWithOptions:(NSDictionary*)launchOptions
{   
    id instance =[NSClassFromString(@"EngagementUnity") performSelector:NSSelectorFromString(@"instance")];
    [instance performSelector:NSSelectorFromString(@"registerApplication")];

    // call the previous implementation (and not itself!)
    return [self application:application engagementDidFinishLaunchingWithOptions:launchOptions];
}

#pragma clang diagnostic pop

//  used in case the "parent" delegate was not implemented
- (void)application:(UIApplication *)application engagementEmpty:(id)_fake
{

}

+ (void)swizzleInstanceSelector:(SEL)originalSelector withNewSelector:(SEL)newSelector
{
    Method originalMethod = class_getInstanceMethod(self, originalSelector);
    Method newMethod = class_getInstanceMethod(self, newSelector);
    
    // if the original Method does not exist, replace it with an empty implementation
    if (originalMethod==nil)
    {
        Method emptyMethod = class_getInstanceMethod(self, @selector(application:engagementEmpty:));
        BOOL methodAdded = class_addMethod([self class],
                                           originalSelector,
                                           method_getImplementation(emptyMethod), // empty code
                                           method_getTypeEncoding(newMethod)); // but keep signature
        
        if (methodAdded==false)
            NSLog( @"Failed to add method %@",NSStringFromSelector(originalSelector));
        
        originalMethod = class_getInstanceMethod(self, originalSelector);
    }
    
    method_exchangeImplementations(originalMethod, newMethod);
}

+(void)load
{   
    [self swizzleInstanceSelector:@selector(application:didReceiveRemoteNotification:fetchCompletionHandler:)
                  withNewSelector:@selector(application:engagementDidReceiveRemoteNotification:fetchCompletionHandler:)];
    [self swizzleInstanceSelector:@selector(application:didFinishLaunchingWithOptions:)
                  withNewSelector:@selector(application:engagementDidFinishLaunchingWithOptions:)];
 }
@end

