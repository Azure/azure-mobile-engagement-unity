/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import "AEIdfaProvider.h"
#import <AdSupport/AdSupport.h>

@implementation AEIdfaProvider

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark Singleton method
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

+ (id)shared
{
  static dispatch_once_t once;
  static id sharedInstance;
  dispatch_once(&once, ^{
    sharedInstance = [[self alloc] init];
  });
  return sharedInstance;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark Public methods
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

- (NSNumber*)isIdfaEnabled
{
#ifndef ENGAGEMENT_DISABLE_IDFA
  ASIdentifierManager* sharedManager = [self ASIdentifierManager];
  if (sharedManager)
  {
    return [NSNumber numberWithBool:[sharedManager isAdvertisingTrackingEnabled]];
  }
#endif
  return nil;
}

- (NSString*)idfa
{
#ifndef ENGAGEMENT_DISABLE_IDFA
  ASIdentifierManager* sharedManager = [self ASIdentifierManager];
  if (sharedManager)
  {
    if ([sharedManager isAdvertisingTrackingEnabled])
    {
      return [[sharedManager advertisingIdentifier] UUIDString];
    }
  }
#endif
  return nil;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark Private methods
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef ENGAGEMENT_DISABLE_IDFA
- (ASIdentifierManager*)ASIdentifierManager
{
  Class ASIdentifierManagerClass = NSClassFromString(@"ASIdentifierManager");
  if (ASIdentifierManagerClass)
  {
    return [ASIdentifierManagerClass sharedManager];
  }
  return nil;
}

#endif

@end
