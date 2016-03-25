/*
 * Copyright (c) Microsoft Corporation.  All rights reserved.
 * Licensed under the MIT license. See License.txt in the project root for license information.
 */

package com.microsoft.azure.engagement.shared;

import java.util.Arrays;
import java.util.Iterator;
import java.util.Map;

import android.annotation.TargetApi;
import android.app.Activity;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.os.Build;
import android.util.Log;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import android.os.Bundle;

import com.microsoft.azure.engagement.EngagementConfiguration;
import com.microsoft.azure.engagement.EngagementAgent;

public class EngagementShared  {

    public enum locationReportingType{
        LOCATIONREPORTING_NONE(100),
        LOCATIONREPORTING_LAZY(101),
        LOCATIONREPORTING_REALTIME(102),
        LOCATIONREPORTING_FINEREALTIME(103);

        private int value;

        private locationReportingType(int value) {
            this.value = value;
        }

        public static locationReportingType fromInteger(int x) {
            switch(x) {
                case 100:
                    return LOCATIONREPORTING_NONE;
                case 101:
                    return LOCATIONREPORTING_LAZY;
                case 102:
                    return LOCATIONREPORTING_REALTIME;
                case 103:
                    return LOCATIONREPORTING_FINEREALTIME;
            }
            return null;
        }

    } ;

    public enum backgroundReportingType {

        BACKGROUNDREPORTING_NONE(200),
        BACKGROUNDREPORTING_FOREGROUND(201),
        BACKGROUNDREPORTING_BACKGROUND(202);

        private int value;

        private backgroundReportingType(int value) {
            this.value = value;
        }

        public static backgroundReportingType fromInteger(int x) {
            switch(x) {
                case 200:
                    return BACKGROUNDREPORTING_NONE;
                case 201:
                    return BACKGROUNDREPORTING_FOREGROUND;
                case 202:
                    return BACKGROUNDREPORTING_BACKGROUND;
            }
            return null;
        }
    } ;

    public final static String LOG_TAG = "engagement-plugin";
    public String pluginVersion = null;
    public String nativeVersion;
    public String pluginName ;
    public boolean enablePluginLog = false;

    public boolean isPaused = true;
    private String previousActivityName = null;

    public boolean readyForPush = false;
    public EngagementDelegate delegate;
    public Activity androidActivity;

    // Singleton Pattern

    private EngagementShared() {}
    private static EngagementShared _instance = null;
    public static EngagementShared instance()
    {
        if (_instance != null)
            return _instance;

        _instance = new EngagementShared();
        return _instance;
    }

    public boolean alreadyInitialized()
    {
        return pluginVersion != null;
    }

    public void initSDK(String _pluginName, String _pluginVersion, String _nativeVersion )
    {

        pluginName = _pluginName;
        pluginVersion = _pluginVersion ;
        nativeVersion =  _nativeVersion ;

        if (enablePluginLog)
            Log.d(LOG_TAG,"Plugin "+pluginName+" v"+_pluginVersion+" (nativeVersion "+_nativeVersion+")");
    }

    public void setPluginLog(boolean _enablePluginLog)
    {
        enablePluginLog = _enablePluginLog;
    }

    public void setEnabled(boolean _enabled)  {
        EngagementAgent.getInstance(androidActivity).setEnabled(_enabled);
    }

    public void setDelegate(EngagementDelegate _delegate)
    {
        delegate = _delegate;
    }

    public void logD(String _message)
    {
        if (enablePluginLog)
            Log.d(LOG_TAG,_message);
    }

    public void logE(String _message)
    {
        Log.e(LOG_TAG,_message);
    }

    public void initialize(Activity _androidActivity,String _connectionString, locationReportingType _locationReporting, backgroundReportingType _background) {

        androidActivity = _androidActivity;

        logD("Initiliazing EngagementAgent");

        EngagementConfiguration engagementConfiguration = new EngagementConfiguration();
        engagementConfiguration.setConnectionString(_connectionString);

        if (_locationReporting ==  locationReportingType.LOCATIONREPORTING_LAZY) {
            engagementConfiguration.setLazyAreaLocationReport(true);
            logD("Lazy Area Location enabled");
        }
        else
        if (_locationReporting ==  locationReportingType.LOCATIONREPORTING_REALTIME) {
            engagementConfiguration.setRealtimeLocationReport(true);
            logD("Realtime Location enabled");
        }
        else
        if (_locationReporting ==  locationReportingType.LOCATIONREPORTING_FINEREALTIME) {
            engagementConfiguration.setRealtimeLocationReport(true);
            engagementConfiguration.setFineRealtimeLocationReport(true);
            logD("Fine Realtime Location enabled");
        }

        if (_background == backgroundReportingType.BACKGROUNDREPORTING_BACKGROUND) {
            if (_locationReporting == locationReportingType.LOCATIONREPORTING_FINEREALTIME || _locationReporting == locationReportingType.LOCATIONREPORTING_REALTIME) {
                engagementConfiguration.setBackgroundRealtimeLocationReport(true);
                logD("Background Location enabled");
            }
            else
                logE("Background mode requires realtime location");
        }
        else
        if (_background == backgroundReportingType.BACKGROUNDREPORTING_FOREGROUND) {
            if (_locationReporting == locationReportingType.LOCATIONREPORTING_NONE)
                logE("Foreground mode requires location");
        }
        else {
            if (_locationReporting != locationReportingType.LOCATIONREPORTING_NONE) {
                logE("Foreground or Background required when using location");
            }
        }

        EngagementAgent.getInstance(_androidActivity).init(engagementConfiguration);

        Bundle b = new Bundle();
        b.putString(pluginName, pluginVersion);
        EngagementAgent.getInstance(androidActivity).sendAppInfo(b);

    }

    private Bundle stringToBundle(String _param) {

        Bundle b = new Bundle();

        if (_param == null || _param.equals("null") )
            return b;

        try {
            JSONObject jObj = new JSONObject(_param);

            @SuppressWarnings("unchecked")
            Iterator<String> keys = jObj.keys();
            while (keys.hasNext()) {
                String key = keys.next();
                String val = jObj.getString(key);
                b.putString(key, val);
            }
            return b;

        } catch (JSONException e) {

            Log.e(LOG_TAG,"Failed to unserialize :"+_param+" => "+e.getMessage());
            return null;
        }
    }

    public void enableDataPush() {
        readyForPush = true;
    }

    public void checkDataPush()
    {
        if (!readyForPush || isPaused) {
             return;
        }
        Map<String,String> m = EngagementDataPushReceiver.getPendingDataPushes(androidActivity.getApplicationContext());
        for (Map.Entry<String, ?> entry : m.entrySet())
        {
            String timestamp = entry.getKey();
            logD("handling data push ("+timestamp+")");

            String v = entry.getValue().toString();
            JSONObject json = null;
            try {
                json = new JSONObject(v);
                delegate.didReceiveDataPush(json);
            } catch (JSONException e) {
                logE("Failed to prepare data push " + e.getMessage());;
            }
        }
    }

    public void getStatus(EngagementDelegate _delegate) {
        final EngagementDelegate delegate = _delegate ;

        EngagementAgent.getInstance(androidActivity).getDeviceId(new EngagementAgent.Callback<String>() {
            @Override
            public void onResult(String deviceId) {

                JSONObject json = new JSONObject();

                try {
                    json.put("pluginVersion", pluginVersion);
                    json.put("nativeVersion",nativeVersion);
                    json.put("deviceId", deviceId);
                    json.put("isEnabled", EngagementAgent.getInstance(androidActivity).isEnabled());

                    logD("getStatus:"+json.toString());

                    delegate.onGetStatusResult(json);

                } catch (JSONException e) {
                    logE("Failed to retrieve Status" + e.getMessage());
                }
            }
        });
    }

    public void startActivity(String _activityName, String _extraInfos) {

        logD("startActivity:"+_activityName+", w/"+_extraInfos);
        Bundle extraInfos = stringToBundle(_extraInfos);
        previousActivityName = _activityName;
        EngagementAgent.getInstance(androidActivity).startActivity(androidActivity, _activityName, extraInfos);
    }

    public void endActivity() {

        logD("endActivity");
        EngagementAgent.getInstance(androidActivity).endActivity();
        previousActivityName = null;
    }

    public void sendEvent(String _eventName, String _extraInfos) {

        logD("sendEvent:"+_eventName+", w/"+_extraInfos);

        Bundle extraInfos = stringToBundle(_extraInfos);
        EngagementAgent.getInstance(androidActivity).sendEvent(_eventName, extraInfos);
    }

    public void sendSessionEvent(String _eventName, String _extraInfos) {

        logD("sendSessionEvent:"+_eventName+", w/"+_extraInfos);

        Bundle extraInfos = stringToBundle(_extraInfos);
        EngagementAgent.getInstance(androidActivity).sendSessionEvent(_eventName, extraInfos);
    }

    public void startJob(String _jobName, String _extraInfos) {

        logD("startJob:"+_jobName+", w/"+_extraInfos);

        Bundle extraInfos = stringToBundle(_extraInfos);
        EngagementAgent.getInstance(androidActivity).startJob(_jobName, extraInfos);
    }

    public void endJob(String _jobName) {

        logD("endJob:"+_jobName);

        EngagementAgent.getInstance(androidActivity).endJob(_jobName);
    }

    public void sendJobEvent(String _eventName, String _jobName, String _extraInfos) {

        logD("sendJobEvent:"+_eventName+", in job:"+_jobName+" w/"+_extraInfos);

        Bundle extraInfos = stringToBundle(_extraInfos);
        EngagementAgent.getInstance(androidActivity).sendJobEvent(_eventName, _jobName, extraInfos);
    }

    public void sendError(String _errorName, String _extraInfos) {

        logD("sendError:"+_errorName+", w/"+_extraInfos);

        Bundle extraInfos = stringToBundle(_extraInfos);
        EngagementAgent.getInstance(androidActivity).sendError(_errorName, extraInfos);
    }

    public void sendSessionError(String _errorName, String _extraInfos) {

        logD("sendSessionError:"+_errorName+", w/"+_extraInfos);

        Bundle extraInfos = stringToBundle(_extraInfos);
        EngagementAgent.getInstance(androidActivity).sendSessionError(_errorName, extraInfos);
    }

    public void sendJobError(String _errorName, String _jobName, String _extraInfos) {

        logD("sendJobError:"+_errorName+", in job:"+_jobName+" w/"+_extraInfos);

        Bundle extraInfos = stringToBundle(_extraInfos);
        EngagementAgent.getInstance(androidActivity).sendJobError(_errorName, _jobName, extraInfos);
    }

    public void sendAppInfo(String _extraInfos) {

        logD("sendAppInfo:"+_extraInfos);

        Bundle extraInfos = stringToBundle(_extraInfos);
        EngagementAgent.getInstance(androidActivity).sendAppInfo(extraInfos);
    }


    public void onPause() {

        logD("onPause: endActivity");

        isPaused = true;
        EngagementAgent.getInstance(androidActivity).endActivity();
    }

    public void onResume() {

        if (previousActivityName != null) {
            logD( "onResume: startActivity:"+previousActivityName);
            EngagementAgent.getInstance(androidActivity).startActivity(androidActivity, previousActivityName, null);
        }
        else
        {
            logD("onResume (no previous activity)");
        }
        isPaused = false;
        checkDataPush();
    }

    @TargetApi(Build.VERSION_CODES.M)
    public JSONObject requestPermissions(JSONArray _permissions)
    {
        JSONObject ret = new JSONObject();
        JSONObject p = new JSONObject();
        String[] requestedPermissions = null;
        try {
            PackageInfo pi = androidActivity.getPackageManager().getPackageInfo(androidActivity.getPackageName(), PackageManager.GET_PERMISSIONS);
            requestedPermissions = pi.requestedPermissions;
            for(int i=0;i<requestedPermissions.length;i++)
                Log.d(LOG_TAG,requestedPermissions[i]);
                

        } catch (PackageManager.NameNotFoundException e) {
            logE("Failed to load permissions, NameNotFound: " + e.getMessage());
        }

        logD("requestPermissions()");

          int l = _permissions.length();
          for(int i=0;i<l;i++)
          {
              try {
                  String permission = _permissions.getString(i);
                  String androidPermission = "android.permission."+permission;

                  int grant = androidActivity.checkCallingOrSelfPermission(androidPermission);
                  try {
                      p.put(permission,grant==PackageManager.PERMISSION_GRANTED);
                  } catch (JSONException e) {
                      logE("invalid permissions "+ e.getMessage());
                  }
                  if (grant != PackageManager.PERMISSION_GRANTED) {


                      if (!Arrays.asList(requestedPermissions).contains(androidPermission))
                      {
                          String errString = "requested permission "+androidPermission+" not set in Manifest";
                          Log.e(LOG_TAG,errString);
                          try {
                              ret.put("error", errString);
                          } catch (JSONException e) {
                              logE("invalid permissions "+ e.getMessage());
                          }
                      }
                      else
                      if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
                          // Trying to request the permission if running on AndroidM
                          logD("requesting runtime permission " + androidPermission);
                          androidActivity.requestPermissions(new String[]{androidPermission}, 0);
                      }

                  }
                  else
                      logD(permission+" OK");
              }catch (JSONException e) {
                  logE("invalid permissions "+ e.getMessage());
              }

          }

          try {
              ret.put("permissions", p);
          } catch (JSONException e) {
              logE("invalid permissions "+ e.getMessage());
          }


        return ret;
    }

    public void refreshPermissions()
    {
        logD("refreshPermissions()");
        EngagementAgent.getInstance(androidActivity).refreshPermissions();
    }

}
