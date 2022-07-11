// Copyright (c) 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardHandler : MonoBehaviour
{
    public GameObject LeaderboardWindow;

    [SerializeField]
    private Transform leaderboardListPanel;
    [SerializeField]
    private Transform contentPanel;

    [SerializeField]
    private Button backToMainMenuButton;

    [SerializeField]
    private Button leaderboardButtonPrefab;

    #region Leaderboard Content 
    [Header("Leaderboard Content")]
    [SerializeField]
    private Text subHeaderText;

    [SerializeField]
    private Button allTimeButton;
    [SerializeField]
    private Button seasonalButton;
    [SerializeField]
    private Button weeklyButton;
    [SerializeField]
    private Button dailyButton;

    [SerializeField]
    private Transform rankingTablePanel;
    [SerializeField]
    private Transform categoryTablePanel;
    [SerializeField]
    private GameObject userRankingPrefab;

    [SerializeField]
    private Transform userRankingDisplayPlaceholder;
    #endregion

    private LeaderboardData currentLeaderboardData = null;

    private int leaderboardOffset = 0;
    private int leaderboardLimit = 10;

    private enum LeaderboardMode
    {
        Menu,
        Content
    }

    private LeaderboardMode DisplayMode
    {
        get => DisplayMode;
        set
        {
            switch (value)
            {
                case LeaderboardMode.Menu:
                    leaderboardListPanel.gameObject.SetActive(true);
                    contentPanel.gameObject.SetActive(false);
                    GetLeaderboardList();
                    break;
                case LeaderboardMode.Content:
                    contentPanel.gameObject.SetActive(true);
                    leaderboardListPanel.gameObject.SetActive(false);
                    LeaderboardType = LeaderboardTimeFrame.ALL_TIME;
                    break;
            }
        }
    }

    private LeaderboardTimeFrame LeaderboardType
    {
        get => LeaderboardType;
        set
        {
            switch (value)
            {
                case LeaderboardTimeFrame.ALL_TIME:
                    subHeaderText.text = currentLeaderboardData.name + " - All Time";
                    GetRanking(currentLeaderboardData.leaderboardCode, LeaderboardTimeFrame.ALL_TIME, leaderboardOffset, leaderboardLimit);
                    break;
                case LeaderboardTimeFrame.CURRENT_SEASON:
                    subHeaderText.text = currentLeaderboardData.name + " - Seasonal";
                    GetRanking(currentLeaderboardData.leaderboardCode, LeaderboardTimeFrame.CURRENT_SEASON, leaderboardOffset, leaderboardLimit);
                    break;
                case LeaderboardTimeFrame.CURRENT_WEEK:
                    subHeaderText.text = currentLeaderboardData.name + " - Weekly";
                    GetRanking(currentLeaderboardData.leaderboardCode, LeaderboardTimeFrame.CURRENT_WEEK, leaderboardOffset, leaderboardLimit);
                    break;
                case LeaderboardTimeFrame.TODAY:
                    subHeaderText.text = currentLeaderboardData.name + " - Daily";
                    GetRanking(currentLeaderboardData.leaderboardCode, LeaderboardTimeFrame.TODAY, leaderboardOffset, leaderboardLimit);
                    break;
            }
        }
    }

    /// <summary>
    /// Setup Leaderboard UI and prepare state
    /// </summary>
    public void Setup()
    {
        DisplayMode = LeaderboardMode.Menu;

        userRankingDisplayPlaceholder.gameObject.SetActive(true);
        backToMainMenuButton.onClick.AddListener(() =>
        {
            LeaderboardWindow.SetActive(false);
            GetComponent<MenuHandler>().Menu.gameObject.SetActive(true);
        });

        // reset listeners, so it won't triggered more than once
        allTimeButton.onClick.RemoveAllListeners();
        seasonalButton.onClick.RemoveAllListeners();
        weeklyButton.onClick.RemoveAllListeners();
        dailyButton.onClick.RemoveAllListeners();

        allTimeButton.onClick.AddListener(() => LeaderboardType = LeaderboardTimeFrame.ALL_TIME);
        seasonalButton.onClick.AddListener(() => LeaderboardType = LeaderboardTimeFrame.CURRENT_SEASON);
        weeklyButton.onClick.AddListener(() => LeaderboardType = LeaderboardTimeFrame.CURRENT_WEEK);
        dailyButton.onClick.AddListener(() => LeaderboardType = LeaderboardTimeFrame.TODAY);
    }

    /// <summary>
    /// Get Leaderboard List, then create and setup the menu buttons based on the result
    /// </summary>
    private void GetLeaderboardList()
    {
        AccelBytePlugin.GetLeaderboard().GetLeaderboardList(result =>
        {
            if (result.IsError)
            {
                Debug.Log($"Error GetLeaderboardList, Error Code: {result.Error.Code} Error Message: {result.Error.Message}");
            }
            else
            {
                Debug.Log("Success to get Leaderboard List");

                LoopThroughTransformAndDestroy(leaderboardListPanel);

                foreach (LeaderboardData leaderboardData in result.Value.data)
                {
                    Button leaderboardButton = Instantiate(leaderboardButtonPrefab, leaderboardListPanel);
                    leaderboardButton.gameObject.GetComponentInChildren<Text>().text = leaderboardData.name;
                    leaderboardButton.onClick.AddListener(() => 
                    {
                        currentLeaderboardData = leaderboardData;
                        DisplayMode = LeaderboardMode.Content;
                    });
                }
            }
        });
    }

    /// <summary>
    /// Get all ranking for specific leaderboard (leaderboardCode) based on leaderboard's time frame type
    /// </summary>
    /// <param name="leaderboardCode">leaderboard code of the category (statcode) that registered on admin portal</param>
    /// <param name="timeFrame">leaderboard's time type</param>
    /// <param name="offset">start point of the query data</param>
    /// <param name="limit">limit of the result per paging</param>
    private void GetRanking(string leaderboardCode, LeaderboardTimeFrame timeFrame, int offset, int limit)
    {
        // Clean and reset the last leaderboard
        LoopThroughTransformAndDestroy(rankingTablePanel, categoryTablePanel, userRankingDisplayPlaceholder);

        AccelBytePlugin.GetLeaderboard().GetRankings(leaderboardCode, timeFrame, offset, limit, result =>
        {
            if (result.IsError)
            {
                // Do something if GetRankings has an error
                Debug.Log($"Error GetRankings, Error Code: {result.Error.Code} Error Message: {result.Error.Message}");

                // Reset the leaderboard in-case triggered more than once and Display Placeholder Text
                //LoopThroughTransformAndDestroy(rankingTablePanel, categoryTablePanel, userRankingDisplayPlaceholder);
                userRankingDisplayPlaceholder.gameObject.SetActive(true);
            }
            else
            {
                // Do something if GetRankings has been successful
                Debug.Log($"Success to get ranking for {leaderboardCode} leaderboard");

                // Store all ranking's userIds in a list of string
                List<string> userIds = new List<string>();
                foreach (UserPoint pointData in result.Value.data)
                {
                    userIds.Add(pointData.userId);
                }

                // Add currentUserId to the list to also get its userInfo 
                string currentUserId = AccelBytePlugin.GetUser().Session.UserId;
                userIds.Add(currentUserId);

                // Get UserInfos from the ranking list userIds
                AccelBytePlugin.GetUser().BulkGetUserInfo(userIds.ToArray(), userInfosResult =>
                {
                    if (result.IsError)
                    {
                        Debug.Log($"Failed to get party member's data: error code: {userInfosResult.Error.Code} message: {userInfosResult.Error.Message}");
                    }
                    else
                    {
                        // Reset the leaderboard in-case triggered more than once and Hide Placeholder Text
                        LoopThroughTransformAndDestroy(rankingTablePanel, categoryTablePanel, userRankingDisplayPlaceholder);
                        userRankingDisplayPlaceholder.gameObject.SetActive(false);

                        int currentUserRank = 0;

                        for (int i = 0; i < result.Value.data.Length; i++)
                        {
                            UserPoint userPoint = result.Value.data[i];
                            int rank = i + 1;

                            // find UserInfo's index based on UserPoint's userId and save the UserInfo data
                            int userInfoIndex = Array.FindIndex(userInfosResult.Value.data, info => info.userId == userPoint.userId);
                            BaseUserInfo userInfo = userInfosResult.Value.data[userInfoIndex];

                            // Create UserRankingDisplay Panel and update UserRankingDisplay's text data with the current ranking information
                            UserRankingDisplayPanel userRankingDisplay = Instantiate(userRankingPrefab, rankingTablePanel).GetComponent<UserRankingDisplayPanel>();
                            userRankingDisplay.SetRankingText(rank, userInfo.displayName, userPoint.point);

                            // if currentUserId is the owner of the current rank, change the frame color and save the rank data
                            if (currentUserId == userPoint.userId)
                            {
                                userRankingDisplay.SetRankingFrameColor(Color.cyan);
                                currentUserRank = rank;
                            }
                            // if the rank is an odd number, change the frame color
                            else if (i % 2 != 0)
                            {
                                userRankingDisplay.SetRankingFrameColor(Color.white);
                            }

                            // if it's the last rank before <limit> and the current userid's rank is not in the top <limit> rank list
                            if (i == limit - 1 && currentUserRank == 0)
                            {
                                // find the current UserInfo's index based on currentUserId and save the UserInfo data
                                int currentUserInfoIndex = Array.FindIndex(userInfosResult.Value.data, info => info.userId == currentUserId);
                                BaseUserInfo currentUserInfo = userInfosResult.Value.data[currentUserInfoIndex];

                                GetUserRanking(userRankingDisplay, currentUserInfo, leaderboardCode, timeFrame);
                            }
                        }
                    }
                });
            }
        });
    }

    /// <summary>
    /// Get specific user ranking for specific leaderboard (leaderboardCode)
    /// </summary>
    /// <param name="userRankingDisplay">UserRankingDisplay Prefab that has instantiated</param>
    /// <param name="userInfo">current user's UserInfo data</param>
    /// <param name="leaderboardCode">leaderboard code of the category</param>
    /// <param name="timeFrame">current leaderboard's time frame</param>
    private void GetUserRanking(UserRankingDisplayPanel userRankingDisplay, BaseUserInfo userInfo, string leaderboardCode, LeaderboardTimeFrame timeFrame)
    {
        AccelBytePlugin.GetLeaderboard().GetUserRanking(userInfo.userId, leaderboardCode, result =>
        {
            if (result.IsError)
            {
                // Do something if GetUserRanking has an error
                Debug.Log($"Error GetUserRanking, Error Code: {result.Error.Code} Error Message: {result.Error.Message}");
            }
            else
            {
                // Do something if GetUserRanking has been successful
                Debug.Log($"Success to get user ranking in {leaderboardCode} leaderboard");

                // get UserRanking data based on the required TimeFrame
                UserRanking userRanking = null;
                switch (timeFrame)
                {
                    case LeaderboardTimeFrame.ALL_TIME:
                        userRanking = result.Value.allTime;
                        break;
                    case LeaderboardTimeFrame.CURRENT_SEASON:
                        userRanking = result.Value.current;
                        break;
                    case LeaderboardTimeFrame.CURRENT_WEEK:
                        userRanking = result.Value.weekly;
                        break;
                    case LeaderboardTimeFrame.TODAY:
                        userRanking = result.Value.daily;
                        break;
                }

                // update UserRankingDisplay Data if the prefab is not destroyed or the rank is not 0
                if (userRankingDisplay != null && userRanking.rank != 0)
                {
                    userRankingDisplay.SetRankingText(userRanking.rank, userInfo.displayName, userRanking.point);
                    userRankingDisplay.SetRankingFrameColor(Color.cyan);
                }
            }
        });
    }

    /// <summary>
    /// A utility function to Destroy all Children of the parent transform. Optionally do not remove a specific Transform
    /// </summary>
    /// <param name="parent">Parent Object to destroy children</param>
    /// <param name="doNotRemove">Optional specified Transform that should NOT be destroyed</param>
    /// <param name="additionalDoNotRemove"></param>
    private static void LoopThroughTransformAndDestroy(Transform parent, Transform doNotRemove = null, Transform additionalDoNotRemove = null)
    {
        //Loop through all the children and add them to a List to then be deleted
        List<GameObject> toBeDeleted = new List<GameObject>();
        foreach (Transform t in parent)
        {
            //except the Do Not Remove transform if there is one
            if (t != doNotRemove && t != additionalDoNotRemove)
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
