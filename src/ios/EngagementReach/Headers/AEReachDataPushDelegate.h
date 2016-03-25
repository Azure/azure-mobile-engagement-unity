/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import <Foundation/Foundation.h>

/**
 * The `AEReachDataPushDelegate` protocol defines methods a delegate of <AEReachModule> should implement to receive data
 * pushes.
 *
 * To process data push, you must implement method <onDataPushBase64ReceivedWithDecodedBody:andEncodedBody:>
 * if it's a file upload or a base64 data, otherwise you implement <onDataPushStringReceived:>.<br>
 * To use it in your application just call the method <[AEReachModule dataPushDelegate]> after module
 * initialization.
 */
@protocol AEReachDataPushDelegate <UIAlertViewDelegate>

@optional

/**
 * This function is called when a datapush of type text has been received.
 * @param category short string describing your data to push
 * @param body Your content.
 * @result YES to acknowledge the content, NO to cancel.
 **/
- (BOOL)didReceiveStringDataPushWithCategory:(NSString*)category body:(NSString*)body;

/**
 * This function is called when a datapush of type base64 has been received.
 * @param category short string describing your data to push
 * @param decodedBody Your base64 content decoded.
 * @param encodedBody Your base64 content encoded.
 * @result YES to acknowledge the content, NO to cancel.
 **/
- (BOOL)didReceiveBase64DataPushWithCategory:(NSString*)category decodedBody:(NSData*)decodedBody encodedBody:(NSString*)
  encodedBody;

@end
