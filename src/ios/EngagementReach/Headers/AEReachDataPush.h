/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import "AEReachContent.h"

/**
 * Data push's type.
 */
typedef NS_ENUM (NSInteger, AEDatapushType)
{
  /** Unknwon data-push type */
  AEDatapushTypeUnknown = -1,
  
  /** Data-push with a text content */
  AEDatapushTypeText = 1,
  
  /** Data-push with a base 64 encoded content */
  AEDatapushTypeBase64 = 2
};

/** Datapush kind */
static NSString* const kAEDatapushKind = @"d";

/**
 * The `AEReachDataPush` class defines objects that represent a generic reach content.
 */
@interface AEReachDataPush : AEReachContent
{
  @private
  AEDatapushType _type;
  NSDictionary* _cachedParams;
}

/**
 * Parse an announcement
 * @param reachValues Parsed reach values.
 * @param params special parameters to replace in the body of the datapush.
 * @result A new announcement or nil if it couldn't be parsed.
 */
+ (id)datapushWithReachValues:(NSDictionary*)reachValues params:(NSDictionary*)params;

/**
 * Intialize data push.
 * @param reachValues Parsed reach values.
 * @param params special parameters to replace in the body of the datapush.
 * @result A new announcement or nil if it couldn't be parsed.
 */
- (id)initWithReachValues:(NSDictionary*)reachValues params:(NSDictionary*)params;

/**
 * Get the type for this data push.
 *
 * Possible values are:
 *
 * - `AEDatapushTypeUnknown`: Unknown data push type
 * - `AEDatapushTypeText`: A text data push
 * - `AEDatapushTypeBase64`: A base 64 data push
 */
@property(readonly) AEDatapushType type;

/** Get decoded body. Only apply on base 64 data pushes (return `nil` for other types). */
@property(readonly) NSData* decodedBody;

@end