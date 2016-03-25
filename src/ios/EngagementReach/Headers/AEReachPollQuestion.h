/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import <Foundation/Foundation.h>

/**
 * The `AEReachPollQuestion` class defines objects that represent poll's questions
 */
@interface AEReachPollQuestion : NSObject {
  @private
  NSString* _questionId;
  NSString* _title;
  NSArray* _choices;
}

/**
 * Create and return a poll's question.
 * @param qId The unique identifier for the quetion.
 * @param title The question's title.
 * @param choices The question's choices.
 */
- (id)initWithId:(NSString*)qId title:(NSString*)title choices:(NSArray*)choices;

/** The unique question identifier */
@property(readonly) NSString* questionId;

/** Localized question text */
@property(readonly) NSString* title;

/**
 * Choices.<br>
 * Contains <AEReachPollChoice> objects
 */
@property(readonly) NSArray* choices;

@end

/**
 * The `AEReachPollQuestion` class defines objects that represent poll's choices
 */
@interface AEReachPollChoice : NSObject {
  @private
  NSString* _choiceId;
  NSString* _title;
  BOOL _isDefault;
}

/**
 * Create and return a question's choice.
 * @param cId The unique identifier for the choice.
 * @param title The choice's title.
 * @param isDefault Is this the default choice for the associated question.
 */
- (id)initWithId:(NSString*)cId title:(NSString*)title isDefault:(BOOL)isDefault;

/** The unique choice identifier */
@property(readonly) NSString* choiceId;

/** The localized choice text */
@property(readonly) NSString* title;

/** YES if this choice is the default one for the associated question. */
@property(readonly) BOOL isDefault;
@end
