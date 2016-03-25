/*
 * Copyright (c) Microsoft Corporation.  All rights reserved.
 * Licensed under the MIT license. See License.txt in the project root for license information.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;


#pragma warning disable 162,429

namespace Microsoft.Azure.Engagement.Unity
{
	
	public enum LocationReportingType 
	{
		NONE=100,
		LAZY=101,
		REALTIME=102,
		FINEREALTIME=103
	} ;
	
	public enum LocationReportingMode 
	{
		NONE=200,
		FOREGROUND=201,
		BACKGROUND=202
	} ;
	
	public class EngagementAgent : MonoBehaviour
	{
		public const string PLUGIN_VERSION = "1.0.0";

		static EngagementAgent _instance = null;

		public  Action<Dictionary<string, object>> 	onStatusReceivedDelegate ;
	
		public static bool hasBeenInitialized = false;
		
		private EngagementAgent()
		{

		}

		public static void Logging(string _message)
		{
			if (EngagementConfiguration.ENABLE_PLUGIN_LOG)
				Debug.Log("[Engagement] " + _message);
		}
		
		public static EngagementAgent Instance()
		{
			if (null == _instance)
			{
				_instance = (EngagementAgent)GameObject.FindObjectOfType(typeof(EngagementAgent));
				if (null == _instance)
				{
					GameObject go = new GameObject("EngagementAgentObj");
					_instance = go.AddComponent(typeof(EngagementAgent)) as EngagementAgent;
					Logging("Initializing EngagementAgent v"+PLUGIN_VERSION);
				}
			}
			return _instance;
		}

		public static void Initialize()
		{
	
#if UNITY_EDITOR
#elif UNITY_IPHONE
			EngagementWrapper.initializeEngagement(Instance().name);
#elif UNITY_ANDROID
			string connectionString = EngagementConfiguration.ANDROID_CONNECTION_STRING;
			if (string.IsNullOrEmpty(connectionString))
				throw new ArgumentException("ANDROID_CONNECTION_STRING cannot be null");
			EngagementWrapper.registerApp (
									Instance().name,
									connectionString,
									(int)EngagementConfiguration.LOCATION_REPORTING_TYPE,
									(int)EngagementConfiguration.LOCATION_REPORTING_MODE,
									EngagementConfiguration.ENABLE_PLUGIN_LOG
									);
#endif
			hasBeenInitialized = true;
		}
	
		public static void StartActivity(string _activityName,Dictionary<object, object> _extraInfos = null)
		{
			string _extraInfosJSON = MiniJSON.Json.Serialize(_extraInfos);
			Logging("startActivity:"+ _activityName+", "+_extraInfosJSON);
			EngagementWrapper.startActivity(_activityName,_extraInfosJSON);
		}

		public static void EndActivity()
		{
			Logging("endActivity");	
			EngagementWrapper.endActivity();
		}

		public static void StartJob(string _jobName,Dictionary<object, object> _extraInfos = null)
		{
			string _extraInfosJSON = MiniJSON.Json.Serialize(_extraInfos);
			Logging("startJob:"+ _jobName+", "+_extraInfosJSON);
			EngagementWrapper.startJob(_jobName,_extraInfosJSON);
		}

		public static void EndJob(string _jobName)
		{
			Logging("endJob:"+ _jobName);
			EngagementWrapper.endJob(_jobName);
		}

		public static void SendEvent(string _eventName, Dictionary<object, object> _extraInfos = null)
		{
			string _extraInfosJSON = MiniJSON.Json.Serialize(_extraInfos);
			Logging("sendEvent:"+ _eventName+" ,"+_extraInfosJSON);
			EngagementWrapper.sendEvent(_eventName,_extraInfosJSON);
		}

		public static void SendAppInfo( Dictionary<object, object> _extraInfos)
		{
			string extraInfosJSON = MiniJSON.Json.Serialize(_extraInfos);	
			Logging("sendAppInfo:"+extraInfosJSON);
			EngagementWrapper.sendAppInfo(extraInfosJSON);
		}

		public static void SendSessionEvent(string _eventName, Dictionary<object, object> _extraInfos = null)
		{
			string extraInfosJSON = MiniJSON.Json.Serialize(_extraInfos);	
			Logging("SendSessionEvent:"+_eventName+" ,"+extraInfosJSON);
			EngagementWrapper.sendSessionEvent(_eventName,extraInfosJSON);
		}

		public static void SendJobEvent(string _eventName, string _jobName, Dictionary<object, object> _extraInfos = null)
		{
			string extraInfosJSON = MiniJSON.Json.Serialize(_extraInfos);	
			Logging("SendJobEvent:"+_eventName+", Job: "+_jobName+" ,"+extraInfosJSON);
			EngagementWrapper.sendJobEvent(_eventName,_jobName,extraInfosJSON);
		}

		public static void SendError(string _errorName, Dictionary<object, object> _extraInfos = null)
		{
			string extraInfosJSON = MiniJSON.Json.Serialize(_extraInfos);	
			Logging("SendError:"+_errorName+" ,"+extraInfosJSON);
			EngagementWrapper.sendError(_errorName,extraInfosJSON);
		}

		public static void SendSessionError(string _errorName, Dictionary<object, object> _extraInfos = null)
		{
			string extraInfosJSON = MiniJSON.Json.Serialize(_extraInfos);	
			Logging("SendSessionError:"+_errorName+" ,"+extraInfosJSON);
			EngagementWrapper.sendSessionError(_errorName,extraInfosJSON);
		}

		public static void SendJobError(string _errorName, string _jobName, Dictionary<object, object> _extraInfos = null)
		{
			string extraInfosJSON = MiniJSON.Json.Serialize(_extraInfos);	
			Logging("SendJobError:"+_errorName+", Job: "+_jobName+" ,"+extraInfosJSON);
			EngagementWrapper.sendJobError(_errorName,_jobName,extraInfosJSON);
		}

		public static void GetStatus(Action<Dictionary<string, object>> _onStatusReceived)
		{
			if (_onStatusReceived == null)
				Debug.LogError ("_onStatusReceived cannot be null");
			else
			{
				Instance().onStatusReceivedDelegate = _onStatusReceived;
				EngagementWrapper.getStatus();
			}
		}

		public static void SaveUserPreferences()
		{
			EngagementWrapper.saveUserPreferences ();
		}
		
		public static void RestoreUserPreferences()
		{
			EngagementWrapper.restoreUserPreferences ();
		}

		public static void SetEnabled(bool _enable)
		{
			EngagementWrapper.setEnabled (_enable);
		}

		// Delegate from Unity

		public void OnApplicationPause(bool pauseStatus)
		{
			Logging ("OnApplicationPause:" + pauseStatus);
			EngagementWrapper.onApplicationPause (pauseStatus);
		}

		// Delegates from Native

		public  void onStatusReceived(string _serialized)
		{
			Logging ("OnStatusReceived: " + _serialized);
			Dictionary<string, object> dict = (Dictionary<string, object>)MiniJSON.Json.Deserialize (_serialized);
			if (onStatusReceivedDelegate != null) {
				onStatusReceivedDelegate (dict);
				onStatusReceivedDelegate = null;
			}
		}

		// Forward the messages to the ReachAgent

		public  void onDataPushReceived(string _serialized)
		{
			EngagementReach.onDataPushMessage (_serialized);

		}
		
		public  void onHandleURL(string _url)
		{
			EngagementReach.onHandleURLMessage (_url);
	
		}

	}
	
}

#pragma warning restore 162,429
