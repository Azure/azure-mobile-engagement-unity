/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

/**
 * The `AEViewControllerUtil` provides utility methods to present and dismiss view controllers on screen.
 */
@interface AEViewControllerUtil : NSObject

/**
 * Present the controller's view inside the key window using an optional vertical cover animation:
 * The view slides up from the bottom of the screen.
 * The given controller will be retained until method <dismissViewController:> is called.
 * @param controller The view controller to present.
 * @param animated If YES, animates the view; otherwise, does not.
 */
+ (void)presentViewController:(UIViewController*)controller animated:(BOOL)animated;

/**
 * Dismiss the given view controller. Remove the view from it's parent using a vertical slide animation.
 * The controller is released.
 * @param controller The view controller to dismiss.
 * @see dismissViewController:animated:
 */
+ (void)dismissViewController:(UIViewController*)controller;

/**
 * Dismiss the given view controller.
 * The controller is released.
 * @param controller The view controller to dismiss
 * @param animated If YES, animates the view; otherwise, does not.
 * @see dismissViewController:
 */
+ (void)dismissViewController:(UIViewController*)controller animated:(BOOL)animated;

/**
 * Get an available window for this application.
 */
+ (UIWindow*)availableWindow;

@end
