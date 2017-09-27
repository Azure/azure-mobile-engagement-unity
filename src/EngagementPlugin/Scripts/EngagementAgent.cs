/*
 * Copyright (c) Microsoft Corporation.  All rights reserved.
 * Licensed under the MIT license. See License.txt in the project root for license information.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;

#if UNITY_WSA && !UNITY_EDITOR
using Windows.ApplicationModel.Activation;
using Microsoft.Azure.Engagement;
#endif

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

		public const string PLUGIN_VERSION = "1.3.0";

		static EngagementAgent _instance = null;

		public  Action<Dictionary<string, object>> 	onStatusReceivedDelegate ;
	
		public static bool hasBeenInitialized = false;

#if UNITY_WSA && !UNITY_EDITOR
        public static string lastURI = null;
#endif

        private EngagementAgent()
		{

		}

		public static void Logging(string _message)
		{
			if (Microsoft.Azure.Engagement.Unity.EngagementConfiguration.ENABLE_PLUGIN_LOG)
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

#if UNITY_WSA && !UNITY_EDITOR
        public static void processURI()
        {
            if (hasBeenInitialized == false)
                return ;

            if (lastURI != null)
            {
                Debug.Log("processing "+lastURI);
                EngagementReach.onHandleURLMessage(lastURI);
                lastURI = null;
            }
        }
#endif

        public static void Initialize()
		{

#if UNITY_EDITOR
#elif UNITY_WSA
        //    Logging("Initialize");
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
#if UNITY_WSA && !UNITY_EDITOR
            processURI();
#endif
		}
	
		public static void StartActivity(string _activityName,Dictionary<object, object> _extraInfos = null)
		{
            Logging("startActivity:" + _activityName);
#if UNITY_WSA && !UNITY_EDITOR
            Microsoft.Azure.Engagement.EngagementAgent.Instance.StartActivity(_activityName, _extraInfos);
#else
            string _extraInfosJSON = MiniJSON.Json.Serialize(_extraInfos);
			EngagementWrapper.startActivity(_activityName,_extraInfosJSON);
#endif
        }

        public static void EndActivity()
		{
			Logging("endActivity");
#if UNITY_WSA && !UNITY_EDITOR
            Microsoft.Azure.Engagement.EngagementAgent.Instance.EndActivity();
#else
            EngagementWrapper.endActivity();
#endif
		}

		public static void StartJob(string _jobName,Dictionary<object, object> _extraInfos = null)
		{
            Logging("startJob:" + _jobName );
#if UNITY_WSA && !UNITY_EDITOR
            Microsoft.Azure.Engagement.EngagementAgent.Instance.StartJob(_jobName);
#else
            string extraInfosJSON = MiniJSON.Json.Serialize(_extraInfos);	
			EngagementWrapper.startJob(_jobName,extraInfosJSON);
#endif
        }

        public static void EndJob(string _jobName)
		{
            Logging("endJob:" + _jobName);
#if UNITY_WSA && !UNITY_EDITOR
            Microsoft.Azure.Engagement.EngagementAgent.Instance.EndJob(_jobName);
#else
            EngagementWrapper.endJob(_jobName);
#endif
		}

		public static void SendEvent(string _eventName, Dictionary<object, object> _extraInfos = null)
		{
            Logging("sendEvent:" + _eventName );
#if UNITY_WSA && !UNITY_EDITOR
            Microsoft.Azure.Engagement.EngagementAgent.Instance.SendEvent(_eventName,_extraInfos);
#else
            string extraInfosJSON = MiniJSON.Json.Serialize(_extraInfos);	
			EngagementWrapper.sendEvent(_eventName,extraInfosJSON);
#endif
        }

        public static void SendAppInfo( Dictionary<object, object> _extraInfos)
		{
            Logging("sendAppInfo");
#if UNITY_WSA && !UNITY_EDITOR
            Microsoft.Azure.Engagement.EngagementAgent.Instance.SendAppInfo(_extraInfos);
#else
            string extraInfosJSON = MiniJSON.Json.Serialize(_extraInfos);	
			EngagementWrapper.sendAppInfo(extraInfosJSON);
#endif
        }

        public static void SendSessionEvent(string _eventName, Dictionary<object, object> _extraInfos = null)
		{
            Logging("SendSessionEvent:" + _eventName );
#if UNITY_WSA && !UNITY_EDITOR
            Microsoft.Azure.Engagement.EngagementAgent.Instance.SendSessionEvent(_eventName,_extraInfos);
#else
            string extraInfosJSON = MiniJSON.Json.Serialize(_extraInfos);
            EngagementWrapper.sendSessionEvent(_eventName,extraInfosJSON);
#endif
        }

        public static void SendJobEvent(string _eventName, string _jobName, Dictionary<object, object> _extraInfos = null)
		{
            Logging("SendJobEvent:" + _eventName + ", Job: " + _jobName);
#if UNITY_WSA && !UNITY_EDITOR
            Microsoft.Azure.Engagement.EngagementAgent.Instance.SendJobEvent(_eventName, _jobName, _extraInfos);
#else
            string extraInfosJSON = MiniJSON.Json.Serialize(_extraInfos);	
			EngagementWrapper.sendJobEvent(_eventName,_jobName,extraInfosJSON);
#endif
        }

		public static void SendError(string _errorName, Dictionary<object, object> _extraInfos = null)
		{
            Logging("SendError:" + _errorName);
#if UNITY_WSA && !UNITY_EDITOR
            Microsoft.Azure.Engagement.EngagementAgent.Instance.SendError(_errorName, _extraInfos);
#else
            string extraInfosJSON = MiniJSON.Json.Serialize(_extraInfos);	
			EngagementWrapper.sendError(_errorName,extraInfosJSON);
#endif
        }

		public static void SendSessionError(string _errorName, Dictionary<object, object> _extraInfos = null)
		{
            Logging("SendSessionError:" + _errorName);
#if UNITY_WSA && !UNITY_EDITOR
            Microsoft.Azure.Engagement.EngagementAgent.Instance.SendSessionError(_errorName, _extraInfos);
#else
            string extraInfosJSON = MiniJSON.Json.Serialize(_extraInfos);	
			EngagementWrapper.sendSessionError(_errorName,extraInfosJSON);
#endif
        }

		public static void SendJobError(string _errorName, string _jobName, Dictionary<object, object> _extraInfos = null)
		{
            Logging("SendJobError:" + _errorName + ", Job: " + _jobName );
#if UNITY_WSA && !UNITY_EDITOR
            Microsoft.Azure.Engagement.EngagementAgent.Instance.SendJobError(_errorName, _jobName, _extraInfos);
#else
            string extraInfosJSON = MiniJSON.Json.Serialize(_extraInfos);	
			EngagementWrapper.sendJobError(_errorName,_jobName,extraInfosJSON);
#endif
        }

		public static void GetStatus(Action<Dictionary<string, object>> _onStatusReceived)
		{
			if (_onStatusReceived == null)
				Debug.LogError ("_onStatusReceived cannot be null");
			else
			{
#if UNITY_WSA && !UNITY_EDITOR
                Dictionary<string, object> status = new Dictionary<string, object>();
                status.Add("deviceId", Microsoft.Azure.Engagement.EngagementAgent.Instance.GetDeviceId());
                status.Add("pluginVersion", EngagementAgent.PLUGIN_VERSION);
                status.Add("nativeVersion", "3.4.0");
                status.Add("isEnabled", true);
                _onStatusReceived(status);
#else
                Instance().onStatusReceivedDelegate = _onStatusReceived;
                EngagementWrapper.getStatus();
#endif
			}
		}

		public static void SaveUserPreferences()
		{
#if UNITY_WSA && !UNITY_EDITOR
#else
			EngagementWrapper.saveUserPreferences ();
#endif
        }
		
		public static void RestoreUserPreferences()
		{
#if UNITY_WSA && !UNITY_EDITOR
#else
			EngagementWrapper.restoreUserPreferences ();
#endif
        }

		public static void SetEnabled(bool _enable)
		{
#if UNITY_WSA && !UNITY_EDITOR
#else
            EngagementWrapper.setEnabled (_enable);
#endif
        }

		// Delegate from Unity

		public void OnApplicationPause(bool pauseStatus)
		{
#if UNITY_WSA && !UNITY_EDITOR
#else
			EngagementWrapper.onApplicationPause (pauseStatus);
#endif
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

#if UNITY_WSA && !UNITY_EDITOR
        public static void initEngagement(IActivatedEventArgs args)
        { 
            if (EngagementConfiguration.ENABLE_PLUGIN_LOG == true)
                Microsoft.Azure.Engagement.EngagementAgent.Instance.TestLogLevel = EngagementTestLogLevel.Verbose;
            else
                Microsoft.Azure.Engagement.EngagementAgent.Instance.TestLogLevel = EngagementTestLogLevel.Off;

            Microsoft.Azure.Engagement.EngagementConfiguration engagementConfiguration = new Microsoft.Azure.Engagement.EngagementConfiguration();
            engagementConfiguration.Agent.ConnectionString = EngagementConfiguration.WINDOWS_CONNECTION_STRING;
            engagementConfiguration.Reach.EnableNativePush = true;
            Microsoft.Azure.Engagement.EngagementAgent.Instance.Init(args, engagementConfiguration);
            Microsoft.Azure.Engagement.EngagementReach.Instance.Init(args);

            Microsoft.Azure.Engagement.EngagementReach.Instance.DataPushStringReceived += (body) =>
            {
                EngagementReach.onDataPushString(null, body);
                return true;
            };

            Microsoft.Azure.Engagement.EngagementReach.Instance.DataPushBase64Received += (decodedBody, encodedBody) =>
            {
                EngagementReach.onDataPushBase64(null, decodedBody, encodedBody);
                return true;
            };
        }

        public static void initEngagementOnActivated(IActivatedEventArgs args)
        { 
            if (args.Kind == ActivationKind.Protocol)
            {
                ProtocolActivatedEventArgs eventArgs = args as ProtocolActivatedEventArgs;
                lastURI = eventArgs.Uri.AbsoluteUri;
                Logging("Got URI "+lastURI);
                processURI();
             }
            initEngagement(args);
        }
#endif
    }

}

#pragma warning restore 162, 429
