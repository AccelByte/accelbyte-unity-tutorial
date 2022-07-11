// Copyright (c) 2021 - 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using UnityEngine;
using UnityEngine.UI;

public class LogMessagePanel : MonoBehaviour
{
    [SerializeField]
    private Text messageText;


    /// <summary>
    /// Update Notification Message's UI 
    /// </summary>
    /// <param name="text"> text that will be shown in the party notification</param>
    public void UpdateNotificationUI(string text, Color color)
    {
        messageText.text = text;
        messageText.color = color;
    }
}
