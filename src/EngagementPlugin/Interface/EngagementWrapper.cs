/*
 * Copyright (c) Microsoft Corporation.  All rights reserved.
 * Licensed under the MIT license. See License.txt in the project root for license information.
 */

using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Azure.Engagement.Unity
{
	internal class EngagementWrapper
    {

#if UNITY_EDITOR

		public static  void initializeReach() { }
		public static  void startActivity( string _activityName, string _extraInfos) { }
		public static  void endActivity() { }
		public static  void startJob( string _jobName, string _extraInfos) { }
		public static  void endJob( string _jobName ) { }
		public static  void sendEvent( string _eventName, string _extraInfos) { }
		public static  void sendAppInfo( string _extraInfos) { }
		public static  void sendSessionEvent(string _eventName, string _extraInfosJSON) { }
		public static  void sendJobEvent(string _eventName, string _jobName,string _extraInfosJSON) { }
		public static  void sendError(string _errorName, string _extraInfosJSON) { }
		public static  void sendSessionError(string _errorName, string _extraInfosJSON) { }
		public static  void sendJobError(string _errorName, string _jobName, string _extraInfosJSON) { }
		public static  void registerForPushNotification()  { }
		public static  void onApplicationPause(bool _paused ) { }
		public static  void setEnabled(bool _enabled ) { }
		public static  void getStatus() 
		{ 
			Dictionary<string, object> status = new Dictionary<string, object>();
			status.Add("deviceId", "UnityEditor");
			status.Add("pluginVersion", EngagementAgent.PLUGIN_VERSION);
			status.Add("nativeVersion", "0");
			status.Add("isEnabled", false);
			string serialized = MiniJSON.Json.Serialize(status);	
			EngagementAgent.Instance().onStatusReceived (serialized);
		}
		public	static void saveUserPreferences() {}
		public	static void restoreUserPreferences() {}

 #elif UNITY_IPHONE 
   
		[DllImport("__Internal")]
		public static extern void initializeEngagement(string _instanceName );

		[DllImport("__Internal")]
		public static extern void initializeReach();

		[DllImport("__Internal")]
		public static extern void startActivity(string _activityName,string _extraInfos);

		[DllImport("__Internal")]
		public static extern void endActivity();

		[DllImport("__Internal")]
		public static extern void startJob(string _jobName,string _extraInfos);
		
		[DllImport("__Internal")]
		public static extern void endJob(string _jobName);

		[DllImport("__Internal")]
		public static extern void sendEvent(string _eventName,string _extraInfos);

		[DllImport("__Internal")]
		public static extern void sendAppInfo(string _extraInfos);

		[DllImport("__Internal")]
		public static extern void sendSessionEvent(string _eventName, string _extraInfosJSON);

		[DllImport("__Internal")]
		public static extern void sendJobEvent(string _eventName, string _jobName,string _extraInfosJSON);

		[DllImport("__Internal")]
		public static extern void sendError(string _errorName, string _extraInfosJSON);

		[DllImport("__Internal")]
		public static extern void sendSessionError(string _errorName, string _extraInfosJSON);

		[DllImport("__Internal")]
		public static extern void sendJobError(string _errorName, string _jobName, string _extraInfosJSON);

		[DllImport("__Internal")]
		public static extern void getStatus();

		[DllImport("__Internal")]
		public static extern void saveUserPreferences();

		[DllImport("__Internal")]
		public static extern void restoreUserPreferences();

		[DllImport("__Internal")]
		public static extern void setEnabled(bool _enabled);

		[DllImport("__Internal")]
		public static extern void onApplicationPause(bool _paused );

#elif UNITY_ANDROID

		static AndroidJavaClass activityJavaClass = null;
		static public AndroidJavaClass javaClass
		{
			get 
			{ 
				if (null == activityJavaClass)
				{  			
					AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
					AndroidJavaObject activity = player.GetStatic<AndroidJavaObject>("currentActivity");	
					
					/*Set Activity, will be used to retrieve URL scheme. */ 	
					activityJavaClass = new AndroidJavaClass("com.microsoft.azure.engagement.unity.EngagementWrapper");
					activityJavaClass.CallStatic("setAndroidActivity", activity);
				}
				return activityJavaClass;
			}
		}

		public static void registerApp(string _instanceName, string _connectionString,  int _locationType, int  _locationMode, bool _enablePluginLog )
		{
		javaClass.CallStatic("registerApp",_instanceName,_connectionString, _locationType,_locationMode,_enablePluginLog);
		}

		public static void initializeReach()
		{
			javaClass.CallStatic("initializeReach");
								 
		}

		public static void startActivity(string _activityName, string _extraInfos) 
		{
			javaClass.CallStatic("startActivity",_activityName, _extraInfos);
		}

		public static void endActivity() 
		{
			javaClass.CallStatic("endActivity");
		}

		public static void startJob(string _jobName, string _extraInfos) 
		{
			javaClass.CallStatic("startJob",_jobName, _extraInfos);
		}
		
		public static void endJob(string _jobName) 
		{
			javaClass.CallStatic("endJob",_jobName);
		}

		public static void sendEvent(string _eventName, string _extraInfos) 
		{
			javaClass.CallStatic("sendEvent",_eventName,_extraInfos);
		}

		public static void sendAppInfo( string _extraInfos) 
		{
			javaClass.CallStatic("sendAppInfo",_extraInfos);
		}

		public static  void sendSessionEvent(string _eventName, string _extraInfos)
		{ 
			javaClass.CallStatic("sendSessionEvent",_eventName,_extraInfos);
		}

		public static  void sendJobEvent(string _eventName, string _jobName,string _extraInfos)
		{
			javaClass.CallStatic("sendJobEvent",_eventName,_jobName,_extraInfos);
		}

		public static  void sendError(string _errorName, string _extraInfos)
		{
			javaClass.CallStatic("sendError",_errorName,_extraInfos);
		}

		public static  void sendSessionError(string _errorName, string _extraInfos)
		{
			javaClass.CallStatic("sendSessionError",_errorName,_extraInfos);
		}

		public static  void sendJobError(string _errorName, string _jobName, string _extraInfos) 
		{
			javaClass.CallStatic("sendJobError",_errorName,_jobName,_extraInfos);
		}

		public static void getStatus( ) 
		{
			javaClass.CallStatic("getStatus");
		}

		public static void saveUserPreferences( ) 
		{

		}

		public static void restoreUserPreferences( ) 
		{

		}

		public static void setEnabled( bool _enabled) 
		{
			javaClass.CallStatic("setEnabled",_enabled);
		}

		public static void onApplicationPause(bool _paused) 
		{
			javaClass.CallStatic("onApplicationPause",_paused);
		}
	
#else
#	error "unsupported platform"
#endif

    }
}