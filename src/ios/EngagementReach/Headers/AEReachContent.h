/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import <Foundation/Foundation.h>

/** If set, the notification has downloadable content */
static NSInteger FLAG_DLC_NOTIFICATION = 1;

/** If set, the clickable content exists */
static NSInteger FLAG_DLC_CONTENT = 2;

/** If set, both notification download and clickable content exists */
static NSInteger FLAG_DLC_NOTIF_AND_CONTENT = 3;


/**
 * Abstract class for reach contents such as announcements and polls.
 */
@interface AEReachContent : NSObject
{
  @private
  NSUInteger _localId;
  NSString* _category;
  NSString* _body;
  BOOL _processed;
  BOOL _feedback;
  NSDate* _expiryDate;
  BOOL _expiryLocaltz;
  NSString* _actionURL;
  NSInteger _dlc;
  NSString* _dlcId;
  BOOL _dlcCompleted;
  NSString* _pushId;
}

/**
 * Parse a reach content from a given xml element
 * @param reachValues Parsed JSON reach payload
 * @result The parsed reach content or nil if content couldn't be parsed.
 */
- (id)initWithReachValues:(NSDictionary*)reachValues;

/** The unique reach content identifier setter method. */
@property(nonatomic, copy) NSString* contentId;

/** The unique push identifier setter method. */
@property(nonatomic, copy) NSString* pushId;

/** Local storage identifier */
@property(assign) NSUInteger localId;

/**
 * Category of this content. You usually don't need to read this value by yourself.
 * Instead, you should use the method <[AEReachModule registerAnnouncementController:forCategory:]>
 * or <[AEReachModule registerPollController:forCategory:]> to tell the reach module
 * which controller to display for a given category.
 */
@property(nonatomic, copy) NSString* category;

/** Reach content's body */
@property(nonatomic, copy) NSString* body;

/** Feedback required ? */
@property BOOL feedback;

/** URL to launch as an action */
@property(nonatomic, retain) NSString* actionURL;

/** The reach kind of this content, it match naming used by reach feedbacks */
- (NSString*)kind;

/** Drop content. */
- (void)drop;

/** Report content has been actioned. */
- (void)actionContent;

/** Report content has been exited.  */
- (void)exitContent;

/** @result YES if content is expired and should be dropped, NO otherwise */
- (BOOL)isExpired;

/**
 * Utility method to decode base64 data.
 * @param str The string to decode.
 */
- (NSData*)decodeBase64:(NSString*)str;

/**
 * Send feedback to reach about this content.
 * @param status The feedback status.
 * @param extras Extra information like poll answers.
 */
- (void)sendFeedback:(NSString*)status extras:(NSDictionary*)extras;

/**
 * Send content reply to the service that sent it, after that new contents can be notified.
 * @param status The feedback status.
 * @param extras Extra information like poll answers.
 */
- (void)process:(NSString*)status extras:(NSDictionary*)extras;

/**
 * Set or update payload.
 * @param payload New payload.
 */
- (void)setPayload:(NSDictionary*)payload;

/** Return true if download is needed */
- (BOOL)hasDLC;

/** Return true if download is needed after notification is clicked */
- (BOOL)hasContentDLC;

/** Return true if download is needed to display notification */
- (BOOL)hasNotificationDLC;

/** Return true if download is needed to display notification(this notification also has content to be shown after click) */
- (BOOL)hasNotificationAndContentDLC;

/**
 * Check if DLC completed.
 * @return true if DLC completed.
 */
- (BOOL)isDlcCompleted;

/**
 * Return dlc Id.
 * @return dlc Id.
 */
- (NSString*)getDlcId;

/**
 * Return unique push Id.
 * @return pushId.
 */
- (NSString*)getPushId;

@end