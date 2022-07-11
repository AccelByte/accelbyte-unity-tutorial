// Copyright (c) 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using AccelByte.Api;
using AccelByte.Models;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class AchievementDisplayPanel : MonoBehaviour
{
    [SerializeField]
    private Image iconImage;

    [SerializeField]
    private Text achievementNameText;
    [SerializeField]
    private Text achievementDescriptionText;

    [SerializeField]
    private Slider progressBar;


    /// <summary>
    /// Prepare Achievement Display's UI
    /// </summary>
    /// <param name="publicAchievementData"></param>
    /// <param name="userAchievementData"></param>
    public void Create(PublicAchievement publicAchievementData, UserAchievement userAchievementData)
    {
        Debug.Log("Updating Achievement Display UI..");

        // set default state
        progressBar.gameObject.SetActive(false);

        // get achievement status
        // 0 = locked(empty), 1 = in-progress, 2 = unlocked
        int achievementStatus = 0;
        if (userAchievementData != null)
        {
            achievementStatus = userAchievementData.status;
        }

        // if achievement's type is hidden and still locked
        if (publicAchievementData.hidden && achievementStatus != 2)
        {
            achievementNameText.text = "Hidden Achievement";
            achievementDescriptionText.text = "Keep playing to find out :)";
        }
        else
        {
            achievementNameText.text = publicAchievementData.name.ToString();
            achievementDescriptionText.text = publicAchievementData.description.ToString();
        }

        // if achievement's type is incremental
        if (publicAchievementData.incremental)
        {
            progressBar.gameObject.SetActive(true);

            float progressPercentValue = 0;
            // count percentage for progress bar (slider) if already in-progress/unlocked
            if (achievementStatus != 0)
            {
                progressPercentValue = userAchievementData.latestValue / publicAchievementData.goalValue;
            }
            progressBar.value = progressPercentValue;
        }

        AchievementIcon[] achievementIcons = publicAchievementData.lockedIcons;
        // if Achievement Status is unlocked
        if (achievementStatus == 2)
        {
            achievementIcons = publicAchievementData.unlockedIcons;
        }

        // display Icon image
        if (achievementIcons.Length > 0)
        {
            string imageUrl = achievementIcons[0].url;

            // Get Item Image from url
            if (!gameObject.activeInHierarchy) return;
            
            StartCoroutine(ABUtilities.DownloadTexture2D(imageUrl, imageResult =>
            {
                iconImage.sprite = Sprite.Create(imageResult.Value, new Rect(0f, 0f, imageResult.Value.width, imageResult.Value.height), Vector2.zero);
            }));
        }
    }
}
