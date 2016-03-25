/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

@class AEInteractiveContent;

/**
 * Abstract view controller used to display an announcement or a poll.
 */
@interface AEContentViewController : UIViewController

/**
 * Return the displayed content. Subclasses must override this method.
 * @result The associated content.
 */
- (AEInteractiveContent*)content;

/**
 * Report content as exited and dimiss this view controller.
 * Should be called by subclasses when the user clicks on the _exit_ button associated to
 * the announcement or poll.
 */
- (void)exit;

/**
 * Load a button toolbar used by poll and announcement views.
 * @param toolbar The native toolbar to load.
 */
- (void)loadToolbar:(UIToolbar*)toolbar;

/**
 * Method called when action button from a loaded toolbar has been clicked.
 * Sub-classes must re-implement this method.
 * @param sender The object that sent the action message.
 */
- (void)actionButtonClicked:(id)sender;

/**
 * Method called when exit button from a loaded toolbar has been clicked.
 * By default, this method will just call the <exit> method.
 * @param sender The object that sent the action message.
 */
- (void)exitButtonClicked:(id)sender;

/**
 * Method called when the action bar button item has been loaded.
 * Sub-classes can reimplement this method to customize the style of the action button.
 * @param actionButton The button used to action the campaign.
 */
- (void)actionButtonLoaded:(UIBarButtonItem*)actionButton;

/**
 * Method called when the exit bar button item has been loaded.
 * Sub-classes can reimplement this method to customize the style of the exit button.
 * @param exitButton The button used to exit the campaign.
 */
- (void)exitButtonLoaded:(UIBarButtonItem*)exitButton;

@end
