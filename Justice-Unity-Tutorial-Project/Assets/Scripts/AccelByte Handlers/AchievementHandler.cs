// Copyright (c) 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using AccelByte.Api;
using AccelByte.Models;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementHandler : MonoBehaviour
{
    public GameObject AchievementWindow;

    [SerializeField]
    private Button backToMainMenuButton;

    [SerializeField]
    private Transform achievementsContent;

    [SerializeField]
    private GameObject achievementDisplayPrefab;

    /// <summary>
    /// Setup Achievement UI
    /// </summary>
    public void Setup()
    {
        DisplayAchievements();
        backToMainMenuButton.onClick.AddListener(() => 
        {
            AchievementWindow.SetActive(false);
            GetComponent<MenuHandler>().Menu.gameObject.SetActive(true);
        });
    }

    /// <summary>
    /// Display Achievement Menu List
    /// </summary>
    private void DisplayAchievements()
    {
        string language = "en";
        AchievementSortBy sortBy = AchievementSortBy.LISTORDER;
        int offset = 0;
        int limit = 99;

        AccelBytePlugin.GetAchievement().QueryUserAchievements(sortBy, userResult =>
        {
            if (userResult.IsError)
            {
                Debug.Log($"Error QueryUserAchievements, Error Code: {userResult.Error.Code} Error Message: {userResult.Error.Message}");
            }
            else
            {
                AccelBytePlugin.GetAchievement().QueryAchievements(language, sortBy, result =>
                {
                    if (result.IsError)
                    {
                        Debug.Log($"Error QueryAchievements, Error Code: {result.Error.Code} Error Message: {result.Error.Message}");
                    }
                    else
                    {
                        LoopThroughTransformAndDestroy(achievementsContent);

                        // Do something if QueryAchievements has been successful
                        foreach (PublicAchievement publicAchievementData in result.Value.data)
                        {
                            // find the current achievement's related data in UserAchievement's result data
                            UserAchievement userAchievementData = new UserAchievement();
                            foreach (UserAchievement userAchievement in userResult.Value.data)
                            {
                                if (userAchievement.achievementCode == publicAchievementData.achievementCode)
                                {
                                    userAchievementData = userAchievement;
                                    break;
                                }
                            }

                            AchievementDisplayPanel achievementDisplay = Instantiate(achievementDisplayPrefab, achievementsContent).GetComponent<AchievementDisplayPanel>();
                            achievementDisplay.Create(publicAchievementData, userAchievementData);
                        }
                    }
                }, offset, limit);
            }
        }, offset, limit);
    }

    /// <summary>
    /// A utility function to Destroy all Children of the parent transform. Optionally do not remove a specific Transform
    /// </summary>
    /// <param name="parent">Parent Object to destroy children</param>
    /// <param name="doNotRemove">Optional specified Transform that should NOT be destroyed</param>
    private static void LoopThroughTransformAndDestroy(Transform parent, Transform doNotRemove = null)
    {
        //Loop through all the children and add them to a List to then be deleted
        List<GameObject> toBeDeleted = new List<GameObject>();
        foreach (Transform t in parent)
        {
            //except the Do Not Remove transform if there is one
            if (t != doNotRemove)
            {
                toBeDeleted.Add(t.gameObject);
            }
        }
        //Loop through list and Delete all Children
        for (int i = 0; i < toBeDeleted.Count; i++)
        {
            Destroy(toBeDeleted[i]);
        }
    }
}
