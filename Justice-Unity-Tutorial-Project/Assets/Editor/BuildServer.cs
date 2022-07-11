// Copyright (c) 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using UnityEditor;

class BuildServer
{
    static void PerformBuild()
    {
        string[] scenes = {"Assets/Scenes/SampleScene.unity",
                            "Assets/Scenes/WatchTime.unity"};

        string pathToDeploy = "Build/Server/Justice-Unity-Tutorial-Project.exe";

        BuildPipeline.BuildPlayer(scenes, pathToDeploy, BuildTarget.StandaloneWindows64, BuildOptions.EnableHeadlessMode);
    }
}