// Copyright (c) 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatisticHandler : MonoBehaviour
{
    private const string StatCode = "vc-spending";

    /// <summary>
    /// Update vc-spending stat item
    /// </summary>
    /// <param name="value"> the value of VC that has been spent.</param>
    public void UpdateStatistic(int value)
    {
        StatItemIncrement[] statItems = 
        {
            new StatItemIncrement { statCode = StatCode, inc = value }
        };

        AccelBytePlugin.GetStatistic().IncrementUserStatItems(statItems, result => 
        {
            if (result.IsError)
            {
                Debug.Log($"Failed to update user stat item: error code: {result.Error.Code} message: {result.Error.Message}");
            }
            else
            {
                Debug.Log("Update stat item is successful");
                
                // loop the result data
                for (int index = 0; index < result.Value.Length; index++)
                {
                    // current result data
                    StatItemOperationResult currentResult = result.Value[index];

                    if (currentResult.statCode == StatCode)
                    {
                        // get the Current Updated Stat Item Value from details
                        Dictionary<string, object> detailsDictionary = JsonExtension.ToObject<Dictionary<string, object>>(currentResult.details.ToJsonString());
                        int currentStatValue = Convert.ToInt32(detailsDictionary["currentValue"]);

                        // count the old value of Stat Item before increment
                        int oldStatValue = currentStatValue - (int)statItems[index].inc;

                        // check if Huge Spender's Achievement is unlocked
                        if (currentStatValue >= 500 && oldStatValue < 500)
                        {
                            // call the achievement unlocked's pop up notification
                            const string achievementCode = "spend-500-vc";
                            PopUpNotificationPanel.Instance.CreateAchievementPopUp(achievementCode);
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            }
        });
    }
}
