/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

/**
 * Helper class used to replace iOS's `UIViewController` class.
 */
@interface EngagementViewController : UIViewController {

}

/**
 * Override this to specify the name reported by your activity. The default implementation returns
 * the simple name of the class and removes the "ViewController" suffix if any (e.g.
 * "Tab1ViewController" -> "Tab1").
 * @result the activity name reported by the Engagement service.
 */
- (NSString*)engagementActivityName;

/**
 * Override this to attach extra information to your activity. The default implementation attaches
 * no extra information (i.e. return nil).
 * @result activity extra information, nil if no extra.
 */
- (NSDictionary*)engagementActivityExtra;

@end
