// Copyright (c) 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserRankingDisplayPanel : MonoBehaviour
{
    [SerializeField]
    private Image rankPanel;
    [SerializeField]
    private Image displayNamePanel;
    [SerializeField]
    private Image scorePanel;

    [SerializeField]
    private Text rankText;
    [SerializeField]
    private Text displayNameText;
    [SerializeField]
    private Text scoreText;


    public void SetRankingText(int rank, string displayName, float point)
    {
        rankText.text = rank.ToString();
        displayNameText.text = displayName;
        scoreText.text = point.ToString();
    }

    public void SetRankingFrameColor(Color color)
    {
        rankPanel.color = color;
        displayNamePanel.color = color;
        scorePanel.color = color;
    }
}
