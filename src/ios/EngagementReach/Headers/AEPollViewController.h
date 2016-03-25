/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import "AEContentViewController.h"

@class AEReachPoll;

/**
 * Abstract view controller displaying a Engagement poll.
 *
 * By inheriting from this class you can create your own view controller to display polls.
 */
@interface AEPollViewController : AEContentViewController

/**
 * Init the view controller with the given poll.
 * Subclasses should re-implement this method.
 * @param poll Poll to display
 */
- (instancetype)initWithPoll:(AEReachPoll*)poll;

/**
 * Submit answers for the associated poll and dismiss this view controller.<br>
 * Dictionary keys must be the question ids and values must be the associated choice ids.
 * Should be called by subclasses when the user clicks on the 'action' button associated to
 * the poll.
 * @param answers The poll answers to submit.
 */
- (void)submitAnswers:(NSDictionary*)answers;

/**
 * Use this property to store poll information when the initWithPoll:
 * method is called.
 * Subclasses should also read this property to init their subviews.
 */
@property(nonatomic, retain) AEReachPoll* poll;

@end
