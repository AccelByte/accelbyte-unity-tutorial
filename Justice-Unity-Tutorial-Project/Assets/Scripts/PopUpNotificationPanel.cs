// Copyright (c) 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using System;
using System.Collections;
using UnityEngine;


public class PopUpNotificationPanel : MonoBehaviour
{
    /// <summary>
    /// Private Instance
    /// </summary>
    static PopUpNotificationPanel _instance;

    /// <summary>
    /// The Instance Getter
    /// </summary>
    public static PopUpNotificationPanel Instance => _instance;

    #region Pop Up Notification Objects
    [SerializeField]
    private Transform popUpPanel;

    [SerializeField]
    private GameObject achievementPopUpPrefab;
    #endregion

    private Achievement achievement;

    private void Awake()
    {
        //Check if another Instance is already created, and if so delete this one, otherwise destroy the object
        if (_instance != null && _instance != this)
        {
            Destroy(this);
            return;
        }
        else
        {
            _instance = this;
        }

        // Keep the Pop Up Panel Object even after changing scene
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        // AccelByte's Multi Registry initialization
        ApiClient apiClient = MultiRegistry.GetApiClient();
        achievement = apiClient.GetApi<Achievement, AchievementApi>();
    }

    /// <summary>
    /// Check achievement if it's already unlocked, if haven't then call the unlockAchievement() SDK function
    /// </summary>
    /// <param name="achievementCode"></param>
    public void CheckAchievementUnlockedStatus(string achievementCode)
    {
        // check achievement's unlocked status from UserAchievement
        AchievementSortBy sortBy = AchievementSortBy.LISTORDER;
        achievement.QueryUserAchievements(sortBy, result =>
        {
            if (!result.IsError)
            {
                bool unlockedStatus = false;
                foreach (UserAchievement data in result.Value.data)
                {
                    if (data.achievementCode == achievementCode && data.status == 2)
                    {
                        unlockedStatus = true;
                    }
                }

                // if the achievement haven't unlocked yet
                if (!unlockedStatus)
                {
                    // try unlock achievement
                    achievement.UnlockAchievement(achievementCode, achievementResult =>
                    {
                        if (result.IsError)
                        {
                            Debug.Log($"Error UnlockAchievement, Error Code: {achievementResult.Error.Code} Error Message: {achievementResult.Error.Message}");
                        }
                        else
                        {
                            CreateAchievementPopUp(achievementCode);
                        }
                    });
                }
            }
        });
    }

    /// <summary>
    /// Create unlocked achievement pop up notification in pop up panel
    /// </summary>
    /// <param name="achievementCode"></param>
    public void CreateAchievementPopUp(string achievementCode)
    {
        achievement.GetAchievement(achievementCode, result =>
        {
            if (result.IsError)
            {
                Debug.Log($"Error GetAchievement, Error Code: {result.Error.Code} Error Message: {result.Error.Message}");
            }
            else
            {
                AchievementPopUpPanel achievementPopUp = Instantiate(achievementPopUpPrefab, popUpPanel).GetComponent<AchievementPopUpPanel>();
                achievementPopUp.Setup(result.Value);

                //wait 3 seconds to destroy pop up notification
                StartCoroutine(DestoryPopUpPanel(achievementPopUp.gameObject));
            }
        });
    }

    /// <summary>
    /// Delay to 3 seconds
    /// </summary>
    /// <returns></returns>
    IEnumerator DestoryPopUpPanel(GameObject achievementPopUp)
    {
        yield return new WaitForSeconds(3);

        Destroy(achievementPopUp);
    }
}
