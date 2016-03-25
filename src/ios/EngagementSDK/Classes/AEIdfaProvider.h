/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import <Foundation/Foundation.h>

/**
 * Provider for the Apple IDFA.
 *
 * This class must be integrated into your project whether you want Engagement to collect the IDFA or not. By default,
 * this class will return the actual
 * advertising identifier (on iOS 6+) using the AdSupport framework. This can be disabled by adding the preprocessor
 * macro named `ENGAGEMENT_DISABLE_IDFA`.
 *
 */
@interface AEIdfaProvider : NSObject

/**
 *  The singleton instance of the IDFA provider.
 *
 *  @return a shared instance of `AEIdfaProvider`.
 */
+ (id)shared;

/**
 * Check if IDFA reporting is enabled and ad tracking is enabled or not.
 *
 * @return A Boolean NSNumber that indicates whether the user has limited ad tracking or nil
 *         if IDFA reporting is not enabled.
 */
- (NSNumber*)isIdfaEnabled;

/* Return the identifier for advertisers or nil if not enabled/available. */

/**
 * Get the advertising identifier UUID value.
 *
 * @return the value of the advertising identifier or nil if ad tracking is disabled
 *         or IDFA collection is disabled.
 */
- (NSString*)idfa;

@end
