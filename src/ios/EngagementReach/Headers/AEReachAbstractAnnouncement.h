/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import <Foundation/Foundation.h>
#import "AEInteractiveContent.h"

/** Announcement kind */
static NSString* const kAEAnnouncementKind = @"a";

/**
 * The `AEReachAbstractAnnouncement` is a base class for all kind of announcements.
 */
@interface AEReachAbstractAnnouncement : AEInteractiveContent
{
}

/**
 * Initialize an abstract announcement. Should only be called by subclasses.
 * @param reachValues Parsed JSON reach values.
 * @param params special parameters to replace in the action URL.
 * @result An initialized abstract announcement or nil if it couldn't be parsed.
 */
- (id)initWithReachValues:(NSDictionary*)reachValues params:(NSDictionary*)params;

/** URL to launch as an action */
@property(nonatomic, retain) NSString* actionURL;

@end
