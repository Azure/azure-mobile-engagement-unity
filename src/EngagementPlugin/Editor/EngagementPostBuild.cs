/*
 * Copyright (c) Microsoft Corporation.  All rights reserved.
 * Licensed under the MIT license. See License.txt in the project root for license information.
 */

#if UNITY_EDITOR_OSX
#define ENABLE_IOS_SUPPORT
#endif

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;

#if ENABLE_IOS_SUPPORT
using UnityEditor.iOS.Xcode;
#endif
using System.Xml;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using Ionic.Zip;
using System;

using Microsoft.Azure.Engagement.Unity;

#pragma warning disable 162,429

public class EngagementPostBuild {
	
	public const string androidNS = "http://schemas.android.com/apk/res/android";
	const string tagName = "Engagement";

	static string[] GetScenePaths()
	{   
		List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
		List<string> enabledScenes = new List<string>();
		foreach (EditorBuildSettingsScene scene in scenes)
		{
			if (scene.enabled)
				enabledScenes.Add(scene.path);
		}
		return enabledScenes.ToArray();
	}

	[MenuItem("File/Engagement/Generate Android Manifest")]
	static void GenerateManifest ()
	{
	
		EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.Android);
		
		string path = Application.temporaryCachePath + "/EngagementManifest";
		if (Directory.Exists(path))
			Directory.Delete (path,true);
	
		BuildPipeline.BuildPlayer(GetScenePaths(), path,BuildTarget.Android,BuildOptions.AcceptExternalModificationsToPlayer);
	}

	public static void addMetaData(XmlDocument doc, XmlNode node, XmlNamespaceManager namespaceManager,string name, string value)
	{
		XmlNode metaData = doc.CreateNode (XmlNodeType.Element, "meta-data", null);
		metaData.Attributes.Append (doc.CreateAttribute ("tag")).Value = tagName;
		metaData.Attributes.Append (doc.CreateAttribute ("android", "name", EngagementPostBuild.androidNS)).Value = name;
		metaData.Attributes.Append (doc.CreateAttribute ("android", "value", EngagementPostBuild.androidNS)).Value = value;
		node.AppendChild (metaData);
	}

	public static void addUsesPermission(XmlDocument doc, XmlNode node, XmlNamespaceManager namespaceManager,string permissionName)
	{
		XmlNode usesPermission = doc.CreateNode(XmlNodeType.Element, "uses-permission", null);
		usesPermission.Attributes.Append (doc.CreateAttribute ("tag")).Value = tagName;
		usesPermission.Attributes.Append (doc.CreateAttribute ("android","name",EngagementPostBuild.androidNS)).Value = permissionName;
		node.AppendChild (usesPermission);
	}

	public static void addActivity(XmlDocument doc, XmlNode node, XmlNamespaceManager namespaceManager,string activityName, string actionName, string theme,string mimeType)
	{

		XmlNode engagementTextAnnouncementActivity = doc.CreateNode (XmlNodeType.Element, "activity", null);
		engagementTextAnnouncementActivity.Attributes.Append (doc.CreateAttribute ("tag")).Value = tagName;
		engagementTextAnnouncementActivity.Attributes.Append (doc.CreateAttribute ("android", "theme", EngagementPostBuild.androidNS)).Value = "@android:style/Theme."+theme;
		engagementTextAnnouncementActivity.Attributes.Append (doc.CreateAttribute ("android", "name", EngagementPostBuild.androidNS)).Value = "com.microsoft.azure.engagement.reach.activity."+activityName;
		
		XmlNode engagementTextAnnouncementIntent = doc.CreateNode (XmlNodeType.Element, "intent-filter", null);
		engagementTextAnnouncementIntent.Attributes.Append (doc.CreateAttribute ("tag")).Value = tagName;
		engagementTextAnnouncementIntent.AppendChild (doc.CreateNode (XmlNodeType.Element, "action", null))
			.Attributes.Append (doc.CreateAttribute ("android", "name", EngagementPostBuild.androidNS)).Value = "com.microsoft.azure.engagement.reach.intent.action."+actionName;
		engagementTextAnnouncementIntent.AppendChild (doc.CreateNode (XmlNodeType.Element, "category", null))
			.Attributes.Append (doc.CreateAttribute ("android", "name", EngagementPostBuild.androidNS)).Value = "android.intent.category.DEFAULT";

		if (mimeType != null) {
			engagementTextAnnouncementIntent.AppendChild (doc.CreateNode (XmlNodeType.Element, "data", null))
				.Attributes.Append (doc.CreateAttribute ("android", "mimeType", EngagementPostBuild.androidNS)).Value = mimeType;	
		}
		engagementTextAnnouncementActivity.AppendChild (engagementTextAnnouncementIntent);
		node.AppendChild (engagementTextAnnouncementActivity);
	}

	public static void addReceiver(XmlDocument doc, XmlNode node, XmlNamespaceManager namespaceManager,string receiverName, string[] actions)
	{
		
		XmlNode receiver = doc.CreateNode (XmlNodeType.Element, "receiver", null);
		receiver.Attributes.Append (doc.CreateAttribute ("tag")).Value = tagName;
		receiver.Attributes.Append (doc.CreateAttribute ("android", "exported", EngagementPostBuild.androidNS)).Value = "false";
		receiver.Attributes.Append (doc.CreateAttribute ("android", "name", EngagementPostBuild.androidNS)).Value = "com.microsoft.azure.engagement." + receiverName;
		XmlNode receiverIntent = doc.CreateNode (XmlNodeType.Element, "intent-filter", null);
		foreach (string action in actions) {
			XmlNode actionNode = doc.CreateNode (XmlNodeType.Element, "action", null);
			actionNode.Attributes.Append (doc.CreateAttribute ("android", "name", EngagementPostBuild.androidNS)).Value = action;
			receiverIntent.AppendChild (actionNode);
		}

		receiver.AppendChild (receiverIntent);
		node.AppendChild (receiver);
	}


	public static int generateAndroidChecksum()
	{
		string chk 	= EngagementConfiguration.ANDROID_CONNECTION_STRING
		            + EngagementConfiguration.ANDROID_GOOGLE_PROJECT_NUMBER
		            + EngagementConfiguration.ANDROID_REACH_ICON
		            + EngagementConfiguration.ACTION_URL_SCHEME
		            + EngagementConfiguration.ANDROID_UNITY3D_ACTIVITY
		            + EngagementConfiguration.ENABLE_NATIVE_LOG
		            + EngagementConfiguration.ENABLE_PLUGIN_LOG
		            + EngagementConfiguration.ENABLE_REACH
		            + EngagementConfiguration.LOCATION_REPORTING_MODE
		            + EngagementConfiguration.LOCATION_REPORTING_TYPE
					+ EngagementAgent.PLUGIN_VERSION;

		return Animator.StringToHash (chk);

	}

	[PostProcessBuildAttribute(1)]
	public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
			
		if (target == BuildTarget.iOS) {

#if ENABLE_IOS_SUPPORT
			string bundleId = PlayerSettings.bundleIdentifier ;

	// 0 - Configuration Check

			string connectionString = EngagementConfiguration.IOS_CONNECTION_STRING;
			if (string.IsNullOrEmpty(connectionString))
				throw new ArgumentException("IOS_CONNECTION_STRING cannot be null when building on iOS project");
				
			string projectFile = PBXProject.GetPBXProjectPath(pathToBuiltProject);
	
	// 1 - Update Project
			PBXProject pbx = new PBXProject();
			pbx.ReadFromFile(projectFile);
	
			string targetUID = pbx.TargetGuidByName(PBXProject.GetUnityTargetName() /*"Unity-iPhone"*/);

			string CTFramework = "CoreTelephony.framework";
			Debug.Log("Adding "+ CTFramework + " to XCode Project");
			pbx.AddFrameworkToProject(targetUID,CTFramework,true);
			 
		
			const string disableAll = "ENGAGEMENT_UNITY=1,ENGAGEMENT_DISABLE_IDFA=1";
			const string enableIDFA = "ENGAGEMENT_UNITY=1,ENGAGEMENT_DISABLE_IDFA=1";
			const string disableIDFA = "ENGAGEMENT_UNITY=1";

			if (EngagementConfiguration.IOS_DISABLE_IDFA == true)
				pbx.UpdateBuildProperty(targetUID,"GCC_PREPROCESSOR_DEFINITIONS",enableIDFA.Split(','),disableAll.Split(','));
			else
				pbx.UpdateBuildProperty(targetUID,"GCC_PREPROCESSOR_DEFINITIONS",disableIDFA.Split(','),disableAll.Split(','));
			

			string[] paths = new string[] { 
				EngagementConfiguration.IOS_REACH_ICON,
				"EngagementPlugin/iOS/res/close.png"
			};

			// 3 - Add files to project

			foreach(string path in paths)
			{
				if (string.IsNullOrEmpty(path))
					continue ;
				string fullpath = Application.dataPath+"/"+path;
				string file = Path.GetFileName (fullpath);
				string fileUID = pbx.AddFile(file,file);
				Debug.Log("Adding  "+ file + " to XCode Project");

				pbx.AddFileToBuild(targetUID,fileUID);

				string xcodePath = pathToBuiltProject+"/"+file ;

				if (File.Exists(xcodePath) == false)
				{
					Debug.Log("Copy from "+ fullpath + " to "+xcodePath);
					File.Copy (fullpath, xcodePath);
				}
			}
	
			pbx.WriteToFile(projectFile);

	// 4 - Modify .PLIST

			string plistPath = pathToBuiltProject + "/Info.plist";
			PlistDocument plist = new PlistDocument();
			plist.ReadFromString(File.ReadAllText(plistPath));
			
			// Get root
			PlistElementDict rootDict = plist.root;


			if (EngagementConfiguration.ENABLE_REACH == true) {
				PlistElementArray UIBackgroundModes = rootDict.CreateArray ("UIBackgroundModes");
				UIBackgroundModes.AddString ("remote-notification");

				PlistElementArray  CFBundleURLTypes = rootDict.CreateArray("CFBundleURLTypes");
				PlistElementDict dict = CFBundleURLTypes.AddDict();
				dict.SetString("CFBundleTypeRole","None");
				dict.SetString("CFBundleURLName",bundleId+".redirect");
				PlistElementArray schemes = dict.CreateArray("CFBundleURLSchemes");
				schemes.AddString(EngagementConfiguration.ACTION_URL_SCHEME);
			}

			// Required on iOS8
			string reportingDesc = EngagementConfiguration.LOCATION_REPORTING_DESCRIPTION;
			if (reportingDesc == null)
				reportingDesc = PlayerSettings.productName + " reports your location for analytics purposes";
			
			if (EngagementConfiguration.LOCATION_REPORTING_MODE == LocationReportingMode.BACKGROUND) 
			{
				rootDict.SetString("NSLocationAlwaysUsageDescription",reportingDesc);
			}
			else
			if (EngagementConfiguration.LOCATION_REPORTING_MODE == LocationReportingMode.FOREGROUND) 
			{
				rootDict.SetString("NSLocationWhenInUseUsageDescription",reportingDesc);
			}



			string icon =  EngagementConfiguration.IOS_REACH_ICON;
			PlistElementDict engagementDict = rootDict.CreateDict("Engagement");
			engagementDict.SetString("IOS_CONNECTION_STRING",EngagementConfiguration.IOS_CONNECTION_STRING);
			engagementDict.SetString("IOS_REACH_ICON",icon);
			engagementDict.SetBoolean("ENABLE_NATIVE_LOG",EngagementConfiguration.ENABLE_NATIVE_LOG);
			engagementDict.SetBoolean("ENABLE_PLUGIN_LOG",EngagementConfiguration.ENABLE_PLUGIN_LOG);
			engagementDict.SetBoolean("ENABLE_REACH",EngagementConfiguration.ENABLE_REACH);
			engagementDict.SetInteger("LOCATION_REPORTING_MODE",Convert.ToInt32(EngagementConfiguration.LOCATION_REPORTING_MODE));
			engagementDict.SetInteger("LOCATION_REPORTING_TYPE",Convert.ToInt32(EngagementConfiguration.LOCATION_REPORTING_TYPE));

			// Write to file
			File.WriteAllText(plistPath, plist.WriteToString());
#else
            Debug.LogError("You need to active ENABLE_IOS_SUPPORT to build for IOS");
#endif

        }

        if (target == BuildTarget.Android)
		{
			string bundleId = PlayerSettings.bundleIdentifier ;
			string productName = PlayerSettings.productName ;

			int chk = generateAndroidChecksum ();
		
			// 0 - Configuration check

			string connectionString = EngagementConfiguration.ANDROID_CONNECTION_STRING;
			if (string.IsNullOrEmpty(connectionString))
				throw new ArgumentException("ANDROID_CONNECTION_STRING cannot be null when building on Android project");

			if (EngagementConfiguration.ENABLE_REACH == true )
			{
				string projectNumber = EngagementConfiguration.ANDROID_GOOGLE_PROJECT_NUMBER;
				if (string.IsNullOrEmpty(projectNumber))
					throw new ArgumentException("ANDROID_GOOGLE_PROJECT_NUMBER cannot be null when Reach is enabled");
			}

			string manifestPath = pathToBuiltProject+"/"+productName+"/AndroidManifest.xml";
			string androidPath = Application.dataPath+"/Plugins/Android";
			string mfFilepath = androidPath+"/AndroidManifest.xml";

			XmlNode root ;
			XmlNodeList nodes ;
			XmlDocument xmlDoc ;
			XmlTextReader reader;
			XmlNamespaceManager namespaceManager = new XmlNamespaceManager(new NameTable());
			namespaceManager.AddNamespace("android", EngagementPostBuild.androidNS);

			// Test Export vs Build
			if (File.Exists(manifestPath) == false)
			{
				// Check that Manifest exists
				if (File.Exists(mfFilepath) == false)
				{
					Debug.LogError ("Missing AndroidManifest.xml in Plugins/Android : execute 'File/Engagement/Generate Android Manifest'");
					return ;
				}

				// Check that it contains Engagement tags
				xmlDoc = new XmlDocument ();
				xmlDoc.XmlResolver = null;

				// Create the reader.
				reader = new XmlTextReader (mfFilepath);
				xmlDoc.Load(reader);
				reader.Close ();

				root = xmlDoc.DocumentElement;	
				nodes = root.SelectNodes("//*[@tag='"+tagName+"']",namespaceManager);
				if (nodes.Count == 0)
					Debug.LogError ("Android manifest in Plugins/Android does not contain Engagement extensions : execute 'File/Engagement/Generate Android Manifest'" );

				// Checking the version

				XmlNode versionNode = root.SelectSingleNode("/manifest/application/meta-data[@android:name='engagement:unity:version']",namespaceManager);
				if (versionNode != null) {
					string ver = versionNode.Attributes["android:value"].Value;
					if (ver != EngagementAgent.PLUGIN_VERSION.ToString())
						versionNode = null;
				}

				if (versionNode == null) 
					Debug.LogError ("EngagementPlugin has been updated : you need to execute 'File/Engagement/Generate Android Manifest' to update your application Manifest first" );
				

				// Checking the checksum

				XmlNode chkNode = root.SelectSingleNode("/manifest/application/meta-data[@android:name='engagement:unity:checksum']",namespaceManager);
				if (chkNode != null) {
					string mfchk = chkNode.Attributes["android:value"].Value;
					if (mfchk != chk.ToString())
						chkNode = null;
				}

				if (chkNode == null) 
					Debug.LogError ("Configuration file has changed : you need to execute 'File/Engagement/Generate Android Manifest' to update your application Manifest" );
				
				// Manifest already processed : nothing to do
				return ;
			}

			Directory.CreateDirectory (androidPath);

			xmlDoc = new XmlDocument ();
			xmlDoc.XmlResolver = null;
			reader = new XmlTextReader (manifestPath);
			xmlDoc.Load(reader);
			reader.Close ();

			root = xmlDoc.DocumentElement;	
		
			// Delete all the former tags
			nodes = root.SelectNodes("//*[@tag='"+tagName+"']",namespaceManager);
			foreach(XmlNode node in nodes)
				node.ParentNode.RemoveChild(node);

			XmlNode manifestNode = root.SelectSingleNode ("/manifest",namespaceManager);
			XmlNode applicationNode = root.SelectSingleNode ("/manifest/application",namespaceManager);
			XmlNode activityNode = root.SelectSingleNode ("/manifest/application/activity[@android:label='@string/app_name']",namespaceManager);
		
			string activity = EngagementConfiguration.ANDROID_UNITY3D_ACTIVITY;
			if (activity == null || activity == "")
				activity = "com.unity3d.player.UnityPlayerActivity";

		// Already in the Unity Default Manifest
		//	EngagementPostBuild.addUsesPermission(xmlDoc,manifestNode,namespaceManager,"android.permission.INTERNET");
			EngagementPostBuild.addUsesPermission(xmlDoc,manifestNode,namespaceManager,"android.permission.ACCESS_NETWORK_STATE");

			EngagementPostBuild.addMetaData(xmlDoc,applicationNode,namespaceManager,"engagement:log:test",XmlConvert.ToString(EngagementConfiguration.ENABLE_NATIVE_LOG));
			EngagementPostBuild.addMetaData(xmlDoc,applicationNode,namespaceManager,"engagement:unity:version",EngagementAgent.PLUGIN_VERSION);
			EngagementPostBuild.addMetaData(xmlDoc,applicationNode,namespaceManager,"engagement:unity:checksum",chk.ToString());

			XmlNode service = xmlDoc.CreateNode(XmlNodeType.Element, "service", null);
			service.Attributes.Append (xmlDoc.CreateAttribute ("tag")).Value = tagName;
			service.Attributes.Append (xmlDoc.CreateAttribute ("android","exported",EngagementPostBuild.androidNS)).Value = "false";
			service.Attributes.Append (xmlDoc.CreateAttribute ("android", "label", EngagementPostBuild.androidNS)).Value = productName + "Service";
			service.Attributes.Append (xmlDoc.CreateAttribute ("android", "name", EngagementPostBuild.androidNS)).Value ="com.microsoft.azure.engagement.service.EngagementService";
			service.Attributes.Append (xmlDoc.CreateAttribute ("android", "process", EngagementPostBuild.androidNS)).Value =":Engagement";
			applicationNode.AppendChild (service);

			string targetAARPath = Application.dataPath+"/Plugins/Android/engagement_notification_icon.aar";
			File.Delete(targetAARPath);

			if (EngagementConfiguration.ENABLE_REACH == true )
			{

				EngagementPostBuild.addActivity(xmlDoc,applicationNode,namespaceManager,"EngagementWebAnnouncementActivity","ANNOUNCEMENT","Light","text/html");
				EngagementPostBuild.addActivity(xmlDoc,applicationNode,namespaceManager,"EngagementTextAnnouncementActivity","ANNOUNCEMENT","Light","text/plain");
				EngagementPostBuild.addActivity(xmlDoc,applicationNode,namespaceManager,"EngagementPollActivity","POLL","Light",null);
				EngagementPostBuild.addActivity(xmlDoc,applicationNode,namespaceManager,"EngagementLoadingActivity","LOADING","Dialog",null);

				const string reachActions = "android.intent.action.BOOT_COMPLETED," +
									  		"com.microsoft.azure.engagement.intent.action.AGENT_CREATED," +
											"com.microsoft.azure.engagement.intent.action.MESSAGE," +
											"com.microsoft.azure.engagement.reach.intent.action.ACTION_NOTIFICATION," +
											"com.microsoft.azure.engagement.reach.intent.action.EXIT_NOTIFICATION," +
											"com.microsoft.azure.engagement.reach.intent.action.DOWNLOAD_TIMEOUT";

				EngagementPostBuild.addReceiver(xmlDoc,applicationNode,namespaceManager,"reach.EngagementReachReceiver",reachActions.Split(','));

				const string downloadActions = "android.intent.action.DOWNLOAD_COMPLETE" ;
				EngagementPostBuild.addReceiver(xmlDoc,applicationNode,namespaceManager,"reach.EngagementReachDownloadReceiver",downloadActions.Split(','));

				// Add GCM Support
				if (string.IsNullOrEmpty(EngagementConfiguration.ANDROID_GOOGLE_PROJECT_NUMBER)==false)
				{

					EngagementPostBuild.addMetaData(xmlDoc,applicationNode,namespaceManager,"engagement:gcm:sender",EngagementConfiguration.ANDROID_GOOGLE_PROJECT_NUMBER+"\\n");

					const string gcmActions = "com.microsoft.azure.engagement.intent.action.APPID_GOT" ;
					EngagementPostBuild.addReceiver(xmlDoc,applicationNode,namespaceManager,"gcm.EngagementGCMEnabler",gcmActions.Split(','));

					XmlNode receiver = xmlDoc.CreateNode(XmlNodeType.Element, "receiver", null);
					receiver.Attributes.Append (xmlDoc.CreateAttribute ("tag")).Value = tagName;
					receiver.Attributes.Append (xmlDoc.CreateAttribute ("android","name",EngagementPostBuild.androidNS)).Value = "com.microsoft.azure.engagement.gcm.EngagementGCMReceiver";
					receiver.Attributes.Append (xmlDoc.CreateAttribute ("android", "permission", EngagementPostBuild.androidNS)).Value = "com.google.android.c2dm.permission.SEND";
					
					XmlNode intentReceiver = xmlDoc.CreateNode(XmlNodeType.Element, "intent-filter", null);
					receiver.AppendChild(intentReceiver);
					intentReceiver.AppendChild ( xmlDoc.CreateNode (XmlNodeType.Element, "action", null) )
						.Attributes.Append (xmlDoc.CreateAttribute ("android","name",EngagementPostBuild.androidNS)).Value = "com.google.android.c2dm.intent.REGISTRATION";
					intentReceiver.AppendChild ( xmlDoc.CreateNode (XmlNodeType.Element, "action", null) )
						.Attributes.Append (xmlDoc.CreateAttribute ("android","name",EngagementPostBuild.androidNS)).Value = "com.google.android.c2dm.intent.RECEIVE";
					intentReceiver.AppendChild ( xmlDoc.CreateNode (XmlNodeType.Element, "category", null) )
						.Attributes.Append (xmlDoc.CreateAttribute ("android","name",EngagementPostBuild.androidNS)).Value = bundleId;
					
					applicationNode.AppendChild (receiver);

					string permissionPackage = bundleId + ".permission.C2D_MESSAGE";
					EngagementPostBuild.addUsesPermission(xmlDoc,manifestNode,namespaceManager,permissionPackage);

					XmlNode permission = xmlDoc.CreateNode(XmlNodeType.Element, "permission", null);
					permission.Attributes.Append (xmlDoc.CreateAttribute ("tag")).Value = tagName;
					permission.Attributes.Append (xmlDoc.CreateAttribute ("android","name",EngagementPostBuild.androidNS)).Value = permissionPackage;
					permission.Attributes.Append (xmlDoc.CreateAttribute ("android", "protectionLevel", EngagementPostBuild.androidNS)).Value = "signature";
					manifestNode.AppendChild (permission);

				}

				const string datapushActions = "com.microsoft.azure.engagement.reach.intent.action.DATA_PUSH" ;
				EngagementPostBuild.addReceiver(xmlDoc,applicationNode,namespaceManager,"shared.EngagementDataPushReceiver",datapushActions.Split(','));

				EngagementPostBuild.addUsesPermission(xmlDoc,manifestNode,namespaceManager,"android.permission.WRITE_EXTERNAL_STORAGE");
				EngagementPostBuild.addUsesPermission(xmlDoc,manifestNode,namespaceManager,"android.permission.DOWNLOAD_WITHOUT_NOTIFICATION");
				EngagementPostBuild.addUsesPermission(xmlDoc,manifestNode,namespaceManager,"android.permission.VIBRATE");
				EngagementPostBuild.addUsesPermission(xmlDoc,manifestNode,namespaceManager,"com.google.android.c2dm.permission.RECEIVE");

				string icon = "app_icon"; // using Application icon as default
			
				if (string.IsNullOrEmpty(EngagementConfiguration.ANDROID_REACH_ICON ) == false)
				{
					// The only way to add resources to the APK is through a AAR library : let just create one with the icon
					icon = "engagement_notification_icon"; 
					string srcAARPath = Application.dataPath+"/EngagementPlugin/Editor/engagement_notification_icon.zip";
					string iconPath = Application.dataPath+"/"+EngagementConfiguration.ANDROID_REACH_ICON;
					File.Copy (srcAARPath,targetAARPath, true );
					ZipFile zip = ZipFile.Read(targetAARPath);
					zip.AddFile (iconPath).FileName = "res/drawable/"+icon+".png";
					zip.Save ();
				}
			
				EngagementPostBuild.addMetaData(xmlDoc,applicationNode,namespaceManager,"engagement:reach:notification:icon",icon);

				XmlNode catcherActivity = xmlDoc.CreateNode(XmlNodeType.Element, "activity", null);
				catcherActivity.Attributes.Append (xmlDoc.CreateAttribute ("tag")).Value = "Engagement";
				catcherActivity.Attributes.Append (xmlDoc.CreateAttribute ("android","label",EngagementPostBuild.androidNS)).Value = "@string/app_name";
				catcherActivity.Attributes.Append (xmlDoc.CreateAttribute ("android","name",EngagementPostBuild.androidNS)).Value = "com.microsoft.azure.engagement.unity.IntentCatcherActivity";
		
				XmlNode intent = xmlDoc.CreateNode(XmlNodeType.Element, "intent-filter", null);
				intent.Attributes.Append (xmlDoc.CreateAttribute ("tag")).Value = tagName;
				
				intent.AppendChild ( xmlDoc.CreateNode (XmlNodeType.Element, "action", null) )
					.Attributes.Append (xmlDoc.CreateAttribute ("android","name",EngagementPostBuild.androidNS)).Value = "android.intent.action.VIEW";
				
				intent.AppendChild ( xmlDoc.CreateNode (XmlNodeType.Element, "category", null) )
					.Attributes.Append (xmlDoc.CreateAttribute ("android","name",EngagementPostBuild.androidNS)).Value = "android.intent.category.DEFAULT";
				
				intent.AppendChild ( xmlDoc.CreateNode (XmlNodeType.Element, "category", null) )
					.Attributes.Append (xmlDoc.CreateAttribute ("android","name",EngagementPostBuild.androidNS)).Value = "android.intent.category.BROWSABLE";
				
				intent.AppendChild ( xmlDoc.CreateNode (XmlNodeType.Element, "data", null) )
					.Attributes.Append (xmlDoc.CreateAttribute ("android","scheme",EngagementPostBuild.androidNS)).Value = EngagementConfiguration.ACTION_URL_SCHEME;
				
				catcherActivity.AppendChild(intent);
				applicationNode.AppendChild (catcherActivity);

				EngagementPostBuild.addMetaData(xmlDoc,applicationNode,namespaceManager,"engagement:unity:activityname",activity);
			}

			if (EngagementConfiguration.LOCATION_REPORTING_MODE == LocationReportingMode.BACKGROUND)
			{
				string bootReceiverActions = "android.intent.action.BOOT_COMPLETED";
				EngagementPostBuild.addReceiver(xmlDoc, applicationNode, namespaceManager,"EngagementLocationBootReceiver", bootReceiverActions.Split(','));
			}
						
			if (EngagementConfiguration.LOCATION_REPORTING_TYPE == LocationReportingType.LAZY ||  EngagementConfiguration.LOCATION_REPORTING_TYPE == LocationReportingType.REALTIME)
				EngagementPostBuild.addUsesPermission(xmlDoc,manifestNode,namespaceManager,"android.permission.ACCESS_COARSE_LOCATION");
			else
			if (EngagementConfiguration.LOCATION_REPORTING_TYPE == LocationReportingType.FINEREALTIME )
				EngagementPostBuild.addUsesPermission(xmlDoc,manifestNode,namespaceManager,"android.permission.ACCESS_FINE_LOCATION");

			if (EngagementConfiguration.LOCATION_REPORTING_MODE == LocationReportingMode.BACKGROUND || EngagementConfiguration.ENABLE_REACH == true)
				EngagementPostBuild.addUsesPermission(xmlDoc,manifestNode,namespaceManager,"android.permission.RECEIVE_BOOT_COMPLETED");

			// Update the manifest
			TextWriter textWriter = new StreamWriter (manifestPath ); 
			xmlDoc.Save (textWriter);
			textWriter.Close ();

			// revert the bundle id
			activityNode.Attributes.Append (xmlDoc.CreateAttribute ("android","name",EngagementPostBuild.androidNS)).Value = activity; 

			Debug.Log ("Generating :"+mfFilepath);
			TextWriter mfWriter = new StreamWriter (mfFilepath ); 
			xmlDoc.Save (mfWriter);
			mfWriter.Close ();
		
		}

	}
}

#pragma warning restore 162,429