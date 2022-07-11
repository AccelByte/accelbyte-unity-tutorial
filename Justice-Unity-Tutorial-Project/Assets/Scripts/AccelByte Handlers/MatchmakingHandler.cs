// Copyright (c) 2021 - 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using AccelByte.Api;
using AccelByte.Models;
using AccelByte.Core;
using UnityEngine;
using UnityEngine.UI;

using UI = UnityEngine.UI;
using System.Collections.Generic;

public class MatchmakingHandler : MonoBehaviour
{

    private const string DEFAULT_COUNTUP = "Time Elapsed: 00:00";
    // This default count down time must be similar with the Lobby Config in the Admin Portal
    private const string DEFAULT_COUNTDOWN = "20";  

    // Current selected game mode
    private GameMode gameMode;
    // Curent active match id
    private string matchId;

    private bool isInitialized = false;

    #region UI
    [SerializeField]
    private GameObject matchmakingWindow;
    [SerializeField]
    private GameObject findMatchWindow;
    [SerializeField]
    private GameObject readyMatchWindow;
    [SerializeField]
    private GameObject gameWindow;

    [SerializeField]
    private Dropdown gameModeDropdown;
    [SerializeField]
    private Dropdown regionDropdown;

    #region Button

    [SerializeField]
    private Button findMatchButton;
    [SerializeField]
    private Button readyButton;
    [SerializeField]
    private Button cancelButton;
    [SerializeField]
    private Button exitButton;
    #endregion

    [SerializeField]
    private Text countUpText;
    [SerializeField]
    private Text countDownText;

    [SerializeField]
    private UI.Image loadingImage;

    [SerializeField]
    private MatchmakingUsernamePanel[] usernameList;
    #endregion

    private enum MatchmakingWindows
    {
        Lobby,
        FindMatch,
        Matchmaking,
        Game
    }

    private MatchmakingWindows DisplayWindow
    {
        get => DisplayWindow;
        set
        {
            switch (value)
            {
                case MatchmakingWindows.Lobby:
                    matchmakingWindow.SetActive(false);
                    findMatchWindow.SetActive(false);
                    readyMatchWindow.SetActive(false);
                    gameWindow.SetActive(false);
                    break;

                case MatchmakingWindows.FindMatch:
                    ResetTimerText();

                    matchmakingWindow.SetActive(true);
                    findMatchWindow.SetActive(true);
                    readyMatchWindow.SetActive(false);
                    gameWindow.SetActive(false);
                    break;

                case MatchmakingWindows.Matchmaking:
                    ResetTimerText();
                    CleanAndPopulateUsernameText();

                    readyButton.interactable = true;

                    matchmakingWindow.SetActive(true);
                    findMatchWindow.SetActive(false);
                    readyMatchWindow.SetActive(true);
                    gameWindow.SetActive(false);
                    break;

                case MatchmakingWindows.Game:
                    matchmakingWindow.SetActive(true);
                    findMatchWindow.SetActive(false);
                    readyMatchWindow.SetActive(false);
                    gameWindow.SetActive(true);
                    break;

                default:
                    break;
            }
        }
    }

    /// <summary>
    /// Grab the current Party Handler
    /// </summary>
    /// <returns> Return the active party handler</returns>
    private PartyHandler GetPartyHandler()
    {
        return LobbyHandler.Instance.partyHandler;
    }

    /// <summary>
    /// Setup UI
    /// </summary>
    public void SetupMatchmaking()
    {
        if (isInitialized) return;
        isInitialized = true;

        // Setup button listener
        findMatchButton.onClick.AddListener(FindMatch);
        readyButton.onClick.AddListener(ReadyMatchmaking);
        cancelButton.onClick.AddListener(CancelMatchmaking);
        exitButton.onClick.AddListener(() => DisplayWindow = MatchmakingWindows.Lobby);
    }

    /// <summary>
    /// Check the game mode and party. After it passes, start matchmaking 
    /// </summary>
    private void FindMatch()
    {
        // Check if the party is already created,
        // game mode is not empty, and
        // party members are not exceeding from the selected game mode
        if (!ValidateGameModeAndParty())
        {
            return;
        }

        // Get the local command line argument for the local test
        bool isLocal = ConnectionHandler.GetLocalArgument();

        if (isLocal)
        {
            string serverName = $"localds-{DeviceProvider.GetFromSystemInfo().DeviceId}";
            AccelBytePlugin.GetLobby().StartMatchmaking(gameMode.GetString(), serverName, OnFindMatch);
        }
        else
        {
            // Check the region dropdown
            Dictionary<string, int> latencies = LobbyHandler.Instance.qosHandler.GetChoosenLatency();

            // Matchmaking using selected region
            AccelBytePlugin.GetLobby().StartMatchmaking(gameMode.GetString(), null, latencies, OnFindMatch);
        }
    }

    /// <summary>
    /// The result for the starting matchmaking
    /// </summary>
    /// <param name="result"> Contains data of matcmaking result</param>
    private void OnFindMatch(Result<MatchmakingCode> result)
    {
        //Check this is not an error
        if (result.IsError)
        {
            Debug.Log($"Unable to start matchmaking: error code: {result.Error.Code} message: {result.Error.Message}");
        }
        else
        {
            Debug.Log("Started matchmaking is successful");

            DisplayWindow = MatchmakingWindows.FindMatch;
        }
    }

    /// <summary>
    /// Cancel find match and back to the Lobby
    /// </summary>
    private void CancelMatchmaking()
    {
        AccelBytePlugin.GetLobby().CancelMatchmaking(gameMode.GetString(), result =>
        {
            //Check this is not an error
            if (result.IsError)
            {
                Debug.Log($"Unable to cancel matchmaking: error code: {result.Error.Code} message: {result.Error.Message}");
            }
            else
            {
                Debug.Log("Canceled matchmaking is successful");

                DisplayWindow = MatchmakingWindows.Lobby;
            }
        });
    }

    /// <summary>
    /// Set ready to consent when getting the matchmaking
    /// </summary>
    private void ReadyMatchmaking()
    {
        AccelBytePlugin.GetLobby().ConfirmReadyForMatch(matchId, result => 
        {
            //Check this is not an error
            if (result.IsError)
            {
                Debug.Log($"Unable to ready for match: error code: {result.Error.Code} message: {result.Error.Message}");
            }
            else
            {
                Debug.Log("Ready for match is successful");

                readyButton.interactable = false;
            }
        });
    }

    #region Notification
    /// <summary>
    /// Called when party leader find a match, cancel match, or found a match
    /// </summary>
    /// <param name="result"> Contains data of status and match id</param>
    public void MatchmakingCompletedNotification(MatchmakingNotif result)
    {
        switch (result.status)
        {
            case "done":
                // Called when found a match
                matchId = result.matchId;
                Debug.Log($"Found a match. Match id: {matchId}");

                DisplayWindow = MatchmakingWindows.Matchmaking;
                break;

            case "start":
                Debug.Log("Start Matchmaking");

                DisplayWindow = MatchmakingWindows.FindMatch;
                break;

            case "cancel":
                Debug.Log("Matchmaking Canceled");

                DisplayWindow = MatchmakingWindows.Lobby;
                break;

            case "timeout":
                Debug.Log("Matchmaking timeout");

                DisplayWindow = MatchmakingWindows.Lobby;
                break;

            default:
                Debug.Log("Matchmaking error");

                DisplayWindow = MatchmakingWindows.Lobby;
                break;
        }
    }

    /// <summary>
    /// Called when there is a player who set ready to consent
    /// </summary>
    /// <param name="result"> Contains data of match id and user id</param>
    public void ReadyForMatchConfirmedNotification(ReadyForMatchConfirmation result)
    {
        // Display unknown player who ready in the current match into right panel
        if (!GetPartyHandler().partyMembers.ContainsKey(result.userId))
        {
            for (int i = GetPartyHandler().partyMembers.Count; i < usernameList.Length; i++)
            {
                if (string.IsNullOrEmpty(usernameList[i].GetUsernameText()))
                {
                    AccelBytePlugin.GetUser().GetUserByUserId(result.userId, getUserResult => 
                    {
                        usernameList[i].SetUsernameText(getUserResult.Value.displayName);
                    });

                    usernameList[i].SetUsernameFrameColor(Color.green);
                    break;
                }
            }
        }
        // Display party member who ready in the current match into left panel
        else
        {
            for (int i = 0; i < GetPartyHandler().partyMembers.Count; i++)
            {
                if (usernameList[i].GetUsernameText() == GetPartyHandler().partyMembers[result.userId])
                {
                    usernameList[i].SetUsernameFrameColor(Color.green);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Called when all player in the current match is already ready for consent
    /// </summary>
    /// <param name="result"> contains data of status, match id, ip, port, etc</param>
    public void DSUpdatedNotification(DsNotif result)
    {
        countDownText.enabled = false;
        ConnectionHandler.ip = result.ip;
        ConnectionHandler.port = result.port;

        if (result.status != "READY" && result.status != "BUSY") return;

        Debug.Log($"Game is started");

        // Remove lobby listener before changing the scene
        LobbyHandler.Instance.RemoveLobbyListeners();
        // Change gameplay scene
        SceneManagementHandler.ChangeScene(SceneManagementHandler.SceneName.WatchTime);
    }

    /// <summary>
    /// Called when matchmaking is canceled due to there are players who not ready to consent
    /// </summary>
    /// <param name="result"> contains data of ban duration </param>
    public void RematchmakingNotif(RematchmakingNotification result)
    {
        // Find another match if the ban duration is zero
        if (result.banDuration == 0)
        {
            Debug.Log($"Find another match");

            DisplayWindow = MatchmakingWindows.FindMatch;
            return;
        }

        // Display ban duration to party notification
        LobbyHandler.Instance.WriteLogMessage($"[Matchmaking] You must wait for {result.banDuration} s to start matchmaking", Color.red);

        DisplayWindow = MatchmakingWindows.Lobby;
    }
    #endregion

    /// <summary>
    /// Validate empty party, game mode, and party members count based on game mode
    /// </summary>
    /// <returns> Return true value if the validation is passed and vice versa</returns>
    private bool ValidateGameModeAndParty()
    {
        // Set game mode based on game mode selector dropdown
        gameMode = gameModeDropdown.options[gameModeDropdown.value].text.ToGameMode();

        // Avoid to choose default game mode
        if (gameModeDropdown.value == 0)
        {
            LobbyHandler.Instance.WriteLogMessage("[Matchmaking] Choose the game mode", Color.red);
            return false;
        }
        // Check if user is not in the party
        if (GetPartyHandler().partyMembers == null || GetPartyHandler().partyMembers?.Count == 0)
        {
            LobbyHandler.Instance.WriteLogMessage("[Matchmaking] You are not in the party", Color.red);
            return false;
        }
        // Avoid party members exceed from the game mode
        if (gameMode == GameMode.versusOne && GetPartyHandler().partyMembers?.Count == 1 ||
            gameMode == GameMode.versusTwo && GetPartyHandler().partyMembers?.Count <= 2)
        {
            return true;
        }

        LobbyHandler.Instance.WriteLogMessage("[Matchmaking] Party members are exceeding from the selected game mode", Color.red);
        return false;
    }

    /// <summary>
    /// Clean up the username text and reset frame color
    /// After that, populate current party members to the username text
    /// </summary>
    private void CleanAndPopulateUsernameText()
    {
        // Emptying username text in the matchmaking window
        foreach (var username in usernameList)
        {
            username.SetUsernameText("");
            username.SetUsernameFrameColor(Color.white);
        }

        // Populate party members
        int count = 0;
        foreach (var username in GetPartyHandler().partyMembers)
        {
            Debug.Log($"Populate party member with username: {username.Value}");

            usernameList[count].SetUsernameText(username.Value);
            count++;
        }
    }

    /// <summary>
    /// Reset the count down and count up into default value
    /// </summary>
    private void ResetTimerText()
    {
        countDownText.enabled = true;

        countDownText.text = DEFAULT_COUNTDOWN;
        countUpText.text = DEFAULT_COUNTUP;
    }
}
