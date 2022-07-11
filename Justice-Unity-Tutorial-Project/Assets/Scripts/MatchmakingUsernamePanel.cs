// Copyright (c) 2021 - 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using UnityEngine;
using UnityEngine.UI;

public class MatchmakingUsernamePanel : MonoBehaviour
{
    [SerializeField]
    private Text UsernameText;
    [SerializeField]
    private Image UsernameFrameImage;

    public string GetUsernameText()
    {
        return UsernameText.text;
    }

    public void SetUsernameText(string text)
    {
        if (UsernameText == null) return;

        UsernameText.text = text;
    }

    public void SetUsernameFrameColor(Color color)
    {
        UsernameFrameImage.color = color;
    }
}
