// Copyright (c) 2021 - 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AccelByte.Api;
using Mirror;
using AccelByte.Core;

public class GameplayManager : NetworkBehaviour
{
    [SerializeField]
    private GameplayInterface gameCanvas;

    private int totalPlayersConnected = 0;
    private int totalPlayersStop = 0;
    private int totalPlayers = 0;

    private bool isLocal = false;

    private double targetTime;
    private double mainTime;

    internal static readonly Dictionary<NetworkConnection, PlayerInfo> playerInfos = new Dictionary<NetworkConnection, PlayerInfo>();

    /// <summary>
    /// Called by server to login by credentials
    /// </summary>
    /// <param name="port"></param>
    internal void OnAccelByteServerStarted(int port)
    {
        // Get the local command line argument for the local test
        isLocal = ConnectionHandler.GetLocalArgument();

        ArmadaHandler.LoginServer(port, isLocal);
    }

    /// <summary>
    /// Called on Start Server
    /// </summary>
    internal void OnServerStarted()
    {
        if (!NetworkServer.active) return;

        NetworkServer.RegisterHandler<ServerStartClientMessage>(OnServerStartClient);
        NetworkServer.RegisterHandler<ServerRequestStopTimerMessage>(OnServerStopTimerMessage);
    }

    /// <summary>
    /// Called on Client connect to server
    /// </summary>
    internal void OnPlayerStarted()
    {
        if (!NetworkClient.active) return;

        NetworkClient.RegisterHandler<ClientStartClientResponseMessage>(OnStartClientResponse);
        NetworkClient.RegisterHandler<ClientUpdateCountdownTimerMessage>(OnUpdateCountdownTime);
        NetworkClient.RegisterHandler<ClientChangeToGameplayStateMessage>(OnChangeToGameplayState);
        NetworkClient.RegisterHandler<ClientUpdateMainTimeMessage>(OnUpdateMainTime);
        NetworkClient.RegisterHandler<ClientOnAllPlayerStopTime>(OnAllClientStopTime);

        // Current user's userId and displayName
        User user = MultiRegistry.GetApiClient().GetApi<User, UserApi>();
        string userId = user.Session.UserId;
        string displayName = LobbyHandler.Instance.partyHandler.partyMembers[userId];

        NetworkClient.connection.Send(new ServerStartClientMessage { playerId = userId, displayName = displayName });

        // Set the user id inside the gameplay interface player id. this to check after gameplay ended, the interface will know where their current player information by matching the player id
        gameCanvas.playerId = userId;
    }

    /// <summary>
    /// Called on Client disconnect
    /// </summary>
    internal void OnPlayerDisconnected()
    {
        gameCanvas.ChangePanel(GameplayInterfaceState.None);
    } 

    /// <summary>
    /// Send message to server that player press the stop button
    /// </summary>
    public void RequestStopTime()
    {
        NetworkClient.connection.Send(new ServerRequestStopTimerMessage { });
    }

    /// <summary>
    /// Server set player's info
    /// </summary>
    /// <param name="conn"> player's network connection</param>
    /// <param name="msg"> message that contains player's info</param>
    void OnServerStartClient(NetworkConnection conn, ServerStartClientMessage msg)
    {
        playerInfos.Add(conn, new PlayerInfo { playerId = msg.playerId, displayName = msg.displayName });

        PlayerInfo playerInfo = playerInfos[conn];

        ArmadaHandler.GetPlayerInfo(result => 
        {
            if (result.IsError) return;

            bool isPartyA = true;
            bool foundPlayer = false;

            // Get total player from game mode in result
            totalPlayers = result.Value.game_mode.ToGameMode().GetTotalPlayers();

            // Check if the user exists and assign the party
            foreach (var ally in result.Value.matching_allies)
            {
                foreach (var party in ally.matching_parties)
                {
                    foreach (var user in party.party_members)
                    {
                        if (user.user_id == playerInfo.playerId)
                        {
                            playerInfo.isPartyA = isPartyA;

                            foundPlayer = true;
                            break;
                        }
                    }

                    if (foundPlayer) break;
                }

                if (foundPlayer) break;

                isPartyA = !isPartyA;
            }

            // Remove player info if the player is not registered in the current match
            if (!foundPlayer)
            {
                playerInfos.Remove(conn);
                return;
            }

            totalPlayersConnected++;

            Debug.Log($"Total player Connected : {totalPlayersConnected}/{totalPlayers}");

            // Update player infos dictionary
            playerInfos[conn] = playerInfo;

            Debug.Log(string.Format("Player {0} is joining in the party {1}", playerInfo.displayName, playerInfo.isPartyA ? "A" : "B"));

            // Start the game if total players connected and total players are same
            if (totalPlayersConnected == totalPlayers)
            {

                foreach (NetworkConnection connection in playerInfos.Keys)
                {
                    connection.Send(new ClientStartClientResponseMessage { });
                }
                if (isServer)
                {
                    StartCoroutine(CountdownTimer());
                }
            }
        });
    }

    /// <summary>
    /// Server set the player stop time
    /// </summary>
    /// <param name="conn"> player's network connection</param>
    /// <param name="msg"> server's message</param>
    void OnServerStopTimerMessage(NetworkConnection conn, ServerRequestStopTimerMessage msg)
    {
        totalPlayersStop++;

        PlayerInfo playerInfo = playerInfos[conn];
        playerInfo.playerScoreTime = mainTime;
        playerInfos[conn] = playerInfo;

        Debug.Log($"Total player Stop: {totalPlayersStop}/{totalPlayers}");

        if (totalPlayersStop == totalPlayers)
        {
            StartCoroutine(CloseServer());
            OnServerStopGameplay();
        }
    }

    /// <summary>
    /// Server finish the round since all players have pressed the stop button
    /// </summary>
    void OnServerStopGameplay()
    {
        StopCoroutine("StopWatch");

        List<NetworkConnection> keysToUpdate = new List<NetworkConnection>();
        keysToUpdate.AddRange(playerInfos.Keys.ToArray());

        List<double> scores = new List<double>();
        for (int i = 0; i < keysToUpdate.Count; i++)
        {
            scores.Add(Mathf.Abs((float)(targetTime - playerInfos.Values.ToArray()[i].playerScoreTime)));
        }

        double currentHigherScore = 99999999.0f; // in this case the lower value is the winner
        for (int i = 0; i < scores.Count; i++)
        {
            if (scores[i] < currentHigherScore)
            {
                currentHigherScore = scores[i];
            }
        }

        int highscoreIndex = scores.FindIndex(x => x == currentHigherScore);

        for (int i = 0; i < keysToUpdate.Count; i++)
        {
            PlayerInfo playerInformation = playerInfos[keysToUpdate[i]];
            if (playerInformation.isPartyA == playerInfos[keysToUpdate[highscoreIndex]].isPartyA)
            {
                playerInformation.isWin = true;
            }
            else
            {
                playerInformation.isWin = false;
            }
            playerInfos[keysToUpdate[i]] = playerInformation;

        }

        List<string> userIds = new List<string>();
        List<bool> isWins = new List<bool>();
        for (int i = 0; i < keysToUpdate.Count; i++)
        {
            userIds.Add(playerInfos[keysToUpdate[i]].playerId);
            isWins.Add(playerInfos[keysToUpdate[i]].isWin);
        }
        ArmadaHandler.UpdateUserStatistic(userIds.ToArray(), isWins.ToArray(), successAchievementsCode => 
        {
            foreach (NetworkConnection connection in playerInfos.Keys)
            { 
                // prepare unlocked achievemets code list
                List<string> achievementsCodeList = new List<string>();
                string currentPlayerId = playerInfos[connection].playerId;
                if (successAchievementsCode.ContainsKey(currentPlayerId))
                {
                    achievementsCodeList = successAchievementsCode[currentPlayerId];
                }

                connection.Send(new ClientOnAllPlayerStopTime { allPlayerInfos = playerInfos.Values.ToArray(), targetTime = targetTime, unlockedAchievementsCodeList = achievementsCodeList });
            }
        });
    }


    /// <summary>
    /// Coroutine: Update loading countdown from 3 to 0
    /// </summary>
    /// <returns> wait for 1 second</returns>
    IEnumerator CountdownTimer()
    {
        for (int countdown = 3; countdown >= 0; countdown--)
        {
            foreach (NetworkConnection connection in playerInfos.Keys)
            {
                if (isServer)
                {
                    connection.Send(new ClientUpdateCountdownTimerMessage { time = countdown });
                }
            }

            yield return new WaitForSeconds(1.0f);

            if (countdown == 0)
            {
                // Set target time

                // random target time with range a to b seconds
                targetTime = Random.Range(3.0f, 9.0f);

                // send targetTime value to all client
                foreach (NetworkConnection connection in playerInfos.Keys)
                {
                    connection.Send(new ClientChangeToGameplayStateMessage { targetTime = targetTime });
                }

                StartCoroutine(MainTime());
            }
        }
    }

    /// <summary>
    /// Coroutine: Update current running mainTime
    /// </summary>
    /// <returns></returns>
    IEnumerator MainTime()
    {
        while (true)
        {
            mainTime += Time.deltaTime;

            foreach (NetworkConnection connection in playerInfos.Keys)
            {
                connection.Send(new ClientUpdateMainTimeMessage { mainTime = mainTime });
            }

            yield return null;
        }
    }

    /// <summary>
    /// Unregister server and close the server automatically after the time is timeout
    /// </summary>
    /// <param name="timeout"></param>
    /// <returns></returns>
    IEnumerator CloseServer(int timeout = 30)
    {
        Debug.Log("Start countdown to close server");

        for (int i = 0; i < timeout; i++)
        {
            yield return new WaitForSeconds(1.0f);
        }

        ArmadaHandler.UnregisterServer(isLocal);
    }

    /// <summary>
    /// On client start, change panel to ReadyPanel
    /// </summary>
    /// <param name="msg"> client's message</param>
    void OnStartClientResponse(ClientStartClientResponseMessage msg)
    {
        gameCanvas.ChangePanel(GameplayInterfaceState.Loading);
    }

    /// <summary>
    /// On loading countdown, update LoadingPanel's UI
    /// </summary>
    /// <param name="msg"></param>
    void OnUpdateCountdownTime(ClientUpdateCountdownTimerMessage msg)
    {
        gameCanvas.UpdateLoadingPanelUI(msg.time);
    }

    /// <summary>
    /// Change panel to GameplayPanel and start the game
    /// </summary>
    /// <param name="msg"></param>
    void OnChangeToGameplayState(ClientChangeToGameplayStateMessage msg)
    {
        gameCanvas.ChangePanel(GameplayInterfaceState.Gameplay);
        gameCanvas.UpdateTargetTimeUI(msg.targetTime);
    }

    /// <summary>
    /// On current mainTime update, update mainTime to its UI
    /// </summary>
    /// <param name="msg"></param>
    void OnUpdateMainTime(ClientUpdateMainTimeMessage msg)
    {
        gameCanvas.UpdateMainTimeUI(msg.mainTime);
    }

    /// <summary>
    /// On all players have pressed the stop button, finish the game
    /// </summary>
    /// <param name="msg"></param>
    void OnAllClientStopTime(ClientOnAllPlayerStopTime msg)
    {
        gameCanvas.ChangePanel(GameplayInterfaceState.Result);
        gameCanvas.UpdateResultPanelUI(msg.allPlayerInfos, msg.targetTime, msg.unlockedAchievementsCodeList);
    }

    private void OnApplicationQuit()
    {
#if UNITY_SERVER
        ArmadaHandler.UnregisterServer(isLocal);
#endif
    }
}