// Copyright (c) 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using AccelByte.Api;
using AccelByte.Models;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class AchievementPopUpPanel : MonoBehaviour
{
    [SerializeField]
    private Image iconImage;
    
    [SerializeField]
    private Text achievementNameText;
    [SerializeField]
    private Text achievementDescriptionText;
    [SerializeField]
    private Text rewardsText;

    
    /// <summary>
    /// Prepare Achievement Pop Up Panel's UI
    /// </summary>
    /// <param name="achievementData"></param>
    public void Setup(MultiLanguageAchievement achievementData)
    {
        Debug.Log("Updating Achievement Pop Up Panel UI");

        const string language = "en";

        achievementNameText.text = achievementData.name[language];
        achievementDescriptionText.text = achievementData.description[language];
        // Set rewards text UI here..

        AchievementIcon[] achievementIcons = achievementData.unlockedIcons;
        // display Icon image
        if (achievementIcons.Length > 0)
        {
            string imageUrl = achievementIcons[0].url;
            // Get Item Image from url
            StartCoroutine(ABUtilities.DownloadTexture2D(imageUrl, imageResult =>
            {
                iconImage.sprite = Sprite.Create(imageResult.Value, new Rect(0f, 0f, imageResult.Value.width, imageResult.Value.height), Vector2.zero);
            }));
        }
    }
}
