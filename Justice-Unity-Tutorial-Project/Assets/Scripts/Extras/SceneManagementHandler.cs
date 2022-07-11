// Copyright (c) 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneManagementHandler
{
    public enum SceneName
    {
        SampleScene,
        WatchTime
    }

    /// <summary>
    /// Change active scene
    /// </summary>
    /// <param name="scene"> Scene name that will be set active</param>
    public static void ChangeScene(SceneName scene)
    {
        SceneManager.LoadScene(scene.ToString());
    }
}
