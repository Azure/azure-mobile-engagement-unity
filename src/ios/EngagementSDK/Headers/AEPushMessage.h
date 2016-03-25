/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import <Foundation/Foundation.h>

/**
 * A `AEPushMessage` object represents a Engagement push message.
 *
 * Engagement push messages are passed through Engagement agent to modules.
 *
 * **See also**
 *
 * - <AEModule>
 */
@interface AEPushMessage : NSObject<NSCoding>

/** Message's identifier */
@property(nonatomic, retain) NSString* messageId;

/** Reach values */
@property(nonatomic, retain) NSDictionary* reachValues;

/** Message's payload */
@property(nonatomic, retain) NSDictionary* payload;

/**
 * Parse payload data.
 * @param payloadData in NSData format.
 * @return Parsing error.
 */
- (NSError*)parseJsonPayload:(NSData*)payloadData;

@end