
/*
 * Copyright (c) Microsoft Corporation.  All rights reserved.
 * Licensed under the MIT license. See License.txt in the project root for license information.
 */

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public  class SampleSetup 
{

	static void CreateSampleScene ()
	{
		EditorApplication.NewEmptyScene ();
		new GameObject("EngagementCamera").AddComponent<Camera>();
		GameObject go = new GameObject("EngagementSample");
		go.AddComponent<Sample>();
		string sn = "Assets/SampleScene.unity";
		EditorApplication.SaveScene (sn);
		var sceneToAdd = new EditorBuildSettingsScene(sn, true); 
		EditorBuildSettings.scenes = new EditorBuildSettingsScene[1]{sceneToAdd};
	}

}

