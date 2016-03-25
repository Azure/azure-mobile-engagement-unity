/*
 * Copyright (c) Microsoft Corporation.  All rights reserved.
 * Licensed under the MIT license. See License.txt in the project root for license information.
 */

package com.microsoft.azure.engagement.shared;

import org.json.JSONObject;

public class EngagementDelegate {
    public void onGetStatusResult(JSONObject _result) {};
    public void didReceiveDataPush(JSONObject _data ) {};
}