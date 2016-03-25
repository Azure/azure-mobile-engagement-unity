/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

/**
 * A `AENotificationView` object represnts a view that is responsible for displaying reach notifications used by the
 * default notifier <AEDefaultNotifier>.
 *
 * This view is using the layout named `AEDefaultNotificationView.xib`.
 * You can change the xib file to your needs as long as you keep view identifier and types.<br>
 * This class overrides the method `layoutSubviews` to move and resize subviews when some of them are hidden.
 */
@interface AENotificationView : UIView
{
  @private
  UILabel* _titleView;
  UILabel* _messageView;
  UIImageView* _iconView;
  UIImageView* _imageView;
  UIButton* _notificationButton;
  UIButton* _closeButton;
}

/** Returns the title view of the notification. */
@property(nonatomic, readonly) UILabel* titleView;

/** Returns the message view of the notification. */
@property(nonatomic, readonly) UILabel* messageView;

/** Returns the icon view of the notification. */
@property(nonatomic, readonly) UIImageView* iconView;

/** Returns the image view of the notification. */
@property(nonatomic, readonly) UIImageView* imageView;

/** Returns the main button of the notification. */
@property(nonatomic, readonly) UIButton* notificationButton;

/** Returns the close button of the notification. */
@property(nonatomic, readonly) UIButton* closeButton;

@end
