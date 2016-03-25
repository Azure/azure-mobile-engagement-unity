
/*
 * Copyright (c) Microsoft Corporation.  All rights reserved.
 * Licensed under the MIT license. See License.txt in the project root for license information.
 */

package com.microsoft.azure.engagement.shared;

import java.io.UnsupportedEncodingException;
import java.net.URLEncoder;
import java.util.Map;
import java.util.TreeMap;
import android.annotation.TargetApi;

import android.content.Context;
import android.content.SharedPreferences;
import android.util.Log;

import com.microsoft.azure.engagement.reach.EngagementReachDataPushReceiver;

import org.json.JSONException;
import org.json.JSONObject;

public class EngagementDataPushReceiver extends EngagementReachDataPushReceiver
{
    public static final String ENGAGEMENT_PREFERENCES = "EngagementDataPush";

    //  http://stackoverflow.com/questions/607176/java-equivalent-to-javascripts-encodeuricomponent-that-produces-identical-outpu
    public static String encodeURIComponent(String s) {
        if (s==null)
            return null;
        String result = null;
        try {
            
                result = URLEncoder.encode(s, "UTF-8")
                        .replaceAll("\\+", "%20")
                        .replaceAll("\\%21", "!")
                        .replaceAll("\\%28", "(")
                        .replaceAll("\\%29", ")")
                        .replaceAll("\\%7E", "~");
        }
        catch (UnsupportedEncodingException e) {
            Log.e(EngagementShared.LOG_TAG,"Unsupported Encoding");
        }
        return result;
    }

    @TargetApi(9)
    public static Map<String,String> getPendingDataPushes(Context context) {

        Map<String, String> smap = new TreeMap<String, String>();

        SharedPreferences settings = context.getSharedPreferences(ENGAGEMENT_PREFERENCES, 0/*MODE_PRIVATE*/);
        Map<String,?> m = settings.getAll();

        // convert to treemap to keep the order by timestamp
        for (Map.Entry<String, ?> entry : m.entrySet()) {
            smap.put(entry.getKey(), entry.getValue().toString());

        }
        // remove all
        settings.edit().clear().apply();

        return smap;
    }

    @TargetApi(9)
    public static void addDataPush(Context context,String category, String body, boolean isBase64) {
        SharedPreferences settings = context.getSharedPreferences(ENGAGEMENT_PREFERENCES, 0/*MODE_PRIVATE*/);
        SharedPreferences.Editor prefEditor = settings.edit();

        Long tsLong = System.currentTimeMillis()/1000;

        JSONObject json = new JSONObject();

        try {
            json.put("isBase64",isBase64);
            if (category == null)
                json.put("category", JSONObject.NULL);
            else
                json.put("category",category);
            json.put("body",body);
        } catch (JSONException e) {
            Log.e(EngagementShared.LOG_TAG, "Cannot store push");
            return ;
        }

        String ts = tsLong.toString();
        String value=  json.toString(); //category+" "+body;
        prefEditor.putString(ts, value);
        prefEditor.apply();

        final int MAX_CHAR = 128;
        int maxLength = (value.length() < MAX_CHAR)?value.length():MAX_CHAR;
        Log.i(EngagementShared.LOG_TAG, "received data push (" + ts + ") : " + value.substring(0,maxLength));
    }

    @Override
    protected Boolean onDataPushStringReceived(Context context, String category, String body) {

        String encodedCategory = encodeURIComponent(category);
        String encodedBody = encodeURIComponent(body);
       
        addDataPush(context.getApplicationContext(), encodedCategory, encodedBody, false);
        EngagementShared.instance().checkDataPush();

        return true;
    }

    @Override
    protected Boolean onDataPushBase64Received(Context context, String category, byte[] decodedBody, String encodedBody) {

        String encodedCategory = encodeURIComponent(category);
        addDataPush(context.getApplicationContext(),encodedCategory,encodedBody,true);
        EngagementShared.instance().checkDataPush();

        return true;
    }
}