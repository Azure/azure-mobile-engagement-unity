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

public class EngagementPostBuild
{

  const string androidNS = "http://schemas.android.com/apk/res/android";
  const string xNameSpaceURI = "http://schemas.microsoft.com/winfx/2006/xaml";
  const string uapNameSpaceURI = "http://schemas.microsoft.com/appx/manifest/uap/windows10";
  const string m3NameSpaceURI = "http://schemas.microsoft.com/appx/2014/manifest";
  const string tagName = "Engagement";

  const string WP81_WINDOWSPHONE = "WindowsPhone";
  const string WP81_WINDOWS = "Windows";
  const string WP81_SHARED = "Shared";

  static string[] GetScenePaths ()
  {
    List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene> (EditorBuildSettings.scenes);
    List<string> enabledScenes = new List<string> ();
    foreach (EditorBuildSettingsScene scene in scenes) {
      if (scene.enabled)
        enabledScenes.Add (scene.path);
    }
    return enabledScenes.ToArray ();
  }

  [MenuItem ("File/Engagement/Generate Android Manifest")]
  static void GenerateManifest ()
  {

    #if UNITY_5_6_OR_NEWER
    EditorUserBuildSettings.SwitchActiveBuildTarget (BuildTargetGroup.Android, BuildTarget.Android);
    #else
			EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.Android);
    #endif
		
    string path = Application.temporaryCachePath + "/EngagementManifest";
    if (Directory.Exists (path))
      Directory.Delete (path, true);

    BuildPipeline.BuildPlayer (GetScenePaths (), path, BuildTarget.Android, BuildOptions.AcceptExternalModificationsToPlayer);
  }

  public static void addMetaData (XmlDocument doc, XmlNode node, XmlNamespaceManager namespaceManager, string name, string value)
  {
    XmlNode metaData = doc.CreateNode (XmlNodeType.Element, "meta-data", null);
    metaData.Attributes.Append (doc.CreateAttribute ("tag")).Value = tagName;
    metaData.Attributes.Append (doc.CreateAttribute ("android", "name", EngagementPostBuild.androidNS)).Value = name;
    metaData.Attributes.Append (doc.CreateAttribute ("android", "value", EngagementPostBuild.androidNS)).Value = value;
    node.AppendChild (metaData);
  }

  public static void addUsesPermission (XmlDocument doc, XmlNode node, XmlNamespaceManager namespaceManager, string permissionName)
  {
    XmlNode usesPermission = doc.CreateNode (XmlNodeType.Element, "uses-permission", null);
    usesPermission.Attributes.Append (doc.CreateAttribute ("tag")).Value = tagName;
    usesPermission.Attributes.Append (doc.CreateAttribute ("android", "name", EngagementPostBuild.androidNS)).Value = permissionName;
    node.AppendChild (usesPermission);
  }

  public static void addActivity (XmlDocument doc, XmlNode node, XmlNamespaceManager namespaceManager, string activityName, string actionName, string theme, string mimeType)
  {

    XmlNode engagementTextAnnouncementActivity = doc.CreateNode (XmlNodeType.Element, "activity", null);
    engagementTextAnnouncementActivity.Attributes.Append (doc.CreateAttribute ("tag")).Value = tagName;
    engagementTextAnnouncementActivity.Attributes.Append (doc.CreateAttribute ("android", "exported", EngagementPostBuild.androidNS)).Value = "false";
    engagementTextAnnouncementActivity.Attributes.Append (doc.CreateAttribute ("android", "theme", EngagementPostBuild.androidNS)).Value = "@android:style/Theme." + theme;
    engagementTextAnnouncementActivity.Attributes.Append (doc.CreateAttribute ("android", "name", EngagementPostBuild.androidNS)).Value = "com.microsoft.azure.engagement.reach.activity." + activityName;
    engagementTextAnnouncementActivity.Attributes.Append (doc.CreateAttribute ("android", "exported", EngagementPostBuild.androidNS)).Value = "false";

    XmlNode engagementTextAnnouncementIntent = doc.CreateNode (XmlNodeType.Element, "intent-filter", null);
    engagementTextAnnouncementIntent.Attributes.Append (doc.CreateAttribute ("tag")).Value = tagName;
    engagementTextAnnouncementIntent.AppendChild (doc.CreateNode (XmlNodeType.Element, "action", null))
            .Attributes.Append (doc.CreateAttribute ("android", "name", EngagementPostBuild.androidNS)).Value = "com.microsoft.azure.engagement.reach.intent.action." + actionName;
    engagementTextAnnouncementIntent.AppendChild (doc.CreateNode (XmlNodeType.Element, "category", null))
            .Attributes.Append (doc.CreateAttribute ("android", "name", EngagementPostBuild.androidNS)).Value = "android.intent.category.DEFAULT";

    if (mimeType != null) {
      engagementTextAnnouncementIntent.AppendChild (doc.CreateNode (XmlNodeType.Element, "data", null))
                .Attributes.Append (doc.CreateAttribute ("android", "mimeType", EngagementPostBuild.androidNS)).Value = mimeType;
    }
    engagementTextAnnouncementActivity.AppendChild (engagementTextAnnouncementIntent);
    node.AppendChild (engagementTextAnnouncementActivity);
  }

  public static void addReceiver (XmlDocument doc, XmlNode node, XmlNamespaceManager namespaceManager, string receiverName, string[] actions)
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

  public static int generateAndroidChecksum ()
  {
    string chk = EngagementConfiguration.ANDROID_CONNECTION_STRING
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

  public static string removeEngagementCode (string _cs)
  {
    while (true) {
      Regex rgx_remove = new Regex ("// BEGIN_ENGAGEMENT(.*?)END_ENGAGEMENT", RegexOptions.IgnoreCase | RegexOptions.Singleline);
      Match match_remove = rgx_remove.Match (_cs);
      if (match_remove.Success)
        _cs = rgx_remove.Replace (_cs, "");
      else
        break;
    }
    return _cs;
  }

  public static void appendChildWithEngagementComment (XmlDocument _doc, XmlNode _target, XmlNode _node)
  {
    XmlNode beginNode = _doc.CreateNode (XmlNodeType.Comment, "", "");

    string key = "";
    XmlNode cur = _target;
    while (cur != _doc.DocumentElement) {
      key = cur.Name + "/" + key;
      cur = cur.ParentNode;
    }
    key = key + _node.Name;
    string attrString = null;
    foreach (XmlAttribute attrs in _node.Attributes) {
      // Name is always a key for the elements added (so far!)
      if (attrs.Name.ToLower ().Contains ("name") == false)
        continue;
      if (attrString != null)
        attrString += " and ";
      else
        attrString = "";
      // Using wildcard to avoid having to deal with namespace!
      attrString += "@*"; 
      attrString += "='" + attrs.Value + "'";
    }
    if (attrString != null) {
      key += "[";
      key += attrString;
      key += "]";
    }

    beginNode.Value = tagName + " " + key;
    _target.AppendChild (beginNode);
    _target.AppendChild (_node);
    
    // Check XSPath
        
    XmlNode test = _doc.DocumentElement.SelectSingleNode (key);
    if (test == null)
      Debug.LogError ("Could not retrieve " + key);   
  }

  public static void removeEngagementComments (XmlNode _root)
  {
    XmlNodeList comments = _root.SelectNodes ("//comment()");
    List<XmlNode> toRemove = new List<XmlNode> ();
    foreach (XmlNode l in comments) {
      if (l.Value.StartsWith (tagName) == true) {
        // Remove Comment
        toRemove.Add (l);
        string key = l.Value.Substring (tagName.Length + 1);
        XmlNode node = _root.SelectSingleNode (key);
        if (node == null)
          Debug.LogError ("Could not find " + key);
        else
                    // Remove key
                    toRemove.Add (node);       
      }
    }

    foreach (XmlNode n in toRemove)
      n.ParentNode.RemoveChild (n);  
  }

  public static void updateAppXManifest (string PackageManifest, XmlNamespaceManager _namespaceManager, Boolean _isWP81)
  {
    if (File.Exists (PackageManifest) == false) {
      Debug.LogError (PackageManifest);
      return;
    }

    Debug.Log ("Patching " + PackageManifest);
    XmlDocument xmlDoc = new XmlDocument ();
    xmlDoc.XmlResolver = null;

        
    XmlTextReader reader = new XmlTextReader (PackageManifest);
    reader.Namespaces = false;
    xmlDoc.Load (reader);
    reader.Close ();
    XmlElement root = xmlDoc.DocumentElement;

    // 1-0 - Removing all begin/engagement

    removeEngagementComments (root);

    // 1-1 - Add extensions node if not present

    XmlNodeList applications = root.SelectNodes ("Applications/Application", null);
    if (applications == null)
      Debug.LogError ("cannot find Applications/Application");
    XmlNode application = applications [0];
     
  
    if (_isWP81 == true) {
      XmlNode visualElementsNode = root.SelectSingleNode ("//*[local-name()='m3:VisualElements']");
      if (visualElementsNode == null)
        Debug.LogError ("Cannot find VisualElements");
      else
        visualElementsNode.Attributes.Append (xmlDoc.CreateAttribute ("ToastCapable")).Value = XmlConvert.ToString (EngagementConfiguration.ENABLE_REACH);     
    }

    // 1-2 - Add Protocol if need be

    if (EngagementConfiguration.ENABLE_REACH == true && EngagementConfiguration.ACTION_URL_SCHEME != null) {
      XmlNode extensionNode;
      XmlNode protocolNode;

      if (_isWP81 == true) {
        // No uap
        extensionNode = xmlDoc.CreateNode (XmlNodeType.Element, "Extension", null);
        protocolNode = xmlDoc.CreateNode (XmlNodeType.Element, "Protocol", null);
      } else {
        extensionNode = xmlDoc.CreateNode (XmlNodeType.Element, "uap", "Extension", uapNameSpaceURI);
        protocolNode = xmlDoc.CreateNode (XmlNodeType.Element, "uap", "Protocol", uapNameSpaceURI);
      }

      extensionNode.Attributes.Append (xmlDoc.CreateAttribute ("Category")).Value = "windows.protocol";
      protocolNode.Attributes.Append (xmlDoc.CreateAttribute ("Name")).Value = EngagementConfiguration.ACTION_URL_SCHEME;
      extensionNode.AppendChild (protocolNode);

      XmlNode extensionsNode = application.SelectSingleNode ("Extensions", null);
      if (extensionsNode == null) {
        extensionsNode = xmlDoc.CreateNode (XmlNodeType.Element, "Extensions", root.NamespaceURI);
        extensionsNode.AppendChild (extensionNode);
        appendChildWithEngagementComment (xmlDoc, application, extensionsNode);
      } else
        appendChildWithEngagementComment (xmlDoc, extensionsNode, extensionNode);
    }

    // 1-3 - Add Capabilities node if not present

    XmlNode capabilitiesNode = root.SelectSingleNode ("Capabilities", null);
    if (capabilitiesNode == null) {
      capabilitiesNode = xmlDoc.CreateNode (XmlNodeType.Element, "Capabilities", root.NamespaceURI);
      root.AppendChild (capabilitiesNode);
    }

    // 1-4 - Add internetClient node if not present

    XmlNode capability = capabilitiesNode.SelectSingleNode ("Capability[@Name='internetClient']");
    if (capability == null) {
      capability = xmlDoc.CreateNode (XmlNodeType.Element, "Capability", root.NamespaceURI);
      capability.Attributes.Append (xmlDoc.CreateAttribute ("Name")).Value = "internetClient";
      appendChildWithEngagementComment (xmlDoc, capabilitiesNode, capability);
    }

    // 1-4 - Writing Manifest

    StreamWriter mfWriter = new StreamWriter (PackageManifest);
    xmlDoc.Save (mfWriter);
    mfWriter.Close ();
  }


  public static void addResourcesToProject (string unzipResourcesDirectory, string projectName)
  {
    // 3 Add ressources to the Project
    const string resourceDirectory = "Resources";

    // 3-1 Unzip ressources to the Project

    string resourcesFilename = Application.dataPath + "/EngagementPlugin/WSA/" + resourceDirectory + ".zip";
    string unzipResources = unzipResourcesDirectory + "/" + resourceDirectory;
    if (Directory.Exists (unzipResources))
      Directory.Delete (unzipResources, true);

    // 3-2 Load Project .csproj
    Debug.Log ("Adding resources in  " + unzipResourcesDirectory);
    ZipFile ressourcesZip = new ZipFile (resourcesFilename);
    ressourcesZip.ExtractAll (unzipResourcesDirectory);

    string vsProject = unzipResourcesDirectory + "/" + projectName;

    Debug.Log ("Patching " + vsProject);
    XmlTextReader reader = new XmlTextReader (vsProject);
    reader.Namespaces = false;
    XmlDocument xmlDoc = new XmlDocument ();
    xmlDoc.Load (reader);
    reader.Close ();
    XmlElement root = xmlDoc.DocumentElement;

    // 3-3 Remove Resources reference from project

    XmlNode compile = root.SelectSingleNode ("//Compile[@Include='App.xaml.cs']");
    if (compile == null) {
      Debug.LogError ("Cannot parse vsproject");
      return;
    }

    string search = "//Content[contains(@Include,'" + resourceDirectory + "\\')]";
    XmlNodeList resourceNodes = root.SelectNodes (search);
    foreach (XmlNode node in resourceNodes)
      node.ParentNode.RemoveChild (node);

    search = "//Page[contains(@Include,'" + resourceDirectory + "\\')]";
    resourceNodes = root.SelectNodes (search);
    foreach (XmlNode node in resourceNodes)
      node.ParentNode.RemoveChild (node);

    search = "//Compile[contains(@Include,'" + resourceDirectory + "\\')]";
    resourceNodes = root.SelectNodes (search);
    foreach (XmlNode node in resourceNodes)
      node.ParentNode.RemoveChild (node);

    // 3-4 Add Resource references to the project

    if (EngagementConfiguration.ENABLE_REACH) {
      List<string> dirs = new List<string> (Directory.GetDirectories (unzipResources));
      foreach (string dir in dirs) {
        List<string> files = new List<string> (Directory.GetFiles (dir));
        foreach (string fi in files) {
          string filename = fi.Substring (unzipResourcesDirectory.Length + 1);
          int i = filename.IndexOf (".");
          string ext = filename.Substring (i);
          if (ext == ".cs" || ext == ".xaml.cs") {
            XmlNode resourceNode = xmlDoc.CreateNode (XmlNodeType.Element, "Compile", root.NamespaceURI);
            resourceNode.Attributes.Append (xmlDoc.CreateAttribute ("Include")).Value = filename;
            compile.ParentNode.AppendChild (resourceNode);
          } else if (ext == ".xaml") {
            XmlNode resourceNode = xmlDoc.CreateNode (XmlNodeType.Element, "Page", root.NamespaceURI);
            resourceNode.Attributes.Append (xmlDoc.CreateAttribute ("Include")).Value = filename;
            XmlNode generatorNode = xmlDoc.CreateNode (XmlNodeType.Element, "Generator", root.NamespaceURI);
            generatorNode.InnerText = "MSBuild:Compile";
            resourceNode.AppendChild (generatorNode);
            XmlNode subtypeNode = xmlDoc.CreateNode (XmlNodeType.Element, "SubType", root.NamespaceURI);
            subtypeNode.InnerText = "Designer";
            resourceNode.AppendChild (subtypeNode);
            compile.ParentNode.AppendChild (resourceNode);
          } else {
            XmlNode resourceNode = xmlDoc.CreateNode (XmlNodeType.Element, "Content", root.NamespaceURI);
            resourceNode.Attributes.Append (xmlDoc.CreateAttribute ("Include")).Value = filename;
            compile.ParentNode.AppendChild (resourceNode);
          }
        }
      }
    }

    // 3-5 Write project

    StreamWriter mfWriter = new StreamWriter (vsProject);
    xmlDoc.Save (mfWriter);
    mfWriter.Close ();
  }

  public static string getWP10Directory (string _base)
  {
    return _base + "/" + PlayerSettings.productName;
  }


  [PostProcessBuildAttribute (1)]
  public static void OnPostprocessBuild (BuildTarget target, string pathToBuiltProject)
  {
    XmlNode root;
    XmlNodeList nodes;
    XmlDocument xmlDoc;
    XmlTextReader reader;
    TextWriter mfWriter;

    if (target == BuildTarget.WSAPlayer) {
      XmlNamespaceManager namespaceManager = new XmlNamespaceManager (new NameTable ());
      namespaceManager.AddNamespace ("x", xNameSpaceURI);
      namespaceManager.AddNamespace ("uap", uapNameSpaceURI);
      namespaceManager.AddNamespace ("m3", m3NameSpaceURI);
            
      // O - Checking configuration

      if (EngagementConfiguration.WINDOWS_CONNECTION_STRING == null) {
        Debug.LogError ("Missing WINDOWS_CONNECTION_STRING in EngagementConfiguration");
        return;
      }

      if (EditorUserBuildSettings.wsaSDK != WSASDK.UWP && EditorUserBuildSettings.wsaSDK != WSASDK.PhoneSDK81) {
        Debug.LogError ("Only Phone8.1 and Universal10 SDKs are supported");
        return;
      }

      Boolean isWP81 = (EditorUserBuildSettings.wsaSDK == WSASDK.UniversalSDK81 || EditorUserBuildSettings.wsaSDK == WSASDK.PhoneSDK81);
          
      string PackageManifest = getWP10Directory (pathToBuiltProject) + "/Package.appxmanifest";
      updateAppXManifest (PackageManifest, namespaceManager, isWP81);
            
      // 2 - Patching App.Xaml.CS

      string appcspath = getWP10Directory (pathToBuiltProject);
      appcspath += "/App.xaml.cs";
      string appcsString = System.IO.File.ReadAllText (appcspath);

      // 2-1 - Remove former patch

      appcsString = removeEngagementCode (appcsString);
            
      Regex rgx2 = new Regex ("OnLaunched\\(.*\\)\\s*\\{(\\s*)", RegexOptions.IgnoreCase);
      Match match2 = rgx2.Match (appcsString);
      if (!match2.Success)
        Debug.LogError ("could not find OnLaunched in App.xaml.cs");

      // 2-2 Add initEngagement OnLaunched

      string r = "OnLaunched(LaunchActivatedEventArgs args)\n";
      r += "\t\t{\n";
      r += "\t\t\t// BEGIN_ENGAGEMENT\n";
      r += "\t\t\tMicrosoft.Azure.Engagement.Unity.EngagementAgent.initEngagement(args);\n";
      r += "\t\t\t// END_ENGAGEMENT\n";

      appcsString = rgx2.Replace (appcsString, r);

      // 2-3 Add initEngagement OnActivated 

      rgx2 = new Regex ("OnActivated\\(.*\\)\\s*\\{", RegexOptions.IgnoreCase);
      match2 = rgx2.Match (appcsString);
      if (!match2.Success)
        Debug.LogError ("could not find OnActivated in App.xaml.cs");

      r = "OnActivated(IActivatedEventArgs args)\n";
      r += "\t\t{\n";
      r += "\t\t\t// BEGIN_ENGAGEMENT\n";
      r += "\t\t\tMicrosoft.Azure.Engagement.Unity.EngagementAgent.initEngagementOnActivated(args);\n";
      r += "\t\t\t// END_ENGAGEMENT\n";

      appcsString = rgx2.Replace (appcsString, r);
            
      System.IO.File.WriteAllText (appcspath, appcsString);
      Debug.Log ("Patching " + appcspath);

      string unzipResourcesDirectoryWP10 = getWP10Directory (pathToBuiltProject);
      string vsProjectWP10 = PlayerSettings.productName + ".csproj";
      addResourcesToProject (unzipResourcesDirectoryWP10, vsProjectWP10);
            
      // 4 Patching MainPage.Xaml

      string mainpagexamlPath = getWP10Directory (pathToBuiltProject);
      mainpagexamlPath += "/MainPage.xaml";

      Debug.Log ("Patching " + mainpagexamlPath);
      reader = new XmlTextReader (mainpagexamlPath);
      reader.Namespaces = false;
      xmlDoc = new XmlDocument ();
      xmlDoc.Load (reader);
      reader.Close ();

      // 4-0 - Remove webview

      root = xmlDoc.DocumentElement;
      removeEngagementComments (root);

      // 4-1 - Create Grid as a placeholder for the webview

      XmlNode grid = root.SelectSingleNode ("Grid");
      if (grid == null) {
              
        grid = xmlDoc.CreateNode (XmlNodeType.Element, "Grid", root.NamespaceURI);
        grid.Attributes.Append (xmlDoc.CreateAttribute ("x", "Name", xNameSpaceURI)).Value = "engagementGrid";

        foreach (XmlNode c in  root.ChildNodes)
          grid.AppendChild (c);
            
        root.AppendChild (grid);
      } else {
        XmlNode attr = grid.Attributes.GetNamedItem ("x:Name");
        if (attr == null)
          grid.Attributes.Append (xmlDoc.CreateAttribute ("x", "Name", xNameSpaceURI)).Value = "engagementGrid";
                   
      }

      // 4-2 Adding the webview

      if (EngagementConfiguration.ENABLE_REACH == true) {
        XmlNode webviewNode = xmlDoc.CreateNode (XmlNodeType.Element, "WebView", root.NamespaceURI);

        webviewNode.Attributes.Append (xmlDoc.CreateAttribute ("x", "Name", xNameSpaceURI)).Value = "engagement_notification_content";
        webviewNode.Attributes.Append (xmlDoc.CreateAttribute ("Visibility")).Value = "Collapsed";
        webviewNode.Attributes.Append (xmlDoc.CreateAttribute ("Height")).Value = "80";
        webviewNode.Attributes.Append (xmlDoc.CreateAttribute ("HorizontalAlignment")).Value = "Stretch";
        webviewNode.Attributes.Append (xmlDoc.CreateAttribute ("VerticalAlignment")).Value = "Top";

        appendChildWithEngagementComment (xmlDoc, grid, webviewNode);
                
        webviewNode = xmlDoc.CreateNode (XmlNodeType.Element, "WebView", root.NamespaceURI);
        webviewNode.Attributes.Append (xmlDoc.CreateAttribute ("x", "Name", xNameSpaceURI)).Value = "engagement_announcement_content";
        webviewNode.Attributes.Append (xmlDoc.CreateAttribute ("Visibility")).Value = "Collapsed";
        webviewNode.Attributes.Append (xmlDoc.CreateAttribute ("HorizontalAlignment")).Value = "Stretch";
        webviewNode.Attributes.Append (xmlDoc.CreateAttribute ("VerticalAlignment")).Value = "Stretch";

        grid.AppendChild (webviewNode);
        appendChildWithEngagementComment (xmlDoc, grid, webviewNode);

      }
           
      // 4-3 Write page

      mfWriter = new StreamWriter (mainpagexamlPath);
      xmlDoc.Save (mfWriter);
      mfWriter.Close ();

         
    } else if (target == BuildTarget.iOS) {

#if ENABLE_IOS_SUPPORT
      string bundleId = PlayerSettings.applicationIdentifier;

      // 0 - Configuration Check

      string connectionString = EngagementConfiguration.IOS_CONNECTION_STRING;
      if (string.IsNullOrEmpty (connectionString))
        throw new ArgumentException ("IOS_CONNECTION_STRING cannot be null when building on iOS project");
				
      string projectFile = PBXProject.GetPBXProjectPath (pathToBuiltProject);
	
      // 1 - Update Project
      PBXProject pbx = new PBXProject ();
      pbx.ReadFromFile (projectFile);
	
      string targetUID = pbx.TargetGuidByName (PBXProject.GetUnityTargetName () /*"Unity-iPhone"*/);

      string CTFramework = "CoreTelephony.framework";
      Debug.Log ("Adding " + CTFramework + " to XCode Project");
      pbx.AddFrameworkToProject (targetUID, CTFramework, true);

      string UNFramework = "UserNotifications.framework";
      Debug.Log ("Adding " + UNFramework + " to XCode Project");
      pbx.AddFrameworkToProject (targetUID, UNFramework, false);
			 
		
      const string disableAll = "ENGAGEMENT_UNITY=1,ENGAGEMENT_DISABLE_IDFA=1";
      const string enableIDFA = "ENGAGEMENT_UNITY=1,ENGAGEMENT_DISABLE_IDFA=1";
      const string disableIDFA = "ENGAGEMENT_UNITY=1";

      if (EngagementConfiguration.IOS_DISABLE_IDFA == true)
        pbx.UpdateBuildProperty (targetUID, "GCC_PREPROCESSOR_DEFINITIONS", enableIDFA.Split (','), disableAll.Split (','));
      else
        pbx.UpdateBuildProperty (targetUID, "GCC_PREPROCESSOR_DEFINITIONS", disableIDFA.Split (','), disableAll.Split (','));
			

      string[] paths = new string[] { 
        EngagementConfiguration.IOS_REACH_ICON,
        "EngagementPlugin/iOS/res/close.png"
      };

      // 3 - Add files to project

      foreach (string path in paths) {
        if (string.IsNullOrEmpty (path))
          continue;
        string fullpath = Application.dataPath + "/" + path;
        string file = Path.GetFileName (fullpath);
        string fileUID = pbx.AddFile (file, file);
        Debug.Log ("Adding  " + file + " to XCode Project");

        pbx.AddFileToBuild (targetUID, fileUID);

        string xcodePath = pathToBuiltProject + "/" + file;

        if (File.Exists (xcodePath) == false) {
          Debug.Log ("Copy from " + fullpath + " to " + xcodePath);
          File.Copy (fullpath, xcodePath);
        }
      }
	
      pbx.WriteToFile (projectFile);

      // 4 - Modify .PLIST

      string plistPath = pathToBuiltProject + "/Info.plist";
      PlistDocument plist = new PlistDocument ();
      plist.ReadFromString (File.ReadAllText (plistPath));
			
      // Get root
      PlistElementDict rootDict = plist.root;


      if (EngagementConfiguration.ENABLE_REACH == true) {
        PlistElementArray UIBackgroundModes = rootDict.CreateArray ("UIBackgroundModes");
        UIBackgroundModes.AddString ("remote-notification");

        PlistElementArray CFBundleURLTypes = rootDict.CreateArray ("CFBundleURLTypes");
        PlistElementDict dict = CFBundleURLTypes.AddDict ();
        dict.SetString ("CFBundleTypeRole", "None");
        dict.SetString ("CFBundleURLName", bundleId + ".redirect");
        PlistElementArray schemes = dict.CreateArray ("CFBundleURLSchemes");
        schemes.AddString (EngagementConfiguration.ACTION_URL_SCHEME);
      }

      // Required on iOS8
      string reportingDesc = EngagementConfiguration.LOCATION_REPORTING_DESCRIPTION;
      if (reportingDesc == null)
        reportingDesc = PlayerSettings.productName + " reports your location for analytics purposes";
			
      if (EngagementConfiguration.LOCATION_REPORTING_MODE == LocationReportingMode.BACKGROUND) {
        rootDict.SetString ("NSLocationAlwaysUsageDescription", reportingDesc);
				
        // Required on iOS11
        rootDict.SetString ("NSLocationAlwaysAndWhenInUseUsageDescription", reportingDesc);
      } else if (EngagementConfiguration.LOCATION_REPORTING_MODE == LocationReportingMode.FOREGROUND) {
        rootDict.SetString ("NSLocationWhenInUseUsageDescription", reportingDesc);
      }



      string icon = EngagementConfiguration.IOS_REACH_ICON;
      PlistElementDict engagementDict = rootDict.CreateDict ("Engagement");
      engagementDict.SetString ("IOS_CONNECTION_STRING", EngagementConfiguration.IOS_CONNECTION_STRING);
      engagementDict.SetString ("IOS_REACH_ICON", icon);
      engagementDict.SetBoolean ("ENABLE_NATIVE_LOG", EngagementConfiguration.ENABLE_NATIVE_LOG);
      engagementDict.SetBoolean ("ENABLE_PLUGIN_LOG", EngagementConfiguration.ENABLE_PLUGIN_LOG);
      engagementDict.SetBoolean ("ENABLE_REACH", EngagementConfiguration.ENABLE_REACH);
      engagementDict.SetInteger ("LOCATION_REPORTING_MODE", Convert.ToInt32 (EngagementConfiguration.LOCATION_REPORTING_MODE));
      engagementDict.SetInteger ("LOCATION_REPORTING_TYPE", Convert.ToInt32 (EngagementConfiguration.LOCATION_REPORTING_TYPE));

      // Write to file
      File.WriteAllText (plistPath, plist.WriteToString ());
#else
            Debug.LogError("You need to active ENABLE_IOS_SUPPORT to build for IOS");
#endif

    }

    if (target == BuildTarget.Android) {
      string bundleId = PlayerSettings.applicationIdentifier;
      string productName = PlayerSettings.productName;

      int chk = generateAndroidChecksum ();
		
      // 0 - Configuration check

      string connectionString = EngagementConfiguration.ANDROID_CONNECTION_STRING;
      if (string.IsNullOrEmpty (connectionString))
        throw new ArgumentException ("ANDROID_CONNECTION_STRING cannot be null when building on Android project");

      if (EngagementConfiguration.ENABLE_REACH == true) {
        string projectNumber = EngagementConfiguration.ANDROID_GOOGLE_PROJECT_NUMBER;
        if (string.IsNullOrEmpty (projectNumber))
          throw new ArgumentException ("ANDROID_GOOGLE_PROJECT_NUMBER cannot be null when Reach is enabled");
      }

      // Search for manifest in build directory.
      string[] manifestPaths = null;
      string manifestPath = null, projectBuildPath = pathToBuiltProject + "/" + productName, manifestFilename = "AndroidManifest.xml";
      if (Directory.Exists(projectBuildPath)){
        try {
          manifestPaths = Directory.GetFiles (projectBuildPath, manifestFilename, SearchOption.AllDirectories);
        } catch (Exception e) {
          Debug.LogError ("An exception occured while searching for the " + manifestFilename + " file in `" + projectBuildPath + "`: " + e);
        }

        // Pick the first file.
        if (manifestPaths != null && manifestPaths.Length > 0) {
          manifestPath = manifestPaths [0];
        }
      }
     
      string androidPath = Application.dataPath + "/Plugins/Android";
      string mfFilepath = androidPath + "/AndroidManifest.xml";

      XmlNamespaceManager namespaceManager = new XmlNamespaceManager (new NameTable ());
      namespaceManager.AddNamespace ("android", EngagementPostBuild.androidNS);

      // Test Export vs Build
      if (File.Exists (manifestPath) == false) {
        // Check that Manifest exists
        if (File.Exists (mfFilepath) == false) {
          Debug.LogError ("Missing AndroidManifest.xml in Plugins/Android : execute 'File/Engagement/Generate Android Manifest'");
          return;
        }

        // Check that it contains Engagement tags
        xmlDoc = new XmlDocument ();
        xmlDoc.XmlResolver = null;

        // Create the reader.
        reader = new XmlTextReader (mfFilepath);
        xmlDoc.Load (reader);
        reader.Close ();

        root = xmlDoc.DocumentElement;
        nodes = root.SelectNodes ("//*[@tag='" + tagName + "']", namespaceManager);
        if (nodes.Count == 0)
          Debug.LogError ("Android manifest in Plugins/Android does not contain Engagement extensions : execute 'File/Engagement/Generate Android Manifest'");

        // Checking the version

        XmlNode versionNode = root.SelectSingleNode ("/manifest/application/meta-data[@android:name='engagement:unity:version']", namespaceManager);
        if (versionNode != null) {
          string ver = versionNode.Attributes ["android:value"].Value;
          if (ver != EngagementAgent.PLUGIN_VERSION.ToString ())
            versionNode = null;
        }

        if (versionNode == null)
          Debug.LogError ("EngagementPlugin has been updated : you need to execute 'File/Engagement/Generate Android Manifest' to update your application Manifest first");
				

        // Checking the checksum

        XmlNode chkNode = root.SelectSingleNode ("/manifest/application/meta-data[@android:name='engagement:unity:checksum']", namespaceManager);
        if (chkNode != null) {
          string mfchk = chkNode.Attributes ["android:value"].Value;
          if (mfchk != chk.ToString ())
            chkNode = null;
        }

        if (chkNode == null)
          Debug.LogError ("Configuration file has changed : you need to execute 'File/Engagement/Generate Android Manifest' to update your application Manifest");
				
        // Manifest already processed : nothing to do
        return;
      }

      Directory.CreateDirectory (androidPath);

      xmlDoc = new XmlDocument ();
      xmlDoc.XmlResolver = null;
      reader = new XmlTextReader (manifestPath);
      xmlDoc.Load (reader);
      reader.Close ();

      root = xmlDoc.DocumentElement;

      // Delete all the former tags
      nodes = root.SelectNodes ("//*[@tag='" + tagName + "']", namespaceManager);
      foreach (XmlNode node in nodes)
        node.ParentNode.RemoveChild (node);

      XmlNode manifestNode = root.SelectSingleNode ("/manifest", namespaceManager);
      XmlNode applicationNode = root.SelectSingleNode ("/manifest/application", namespaceManager);
      XmlNode activityNode = root.SelectSingleNode ("/manifest/application/activity[@android:label='@string/app_name']", namespaceManager);

      string activity = EngagementConfiguration.ANDROID_UNITY3D_ACTIVITY;
      if (activity == null || activity == "")
        activity = "com.unity3d.player.UnityPlayerActivity";

      // Already in the Unity Default Manifest
      //	EngagementPostBuild.addUsesPermission(xmlDoc,manifestNode,namespaceManager,"android.permission.INTERNET");
      EngagementPostBuild.addUsesPermission (xmlDoc, manifestNode, namespaceManager, "android.permission.ACCESS_NETWORK_STATE");

      EngagementPostBuild.addMetaData (xmlDoc, applicationNode, namespaceManager, "engagement:log:test", XmlConvert.ToString (EngagementConfiguration.ENABLE_NATIVE_LOG));
      EngagementPostBuild.addMetaData (xmlDoc, applicationNode, namespaceManager, "engagement:unity:version", EngagementAgent.PLUGIN_VERSION);
      EngagementPostBuild.addMetaData (xmlDoc, applicationNode, namespaceManager, "engagement:unity:checksum", chk.ToString ());

      XmlNode service = xmlDoc.CreateNode (XmlNodeType.Element, "service", null);
      service.Attributes.Append (xmlDoc.CreateAttribute ("tag")).Value = tagName;
      service.Attributes.Append (xmlDoc.CreateAttribute ("android", "exported", EngagementPostBuild.androidNS)).Value = "false";
      service.Attributes.Append (xmlDoc.CreateAttribute ("android", "label", EngagementPostBuild.androidNS)).Value = productName + "Service";
      service.Attributes.Append (xmlDoc.CreateAttribute ("android", "name", EngagementPostBuild.androidNS)).Value = "com.microsoft.azure.engagement.service.EngagementService";
      service.Attributes.Append (xmlDoc.CreateAttribute ("android", "process", EngagementPostBuild.androidNS)).Value = ":Engagement";
      applicationNode.AppendChild (service);

      string targetAARPath = Application.dataPath + "/Plugins/Android/engagement_notification_icon.aar";
      File.Delete (targetAARPath);
      File.Delete (targetAARPath + ".meta");

      if (EngagementConfiguration.ENABLE_REACH == true) {

        EngagementPostBuild.addActivity (xmlDoc, applicationNode, namespaceManager, "EngagementWebAnnouncementActivity", "ANNOUNCEMENT", "Light", "text/html");
        EngagementPostBuild.addActivity (xmlDoc, applicationNode, namespaceManager, "EngagementTextAnnouncementActivity", "ANNOUNCEMENT", "Light", "text/plain");
        EngagementPostBuild.addActivity (xmlDoc, applicationNode, namespaceManager, "EngagementPollActivity", "POLL", "Light", null);
        EngagementPostBuild.addActivity (xmlDoc, applicationNode, namespaceManager, "EngagementLoadingActivity", "LOADING", "Dialog", null);

        const string reachActions = "android.intent.action.BOOT_COMPLETED," +
                                            "com.microsoft.azure.engagement.intent.action.AGENT_CREATED," +
                                            "com.microsoft.azure.engagement.intent.action.MESSAGE," +
                                            "com.microsoft.azure.engagement.reach.intent.action.ACTION_NOTIFICATION," +
                                            "com.microsoft.azure.engagement.reach.intent.action.EXIT_NOTIFICATION," +
                                            "com.microsoft.azure.engagement.reach.intent.action.DOWNLOAD_TIMEOUT";

        EngagementPostBuild.addReceiver (xmlDoc, applicationNode, namespaceManager, "reach.EngagementReachReceiver", reachActions.Split (','));

        const string downloadActions = "android.intent.action.DOWNLOAD_COMPLETE";
        EngagementPostBuild.addReceiver (xmlDoc, applicationNode, namespaceManager, "reach.EngagementReachDownloadReceiver", downloadActions.Split (','));

        // Add GCM Support
        if (string.IsNullOrEmpty (EngagementConfiguration.ANDROID_GOOGLE_PROJECT_NUMBER) == false) {

          EngagementPostBuild.addMetaData (xmlDoc, applicationNode, namespaceManager, "engagement:gcm:sender", EngagementConfiguration.ANDROID_GOOGLE_PROJECT_NUMBER + "\\n");

          const string gcmActions = "com.microsoft.azure.engagement.intent.action.APPID_GOT";
          EngagementPostBuild.addReceiver (xmlDoc, applicationNode, namespaceManager, "gcm.EngagementGCMEnabler", gcmActions.Split (','));

          XmlNode receiver = xmlDoc.CreateNode (XmlNodeType.Element, "receiver", null);
          receiver.Attributes.Append (xmlDoc.CreateAttribute ("tag")).Value = tagName;
          receiver.Attributes.Append (xmlDoc.CreateAttribute ("android", "name", EngagementPostBuild.androidNS)).Value = "com.microsoft.azure.engagement.gcm.EngagementGCMReceiver";
          receiver.Attributes.Append (xmlDoc.CreateAttribute ("android", "permission", EngagementPostBuild.androidNS)).Value = "com.google.android.c2dm.permission.SEND";

          XmlNode intentReceiver = xmlDoc.CreateNode (XmlNodeType.Element, "intent-filter", null);
          receiver.AppendChild (intentReceiver);
          intentReceiver.AppendChild (xmlDoc.CreateNode (XmlNodeType.Element, "action", null))
                        .Attributes.Append (xmlDoc.CreateAttribute ("android", "name", EngagementPostBuild.androidNS)).Value = "com.google.android.c2dm.intent.REGISTRATION";
          intentReceiver.AppendChild (xmlDoc.CreateNode (XmlNodeType.Element, "action", null))
                        .Attributes.Append (xmlDoc.CreateAttribute ("android", "name", EngagementPostBuild.androidNS)).Value = "com.google.android.c2dm.intent.RECEIVE";
          intentReceiver.AppendChild (xmlDoc.CreateNode (XmlNodeType.Element, "category", null))
                        .Attributes.Append (xmlDoc.CreateAttribute ("android", "name", EngagementPostBuild.androidNS)).Value = bundleId;

          applicationNode.AppendChild (receiver);

          string permissionPackage = bundleId + ".permission.C2D_MESSAGE";
          EngagementPostBuild.addUsesPermission (xmlDoc, manifestNode, namespaceManager, permissionPackage);

          XmlNode permission = xmlDoc.CreateNode (XmlNodeType.Element, "permission", null);
          permission.Attributes.Append (xmlDoc.CreateAttribute ("tag")).Value = tagName;
          permission.Attributes.Append (xmlDoc.CreateAttribute ("android", "name", EngagementPostBuild.androidNS)).Value = permissionPackage;
          permission.Attributes.Append (xmlDoc.CreateAttribute ("android", "protectionLevel", EngagementPostBuild.androidNS)).Value = "signature";
          manifestNode.AppendChild (permission);

        }

        const string datapushActions = "com.microsoft.azure.engagement.reach.intent.action.DATA_PUSH";
        EngagementPostBuild.addReceiver (xmlDoc, applicationNode, namespaceManager, "shared.EngagementDataPushReceiver", datapushActions.Split (','));

        EngagementPostBuild.addUsesPermission (xmlDoc, manifestNode, namespaceManager, "android.permission.WRITE_EXTERNAL_STORAGE");
        EngagementPostBuild.addUsesPermission (xmlDoc, manifestNode, namespaceManager, "android.permission.DOWNLOAD_WITHOUT_NOTIFICATION");
        EngagementPostBuild.addUsesPermission (xmlDoc, manifestNode, namespaceManager, "android.permission.VIBRATE");
        EngagementPostBuild.addUsesPermission (xmlDoc, manifestNode, namespaceManager, "com.google.android.c2dm.permission.RECEIVE");

        string icon = "app_icon"; // using Application icon as default

        if (string.IsNullOrEmpty (EngagementConfiguration.ANDROID_REACH_ICON) == false) {
          // The only way to add resources to the APK is through a AAR library : let just create one with the icon
          icon = "engagement_notification_icon";
          string srcAARPath = Application.dataPath + "/EngagementPlugin/Editor/engagement_notification_icon.zip";
          string iconPath = Application.dataPath + "/" + EngagementConfiguration.ANDROID_REACH_ICON;
          File.Copy (srcAARPath, targetAARPath, true);
          ZipFile zip = ZipFile.Read (targetAARPath);
          zip.AddFile (iconPath).FileName = "res/drawable/" + icon + ".png";
          zip.Save ();
        }

        EngagementPostBuild.addMetaData (xmlDoc, applicationNode, namespaceManager, "engagement:reach:notification:icon", icon);

        XmlNode catcherActivity = xmlDoc.CreateNode (XmlNodeType.Element, "activity", null);
        catcherActivity.Attributes.Append (xmlDoc.CreateAttribute ("tag")).Value = "Engagement";
        catcherActivity.Attributes.Append (xmlDoc.CreateAttribute ("android", "label", EngagementPostBuild.androidNS)).Value = "@string/app_name";
        catcherActivity.Attributes.Append (xmlDoc.CreateAttribute ("android", "name", EngagementPostBuild.androidNS)).Value = "com.microsoft.azure.engagement.unity.IntentCatcherActivity";

        XmlNode intent = xmlDoc.CreateNode (XmlNodeType.Element, "intent-filter", null);
        intent.Attributes.Append (xmlDoc.CreateAttribute ("tag")).Value = tagName;

        intent.AppendChild (xmlDoc.CreateNode (XmlNodeType.Element, "action", null))
                    .Attributes.Append (xmlDoc.CreateAttribute ("android", "name", EngagementPostBuild.androidNS)).Value = "android.intent.action.VIEW";

        intent.AppendChild (xmlDoc.CreateNode (XmlNodeType.Element, "category", null))
                    .Attributes.Append (xmlDoc.CreateAttribute ("android", "name", EngagementPostBuild.androidNS)).Value = "android.intent.category.DEFAULT";

        intent.AppendChild (xmlDoc.CreateNode (XmlNodeType.Element, "category", null))
                    .Attributes.Append (xmlDoc.CreateAttribute ("android", "name", EngagementPostBuild.androidNS)).Value = "android.intent.category.BROWSABLE";

        intent.AppendChild (xmlDoc.CreateNode (XmlNodeType.Element, "data", null))
                    .Attributes.Append (xmlDoc.CreateAttribute ("android", "scheme", EngagementPostBuild.androidNS)).Value = EngagementConfiguration.ACTION_URL_SCHEME;

        catcherActivity.AppendChild (intent);
        applicationNode.AppendChild (catcherActivity);

        EngagementPostBuild.addMetaData (xmlDoc, applicationNode, namespaceManager, "engagement:unity:activityname", activity);
      }

      if (EngagementConfiguration.LOCATION_REPORTING_MODE == LocationReportingMode.BACKGROUND) {
        string bootReceiverActions = "android.intent.action.BOOT_COMPLETED";
        EngagementPostBuild.addReceiver (xmlDoc, applicationNode, namespaceManager, "EngagementLocationBootReceiver", bootReceiverActions.Split (','));
      }

      if (EngagementConfiguration.LOCATION_REPORTING_TYPE == LocationReportingType.LAZY || EngagementConfiguration.LOCATION_REPORTING_TYPE == LocationReportingType.REALTIME)
        EngagementPostBuild.addUsesPermission (xmlDoc, manifestNode, namespaceManager, "android.permission.ACCESS_COARSE_LOCATION");
      else if (EngagementConfiguration.LOCATION_REPORTING_TYPE == LocationReportingType.FINEREALTIME)
        EngagementPostBuild.addUsesPermission (xmlDoc, manifestNode, namespaceManager, "android.permission.ACCESS_FINE_LOCATION");

      if (EngagementConfiguration.LOCATION_REPORTING_MODE == LocationReportingMode.BACKGROUND || EngagementConfiguration.ENABLE_REACH == true)
        EngagementPostBuild.addUsesPermission (xmlDoc, manifestNode, namespaceManager, "android.permission.RECEIVE_BOOT_COMPLETED");

      // Update the manifest
      TextWriter textWriter = new StreamWriter (manifestPath);
      xmlDoc.Save (textWriter);
      textWriter.Close ();

      // revert the bundle id
      activityNode.Attributes.Append (xmlDoc.CreateAttribute ("android", "name", EngagementPostBuild.androidNS)).Value = activity;

      Debug.Log ("Generating :" + mfFilepath);
      mfWriter = new StreamWriter (mfFilepath);
      xmlDoc.Save (mfWriter);
      mfWriter.Close ();
    }

  }
}

#pragma warning restore 162, 429