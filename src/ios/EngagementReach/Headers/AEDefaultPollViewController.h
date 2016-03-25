/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "AEPollViewController.h"

@class AEReachPoll;


/**
 * Default implementation of <AEPollViewController>.
 *
 * This view controller display a poll using layout view named `AEDefaultPollView.xib`.<br>
 * You can change the xib file to your needs as long as you keep view identifier and types.
 */
@interface AEDefaultPollViewController : AEPollViewController {
  @private
  BOOL _hasBody;
  NSMutableDictionary* _selectedChoices;
}

/** Submit's button */
@property(nonatomic, retain) UIBarButtonItem* submitButton;

/** Navigation bar displaying poll's title */
@property(nonatomic, retain) IBOutlet UINavigationBar* titleBar;

/** Table view responsible for displaying questions and choices */
@property(nonatomic, retain) IBOutlet UITableView* tableView;

/** Toolbar containing action and exit buttons */
@property(nonatomic, retain) IBOutlet UIToolbar* toolbar;

@end
