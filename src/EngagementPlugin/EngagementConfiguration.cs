/*
 * Copyright (c) Microsoft Corporation.  All rights reserved.
 * Licensed under the MIT license. See License.txt in the project root for license information.
 */

namespace Microsoft.Azure.Engagement.Unity 
{

	// Warning : every time this class is being modified, the File/Engagement/Generate Android Manifest must be executed

	public static class EngagementConfiguration
	{

	// Logging

		public const bool 	ENABLE_NATIVE_LOG = true;	
		public const bool 	ENABLE_PLUGIN_LOG = true; 	// Recommended to set both to true while working on the integration

	// Engagement

		public const string IOS_CONNECTION_STRING = null; // Must be filled for iOS
		public const bool 	IOS_DISABLE_IDFA = false;

		public const string ANDROID_CONNECTION_STRING = null; // Must be filled for Android
		public const string ANDROID_UNITY3D_ACTIVITY = null;	

	// Location reporting (cf. constant definitions in the Engagement Agent)

		public const LocationReportingType LOCATION_REPORTING_TYPE = LocationReportingType.NONE;
		public const LocationReportingMode LOCATION_REPORTING_MODE = LocationReportingMode.NONE;
		public const string LOCATION_REPORTING_DESCRIPTION = null ; // To define the message that would be displayed to the user to explain the usage of the locatin permission

	// Reach

		public const bool ENABLE_REACH = true;	// Set to true to enable reach feature

		public const string ACTION_URL_SCHEME = null;	// Define an URL scheme to be triggered by a campaign

		public const string IOS_REACH_ICON = null;	// filepath relative to the Assets/ directory if you want to use an icon other than the application icon
		public const string ANDROID_REACH_ICON = null;	// filepath relative to the Assets/ directory if you want to use an icon other than the application icon
		public const string ANDROID_GOOGLE_PROJECT_NUMBER = null;
		
	}
}

