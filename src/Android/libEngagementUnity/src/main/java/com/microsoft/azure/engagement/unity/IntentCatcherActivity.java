/*
 * Copyright (c) Microsoft Corporation.  All rights reserved.
 * Licensed under the MIT license. See License.txt in the project root for license information.
 */

package com.microsoft.azure.engagement.unity;

import android.app.Activity;
import android.content.Intent;
import android.content.pm.ApplicationInfo;
import android.content.pm.PackageManager;
import android.net.Uri;
import android.os.Bundle;
import android.util.Log;

import com.microsoft.azure.engagement.shared.EngagementShared;

// To compensate for the lack of onNewIntent() on the UnityActivity

public class IntentCatcherActivity extends Activity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        Uri data  = getIntent().getData();
        if (data != null) {
            String url = data.toString();
            Log.i(EngagementShared.LOG_TAG, "IntentCatcher handURL " + url);
            EngagementWrapper.handleOpenURL(url);
        }

        // By default, the ActivityName is com.unity3d.player.UnityPlayerActivity, but this can be overriden in the manifest
        Class<?> clazz = com.unity3d.player.UnityPlayerActivity.class;
        try {
            ApplicationInfo ai =getPackageManager().getApplicationInfo(getPackageName(), PackageManager.GET_META_DATA);
            Bundle bundle = ai.metaData;
            String activityName = bundle.getString("engagement:unity:activityname");
            if (activityName==null)
                Log.d(EngagementShared.LOG_TAG,"could not retrieve engagement:unity:activityname");
            else
                clazz = Class.forName(activityName);

        } catch (Exception e) {
            Log.e(EngagementShared.LOG_TAG,"Failed to find activityname: " + e.getMessage());
        }

        Log.i(EngagementShared.LOG_TAG,"Launching activity"+clazz.toString());
        Intent gameIntent = new Intent(this, clazz);
        startActivity(gameIntent);

        // Close the IntentCatcherActivity
        if (!isFinishing())
            finish();
    }

}