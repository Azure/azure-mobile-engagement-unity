/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import <Foundation/Foundation.h>

/**
 * Data storage used by Engagement.
 */
@interface AEStorage : NSObject<NSFastEnumeration>

/**
 * Create a new storage.
 * @param name The name of the store
 * @param version The version of the store
 * @param autoSync If `true` every operation on the store will be automatically written to the persistent storage.
 * Otherwise, the synchronize method should be called manually. You should consider synchronizing the store manually
 */
+ (instancetype)storageWithName:(NSString*)name version:(double)version autoSync:(BOOL)autoSync;

/**
 * Create a new storage.
 * @param name The name of the store
 * @param autoSync If `true` every operation on the store will be automatically written to the persistent storage.
 * Otherwise, the synchronize method should be called manually. You should consider synchronizing the store manually
 * @param capacity max number of enteries of the storage
 */
+ (instancetype)userDefaultstorageWithName:(NSString*)name autoSync:(BOOL)autoSync capacity:(NSUInteger)capacity;

/**
 * Set the maximum capacity of the store.
 * @param capacity Maximum number of entries to keep.
 */
- (void)setCapacity:(NSUInteger)capacity;

/**
 * Put a new entry in the store.
 * @param value Object to store. Should conform to `NSCoding`.
 * @result A unique identifier associated to the new entry.
 */
- (NSUInteger)put:(id)value;

/**
 * Put all entries in the store.
 * @param values Objects to store.
 * @result Unique identifiers associated to entries.
 */
- (NSIndexSet*)putAll:(NSArray*)values;

/**
 * Remove an existing value from the store based on its unique identifier
 * @param key Key's value to remove
 */
- (void)remove:(NSUInteger)key;

/** Clear any data from the store. Clear in memory data and remove persistant storage. */
- (void)clear;

/** Causes all in-memory data to be written to the persistent storage. */
- (void)synchronize;

/**
 * Makes the given object the cache’s delegate.
 * @param del The object to be registered as the delegate.
 */
- (void)setDelegate:(id)del;

@end

/**
 * A `AEStorageEntry` object represents an entry in the <AEStorage>
 *
 * An object of this type is returned while enumerating through a <AEStorage> collection.
 */
@interface AEStorageEntry : NSObject<NSCoding>

/** Unique identifier of the associated data */
@property(nonatomic, assign) NSUInteger uid;

/** Underlying data */
@property(nonatomic, retain) NSObject<NSCoding>* data;

/**
 * Create a new storage entry.
 * @param uid Unique identifier of the entry.
 * @param data The underlying data object.
 */
+ (instancetype)dataWithId:(NSUInteger)uid data:(NSObject<NSCoding>*)data;

@end

/**
 * The delegate of an NSCache object implements this protocol to perform specialized actions
 * when an object is about to be evicted from the cache.
 */
@protocol AEStorageDelegate<NSObject>

/**
 * Called when an object is evicted from the cache.
 * @param storage The storage with which the object of interest is associated.
 * @param entry The entry of interest in the storage.
 */
+ (void)storage:(AEStorage*)storage didEvictEntry:(AEStorageEntry*)entry;

@end
