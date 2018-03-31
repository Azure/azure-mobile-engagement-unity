# Azure Mobile Engagement service has been retired and is no longer available.

## Azure Mobile Engagement : Unity SDK

### Installation
* Import the`Engagement.unitypackage`into your Unity project
* Edit the configuration file `EngagementPlugin/EngagementConfiguration.cs` with your credentials
* Add the SDK APis into your code (see the *Integration* Section)
* Excute *File/Engagement/Generate Android Manifest* to update your project settings (needed to be executed everytime the configuration file is beging modified)

### Configuration
The configuration of your application is performed via an `EngagementConfiguration` class located in the `EngagementPlugin/` directory. 

##### Engagement Configuration

###### Windows Universal 10 / Windows Phone 8.1
- `WINDOWS_CONNECTION_STRING` : the Windows connection string (to retrieve from the Engagement portal)
###### Android
- `ANDROID_CONNECTION_STRING` : the Android connection string (to retrieve from the Engagement portal)
- `ANDROID_UNITY3D_ACTIVITY`: needs to be filled if your application does not use the default Unity Activity (`com.unity3d.player.UnityPlayerActivity`)

###### iOS
- `IOS_CONNECTION_STRING` : the iOS connection string (to retrieve from the Engagement portal)
- `IOS_DISABLE_IDFA` : `true`|`false` : to disable the IDFA integration on iOS

###### Generic
- `ENABLE_PLUGIN_LOG` : `true`|`false`, enable the plugin debug logs
- `ENABLE_NATIVE_LOG` : `true`|`false`, enable the Engagement native SDK to debug logs
- It is recommended to set both to `true` while working on the plugin integration.

##### Location Reporting
- `LOCATION_REPORTING_TYPE`: Can be one of the following (please refer to the Native SDK documentation for more information)
	* `LocationReportingType.NONE`
	* `LocationReportingType.LAZY`
	* `LocationReportingType.REALTIME`
	* `LocationReportingType.FINEREALTIME`
- `LOCATION_REPORTING_MODE`: Can be one of the following (please refer to the Native SDK documentation for more information)
    * `LocationReportingMode.NONE`
	* `LocationReportingMode.FOREGROUND`
	* `LocationReportingMode.BACKGROUND`

##### Reach support

###### Generic
- `ENABLE_REACH` : `true`|`false`, to enable the reach integration 
- `ACTION_URL_SCHEME` : the url scheme of your application when using redirect actions in your campaign

###### iOS
- `IOS_REACH_ICON` : the path (relative to the *Assets/* directory) of the icon to display reach notification on iOS. If not specified, the application icon will be used

###### Android
- `ANDROID_REACH_ICON` : the path (relative to the *Assets/* directory) of the icon to display reach notification on Android. If not specified, the application icon will be used
- `ANDROID_GOOGLE_PROJECT_NUMBER` : the project number used as the GCM (Google Cloud Messaging) sender ID

##### Notes
* On iOS, you need to add enable the Push Notification in your XCode capabilities  
* Do not follow the installation instruction from the Engagement native SDK for Android or iOS : in the Unity Engagement SDK, the application configuration is performed automatically (cf. `EngagementPlugin/Editor/EngagementPostBuild.cs` to see how it works under the hood).


###  Basic Integration
To initialize the Engagement service, just call `EngagementAgent.Initialize()`. No arguments are needed as the credentials are automatically retrieved from the `EngagementConfiguration`class.

##### Example
Basic initialization:
```C#
	void Start () {
        // initialize the application
		EngagementAgent.Initialize ();
		// start your first activity
		EngagementAgent.StartActivity ("home");
	}
```

### Reach Integration
To be able to receive pushes from your application, you need to call  `EngagementReach.initialize()` and define the 3 delegates (events) to be called when a push related event is received
* `StringDataPushReceived(string _category, string _body)` to receive text data push
* `Base64DataPushReceived(string _category, byte[] data, string _encodedbody)` to receive binary push (through byte[] and base64 encoded string)
* `HandleURL(string _url)` when an application specific URL is triggered (from a push campaign for example)

##### Notes
* Reach must be enabled in the configuration file by setting the `EngagementConfiguration.ENABLE_REACH`variable
* The URL scheme must match the one defined in the `EngagementConfiguration.ACTION_URL_SCHEME` setting

##### Example
Initialization with push support :
```C#
	void Start () {
        // initialize the Engagement Agent
		EngagementAgent.Initialize ();
		
		// set the Events
		EngagementReach.HandleURL += (string _push) => {
			Debug.Log ("OnHandleURL " + _push);
		};

		EngagementReach.StringDataPushReceived += (string _category, string _body) => {
			Debug.Log ("StringDataPushReceived category:" + _category + ", body:" + _body);
		};

		EngagementReach.Base64DataPushReceived += (string _category, byte[] _data, string _body) => {
			Debug.Log ("Base64DataPushReceived category:" + _category);
		};
	    //  Activate the push notification support
	    EngagementReach.Initialize();
	    // start your first activity
		EngagementAgent.StartActivity ("home");
	}

```

### Full API

##### Initialization
* `EngagementAgent.Initialize`
* `EngagementReach.Initialize`

(see above)

##### Reporting APIs 
* `EngagementAgent.StartActivity`
* `EngagementAgent.EndActivity`
* `EngagementAgent.StartJob`
* `EngagementAgent.EndJob`
* `EngagementAgent.SendJobEvent`
* `EngagementAgent.SendJobError`
* `EngagementAgent.SendEvent`
* `EngagementAgent.SendError`
* `EngagementAgent.SendSessionEvent`
* `EngagementAgent.SendSessionError`
* `EngagementAgent.SendAppInfo`
(see the AZME documentation)

##### Miscellaneous:
* `EngagementAgent.SaveUserPreferences`
* `EngagementAgent.RestoreUserPreferences`
* `EngagementAgent.SetEnabled`
* `EngagementAgent.GetStatus`

### Sample Application
A sample application is available in the `sample`directory.
* Execute `build-sample.sh`to create a new sample project
* Once done, the Unity Editor should be displayed with the `sample-project`openned
* Edit the `EngagementPlugin/EngagementConfiguration.cs`with your credentials (see the configuration section)
* In the Unity *PlayerSettings*, set the bundle id for your application 
* iOS:
   * Go to *File/Build Settings/iOS* and press *Build* to create the XCode projet
   * From XCode, activate the *Push Notification* from the *Capabilities* menu
   * Build and run from XCode!
* Android:
   * Go to *File/Engagement/Generate Android Manifest* to create a manifest file with the requirements for Engagement
   * Go to *File/Build Settings/Android* and press *Build&Run* to launch the sample

### Building from source
The source code of the plugin are included in the `src/`directory. To build the package, just execute the `package.sh` script at the root of the SDK. 

It only works on Mac OSX, with XCode, Unity and Android Studio installed.

### History

##### 1.3.0
* Updated iOS SDK to 4.1.0 (iOS 6 deprecation, iOS 11 support)
* Updated iOS SDK to 4.3.1 (Android O support)
* Fix generated Android manifest file not always detected

##### 1.2.2
* Updated iOS SDK to 4.0.1

##### 1.2.1
* Fix possible duplicate symbols when building iOS project in Unity 5.4

##### 1.2.0 
* Updated iOS SDK to 4.0.0
* Updated Android SDK to 4.2.3
* Fix iOS support for Unity 5.3.1+ versions

##### 1.1.0 
* Added Windows Phone 8.1/Windows Universal 10 support

##### 1.0.0 
* Initial release

## Open Source Code of Conduct
This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
