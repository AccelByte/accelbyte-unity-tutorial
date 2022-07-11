// Copyright (c) 2021 - 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System;
using System.Collections.Generic;

public class GameplayInterface : NetworkBehaviour
{
    [SerializeField]
    private GameplayManager gameplayManager;

    [Header("Panel Object")]
    // all canvas panel object - this for switching the interface
    [SerializeField]
    private GameObject readyPanel;
    [SerializeField]
    private GameObject loadingPanel;
    [SerializeField]
    private GameObject gameplayPanel;
    [SerializeField]
    private GameObject resultPanel;

    [Header("Loading")]
    // countdown state
    [SerializeField]
    private Text countdownTimer;

    [Header("Gameplay")]
    // gameplay state
    public Text stopWatch;
    public Button stopButton;
    public Text currentTargetTimer;
    
    [Header("Result")]
    // win or lose state
    public Text winnerText;
    public Text gameTimer;
    public Text[] playerResultInfos;
    public Text[] playerResultTimes;
    public Button exitButton;

    public string playerId;

    /// <summary>
    /// Update Panel State and add event listener
    /// </summary>
    /// <param name="State"> current Panel State</param>
    public void ChangePanel(GameplayInterfaceState State)
    {
        switch (State)
        {
            case GameplayInterfaceState.Loading:
                readyPanel.SetActive(false);
                loadingPanel.SetActive(true);
                gameplayPanel.SetActive(false);
                resultPanel.SetActive(false);
                break;
            
            case GameplayInterfaceState.Gameplay:
                stopButton.interactable = true;
                stopButton.onClick.AddListener(() =>
                {
                    gameplayManager.RequestStopTime();
                    stopButton.interactable = false;
                });

                readyPanel.SetActive(false);
                loadingPanel.SetActive(false);
                gameplayPanel.SetActive(true);
                resultPanel.SetActive(false);
                break;

            case GameplayInterfaceState.Result:
                exitButton.onClick.AddListener(() =>
                {
                    NetworkManager.singleton.StopClient();
                    SceneManagementHandler.ChangeScene(SceneManagementHandler.SceneName.SampleScene);
                });

                readyPanel.SetActive(false);
                loadingPanel.SetActive(false);
                gameplayPanel.SetActive(false);
                resultPanel.SetActive(true);
                break;

            case GameplayInterfaceState.None:
                SceneManagementHandler.ChangeScene(SceneManagementHandler.SceneName.SampleScene);
                break;
        }
    }

    /// <summary>
    /// Update Loading Panel's UI
    /// </summary>
    /// <param name="countdownTime"> countdown time to indicate loading state</param>
    public void UpdateLoadingPanelUI(int countdownTime)
    {
        // check countdown time
        if (countdownTime == 0)
        {
            countdownTimer.text = "GO !!";
        }
        else
        {
            countdownTimer.text = countdownTime.ToString();
        }
    }

    /// <summary>
    /// Update targetTime value to UI
    /// </summary>
    /// <param name="targetTime"> the target time for this game round</param>
    public void UpdateTargetTimeUI(double targetTime)
    {
        currentTargetTimer.text = TimeSpan.FromSeconds(targetTime).ToString("mm':'ss':'ff");
    }

    /// <summary>
    /// Update mainTime value to UI
    /// </summary>
    /// <param name="mainTime"> the current running mainTime</param>
    public void UpdateMainTimeUI(double mainTime)
    {
        stopWatch.text = TimeSpan.FromSeconds(mainTime).ToString("mm':'ss':'ff");
    }

    /// <summary>
    /// Update Result Panel's UI
    /// </summary>
    /// <param name="allPlayerInfos"> list of all players' game data</param>
    /// <param name="targetTime"> target time of the current game</param>
    public void UpdateResultPanelUI(PlayerInfo[] allPlayerInfos, double targetTime, List<string> unlockedAchievementsCode)
    {
        HideUnnecessaryPanelUI(allPlayerInfos.Length);

        gameTimer.text = TimeSpan.FromSeconds(targetTime).ToString("mm':'ss':'ff");

        bool isMyPartyA = false;
        foreach (var player in allPlayerInfos)
        {
            if (player.playerId == playerId)
            {
                isMyPartyA = player.isPartyA;
            }
        }

        int count = 1;
        foreach (var player in allPlayerInfos)
        {
            if (player.isPartyA == isMyPartyA)
            {
                if (player.playerId == playerId)
                {
                    playerResultInfos[0].text = player.displayName + "\n" + player.playerId;
                    playerResultTimes[0].text = TimeSpan.FromSeconds(player.playerScoreTime).ToString("mm':'ss':'ff");

                    winnerText.text = (player.isWin) ? "YOU WIN !!" : "YOU LOSE !!";
                }
                else
                {
                    playerResultInfos[2].text = player.displayName + "\n" + player.playerId;
                    playerResultTimes[2].text = TimeSpan.FromSeconds(player.playerScoreTime).ToString("mm':'ss':'ff");
                }
            }
            else
            {
                playerResultInfos[count].text = player.displayName + "\n" + player.playerId;
                playerResultTimes[count].text = TimeSpan.FromSeconds(player.playerScoreTime).ToString("mm':'ss':'ff");
                count += 2;
            }
        }

        // if players number in current's player party is more than 1
        if (allPlayerInfos.Length/2 > 1)
        {
            const string friendlyFaceAchievementCode = "finish-party-1";
            unlockedAchievementsCode.Add(friendlyFaceAchievementCode);
        }

        // loop the achievements' code list
        foreach (string achievementCode in unlockedAchievementsCode)
        {
            if (achievementCode == "finish-party-1" || achievementCode == "win-first-time")
            {
                PopUpNotificationPanel.Instance.CheckAchievementUnlockedStatus(achievementCode);
            }
            else
            {
                PopUpNotificationPanel.Instance.CreateAchievementPopUp(achievementCode);
            }
        }
    }

    /// <summary>
    /// Hide some of the panels for 1v1 game mode (max player is 2)
    /// </summary>
    /// <param name="totalPlayers"></param>
    private void HideUnnecessaryPanelUI(int totalPlayers)
    {
        // Don't hide panel UI if total max player is not 2
        if (totalPlayers != 2) return;

        for(int i = 2; i < playerResultInfos.Length; i++)
        {
            playerResultInfos[i].text = "";
            playerResultTimes[i].text = "";
        }
    }
}
