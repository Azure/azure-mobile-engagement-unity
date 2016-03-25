
/*
 * Copyright (c) Microsoft Corporation.  All rights reserved.
 * Licensed under the MIT license. See License.txt in the project root for license information.
 */

using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Schema;
using System.IO;
using System.Collections.Generic;
using Microsoft.Azure.Engagement.Unity;

public class Sample : MonoBehaviour 
{

	private string log = "";
	private int maxLine =  0;

	void Start () 
	{
		Display("Engagement Sample");
		EngagementAgent.Initialize ();

		EngagementReach.HandleURL += (string _push) => {
			Display ("HandleURL " + _push);
		};

		EngagementReach.StringDataPushReceived += (string _category, string _body) => {
			Display ("StringDataPushReceived category:" + _category );
		};

		EngagementReach.Base64DataPushReceived += (string _category, byte[] _data, string _body) => {
			Display("Base64DataPushReceived category:" + _category);
		};
		EngagementReach.Initialize ();

		EngagementAgent.StartActivity ("home");
		EngagementAgent.GetStatus (OnStatusReceived);
	}

	void OnStatusReceived(Dictionary<string, object> _status)
	{
		Display ("deviceId:"+_status["deviceId"]);
		Display ("pluginVersion:"+_status["pluginVersion"]);
		Display ("nativeVersion:"+_status["nativeVersion"]);
	}
	
	public void Display(string str)
	{
		Debug.Log (str);

		maxLine++;
		if (maxLine == 6) {
			maxLine = 0;
			log = str;
		}
		else
			log = log +"\n"+str;
		
	}

	public void OnGUI() {
		GUIStyle myStyle = new GUIStyle();
		myStyle.fontSize = 32;
		Rect r = new Rect ( Screen.width/4,  Screen.height/4, Screen.width / 2, Screen.height / 2);
		GUI.Box(r,log,myStyle);
	
	}
}
