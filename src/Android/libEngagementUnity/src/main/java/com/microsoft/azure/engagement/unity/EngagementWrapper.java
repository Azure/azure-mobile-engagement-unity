/*
 * Copyright (c) Microsoft Corporation.  All rights reserved.
 * Licensed under the MIT license. See License.txt in the project root for license information.
 */

package com.microsoft.azure.engagement.unity;

import android.app.Activity;
import android.net.Uri;
import android.util.Log;
import org.json.JSONObject;
import android.content.pm.ApplicationInfo;
import android.content.pm.PackageManager;
import android.os.Bundle;

import com.microsoft.azure.engagement.shared.EngagementShared;
import com.microsoft.azure.engagement.shared.EngagementDelegate;

import com.unity3d.player.UnityPlayer;

public class EngagementWrapper {

    private static final String pluginName = "UNITY";
    private static final String pluginVersion = "1.0.0";
    private static final String nativeVersion = "4.1.3"; // to eventually retrieve from the SDK itself
    private static final String unityMethod_onDataPushReceived = "onDataPushReceived";
    private static final String unityMethod_onHandleUrl = "onHandleURL";
    private static final String unityMethod_onStatusReceived= "onStatusReceived";

    private static Activity androidActivity ;

    private static String openURL;
    private static String unityObjectName  = null;

    // Helper

    private static void UnitySendMessage(String _method,String _message) {
        if ( unityObjectName == null)
            Log.e(EngagementShared.LOG_TAG, "Missing unityObjectMethod");
        UnityPlayer.UnitySendMessage(unityObjectName, _method, _message);
    }

    private static EngagementDelegate engagementDelegate = new EngagementDelegate() {
        @Override
        public void didReceiveDataPush(JSONObject _data) {
            UnitySendMessage(unityMethod_onDataPushReceived, _data.toString());
        }
    };

    public static void handleOpenURL(String _url)
    {
        Log.i(EngagementShared.LOG_TAG, "handleOpenURL: " + _url);
        openURL = _url;
    }

    public static void processOpenUrl()
    {
        if (openURL == null)
            return ;
        Log.i(EngagementShared.LOG_TAG,"onHandleOpenURL: "+openURL);
        UnitySendMessage(unityMethod_onHandleUrl, openURL);
        openURL = null;
    }

    // Unity Interface

    public static void setAndroidActivity(Activity _androidActivity) {

        androidActivity = _androidActivity;
        Uri data = _androidActivity.getIntent().getData();
        if (data != null) {
            String lastUrl = data.toString();
            handleOpenURL(lastUrl);
        }
    }

    public static void registerApp( String _instanceName, String _connectionString,
                                    int _locationType, int  _locationMode, boolean _enablePluginLog)
    {
        unityObjectName =  _instanceName;

        if (androidActivity == null)
        {
            Log.e(EngagementShared.LOG_TAG,"missing AndroidActivty (setAndroidActivity() not being called?)");
            return ;
        }

        if (EngagementShared.instance().alreadyInitialized())
        {
            Log.e(EngagementShared.LOG_TAG,"registerApp() already called");
            return ;
        }

        try {
            ApplicationInfo ai = androidActivity.getPackageManager().getApplicationInfo(androidActivity.getPackageName(), PackageManager.GET_META_DATA);
            Bundle bundle = ai.metaData;
            String mfPluginVersion = bundle.getString("engagement:unity:version");
            if (mfPluginVersion == null)
                throw new PackageManager.NameNotFoundException();
            if (pluginVersion.equals(mfPluginVersion)==false)
                Log.i(EngagementShared.LOG_TAG, "Unity Plugin Version (" + pluginVersion +") does not match manifest version ("+mfPluginVersion+") : Manifest might need to be regenerated");
        }
        catch ( Exception e) {
            Log.e(EngagementShared.LOG_TAG, "Cannot find engagement:unity:version in Android Manifest : Manifest file needs to be generated through File/Engagement/Generate Android Manifest");
        }

        EngagementShared.instance().setPluginLog(_enablePluginLog);
        EngagementShared.instance().initSDK(pluginName, pluginVersion, nativeVersion);
        EngagementShared.instance().setDelegate(engagementDelegate);

        EngagementShared.locationReportingType locationReporting = EngagementShared.locationReportingType.fromInteger(_locationType);
        EngagementShared.backgroundReportingType background  = EngagementShared.backgroundReportingType.fromInteger(_locationMode);

        EngagementShared.instance().initialize(androidActivity, _connectionString, locationReporting, background);

        // We consider the app to be active on registerApp as onResume() is not being automatically called
        EngagementShared.instance().onResume();

    }

    public static void initializeReach() {

        processOpenUrl();
        EngagementShared.instance().enableDataPush();
    }

    public static void startActivity(String _activityName, String _extraInfos) {

        EngagementShared.instance().startActivity(_activityName, _extraInfos);
    }

    public static void endActivity() {

        EngagementShared.instance().endActivity();
    }

    public static void startJob(String _jobName, String _extraInfos) {

        EngagementShared.instance().startJob(_jobName, _extraInfos);
    }

    public static void endJob(String _jobName) {

        EngagementShared.instance().endJob(_jobName);
    }

    public static void sendEvent(String _eventName, String _extraInfos) {

        EngagementShared.instance().sendEvent(_eventName, _extraInfos);
    }

    public static void sendAppInfo(String _extraInfos) {

        EngagementShared.instance().sendAppInfo(_extraInfos);
    }

    public static  void sendSessionEvent(String _eventName, String _extraInfos)
    {
        EngagementShared.instance().sendSessionEvent(_eventName, _extraInfos);
    }

    public static  void sendJobEvent(String _eventName, String _jobName,String _extraInfos)
    {
        EngagementShared.instance().sendJobEvent(_eventName, _jobName, _extraInfos);
    }

    public static  void sendError(String _errorName, String _extraInfos)
    {
        EngagementShared.instance().sendError(_errorName, _extraInfos);
    }

    public static  void sendSessionError(String _errorName, String _extraInfos)
    {
        EngagementShared.instance().sendSessionError(_errorName, _extraInfos);
    }

    public static  void sendJobError(String _errorName, String _jobName, String _extraInfos)
    {
        EngagementShared.instance().sendJobError(_errorName, _jobName, _extraInfos);
    }

    public static void getStatus() {

        EngagementShared.instance().getStatus(new EngagementDelegate() {
            @Override
            public void onGetStatusResult(JSONObject _result) {
                UnitySendMessage(unityMethod_onStatusReceived, _result.toString());
            }
        });
    }

    public static void setEnabled(boolean _enabled) {

        EngagementShared.instance().setEnabled(_enabled);
    }

    public static void onApplicationPause(boolean _paused) {
        final boolean paused = _paused;

        // Check if there's an url to be processed
        processOpenUrl();

        // When clicking on a view, we may be called from another thread
        UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
            public void run() {
                if (paused) {
                    EngagementShared.instance().onPause();
                } else {
                    EngagementShared.instance().onResume();
                }
            }
        });

    }

}
