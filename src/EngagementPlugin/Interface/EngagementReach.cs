/*
 * Copyright (c) Microsoft Corporation.  All rights reserved.
 * Licensed under the MIT license. See License.txt in the project root for license information.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;

#pragma warning disable 162

namespace Microsoft.Azure.Engagement.Unity
{
	
	public class EngagementReach : MonoBehaviour
	{

		/// <summary>
		/// Event when receiving a string datapush.
		///
		/// The callback must of the form :
		/// DataPushStringReceived(<b>string category</b>, <b>string body</b>) {}
		/// </summary>
		/// 
		public  static Action<string,string> 			StringDataPushReceived ;
		/// <summary>
		/// Event when receiving a base64 datapush.
		///
		/// The callback must of the form :
		/// DataPushStringReceived(<b>string category</b>, <b>byte[] decodedBody</b>, <b>string encodedBody</b>) {}
		/// </summary>
		/// 
		public  static Action<string,byte[],string>		Base64DataPushReceived ;

		/// <summary>
		/// Event when receiving an openUrl event
		///
		/// The callback must of the form :
		/// HandleURL(<b>string url</b>) {}
		/// </summary>
		/// 
		public  static Action<string> 					HandleURL ;

		private EngagementReach()
		{

		}

		public static void Initialize( )
		{
			EngagementAgent.Logging ("Initializing Reach");

			if (EngagementConfiguration.ENABLE_REACH == false)
			{
				Debug.LogError ("Reach must be enabled in configuration first");
				return;
			}

			if (EngagementAgent.hasBeenInitialized == false) 
			{
				Debug.LogError ("Agent must be initialized before initializing Reach");
				return ;
			}

			EngagementWrapper.initializeReach();
		}

		// Delegates from Native

		public static void onDataPushMessage(string _serialized)
		{
			Dictionary<string, object> dict = (Dictionary<string, object>)MiniJSON.Json.Deserialize(_serialized);
			string category = null;
			string body = null;
			bool isBase64 = (bool)dict ["isBase64"];
			if (dict["category"] != null)
				category = WWW.UnEscapeURL (dict["category"].ToString(),System.Text.Encoding.UTF8);

			EngagementAgent.Logging ("DataPushReceived, category: " + category+", isBase64:"+isBase64);

			if (isBase64 == false) {
				if (StringDataPushReceived != null)
				{
					body = WWW.UnEscapeURL (dict["body"].ToString(),System.Text.Encoding.UTF8);
					StringDataPushReceived (category, body);
				}
				else
					EngagementAgent.Logging ("WARNING: unitialized StringDataPushReceived");

			} else {
				if (Base64DataPushReceived != null) {
					body = dict ["body"].ToString ();
					byte[] data = Convert.FromBase64String (body);
					Base64DataPushReceived (category, data,body);
				}
				else
					EngagementAgent.Logging ("WARNING: unitialized Base64DataPushReceived");
			}
		}

		public static  void onHandleURLMessage(string _url)
		{
			EngagementAgent.Logging ("OnHandleURL: " + _url);
			if (HandleURL != null)
				HandleURL (_url);
			else
				EngagementAgent.Logging ("WARNING: unitialized HandleURL");
		}
	}
	
}

#pragma warning restore 162
