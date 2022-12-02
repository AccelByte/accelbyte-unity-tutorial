// Copyright (c) 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using UnityEngine;
using AccelByte.Server;
using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using System.Collections.Generic;

public static class ArmadaHandler
{
    private static readonly string[] StatCodes = { "match", "win" };

    private static ServerApiClient serverApiClient;
    private static DedicatedServerManager dedicatedServerManager;
    private static ServerMatchmaking serverMatchmaking;
    private static ServerStatistic serverStatistic;

    private static void Main()
    {
        serverApiClient = MultiRegistry.GetServerApiClient();
        dedicatedServerManager = serverApiClient.GetServerApi<DedicatedServerManager, DedicatedServerManagerApi>();
        serverMatchmaking = serverApiClient.GetServerApi<ServerMatchmaking, ServerMatchmakingApi>();
        serverStatistic = serverApiClient.GetServerApi<ServerStatistic, ServerStatisticApi>();
    }

    /// <summary>
    /// Server login with the server client credentials and register DS to DSM
    /// </summary>
    /// <param name="port"> </param>
    /// <param name="isLocal"></param>
    public static void LoginServer(int port, bool isLocal)
    {
        // Initialize the API if it is not yet.
        if (serverApiClient == null)
        {
            Main();
        }

        // Login to server
        AccelByteServerPlugin.GetDedicatedServer().LoginWithClientCredentials(result =>
        {
            if (result.IsError)
            {
                // If we error, grab the Error Code and Message to print in the Log
                Debug.Log($"Server login failed : {result.Error.Code}: {result.Error.Message}");
            }
            else
            {
                Debug.Log("Server login successful");

                if (!isLocal)
                {
                    // Register Server to DSM
                    dedicatedServerManager.RegisterServer(port, registerResult =>
                    {
                        if (registerResult.IsError)
                        {
                            Debug.Log("Register Server to DSM failed");
                        }
                        else
                        {
                            Debug.Log("Register Server to DSM successful");
                        }
                    });
                }
                else
                {
                    string ip = "127.0.0.1";
                    string name = $"localds-{DeviceProvider.GetFromSystemInfo().DeviceId}";
                    uint portNumber = Convert.ToUInt32(port);

                    // Register Local Server to DSM
                    dedicatedServerManager.RegisterLocalServer(ip, portNumber, name, registerResult =>
                    {
                        if (registerResult.IsError)
                        {
                            Debug.Log("Register Local Server to DSM failed");
                        }
                        else
                        {
                            Debug.Log("Register Local Server to DSM successful");
                        }
                    });
                }
            }
        });
    }

    /// <summary>
    /// Unregister DS from DSM and quit the app
    /// </summary>
    /// <param name="isLocal"> Unregister local DS if the value is true</param>
    public static void UnregisterServer(bool isLocal)
    {
        if (isLocal)
        {
            // Deregister Local Server to DSM
            dedicatedServerManager.DeregisterLocalServer(result => 
            {
                if (result.IsError)
                {
                    Debug.Log("Failed Deregister Local Server");
                }
                else
                {
                    Debug.Log("Successfully Deregister Local Server");

                    Application.Quit();
                }
            });
        }
        else
        {
            // Shutdown Server to DSM
            dedicatedServerManager.ShutdownServer(true, result => 
            {
                if (result.IsError)
                {
                    Debug.Log("Failed Shutdown Server");
                }
                else
                {
                    Debug.Log("Successfully Shutdown Server");

                    Application.Quit();
                }
            });
        }
    }

    /// <summary>
    /// DS queries match info from Matchmaking (MM)
    /// </summary>
    /// <param name="callback"> Return match info callback</param>
    public static void GetPlayerInfo(ResultCallback<MatchmakingResult> callback)
    {
        // Get session id/ match id from DSM
        dedicatedServerManager.GetSessionId(dsmResult =>
        {
            if (dsmResult.IsError)
            {
                Debug.Log("Failed Get Session Id");

                callback.TryError(dsmResult.Error);
            }
            else
            {
                // Query Session Status to get match info from Matchmaking
                serverMatchmaking.QuerySessionStatus(dsmResult.Value.session_id, queryResult =>
                {
                    if (queryResult.IsError)
                    {
                        Debug.Log("Failed Query Session Status");

                        callback.TryError(queryResult.Error);
                    }
                    else
                    {
                        // Return error if status is not matched
                        if (queryResult.Value.status != AccelByte.Models.MatchmakingStatus.matched)
                        {
                            Debug.Log("Matchmaking status is not matched");

                            // Return error callback
                            callback.TryError(queryResult.Error);
                        }

                        // Return success callback
                        callback.TryOk(queryResult.Value);
                    }
                });
            }
        });
    }

    /// <summary>
    /// Update users' match and win stat item
    /// </summary>
    /// <param name="userIds"> The list of user id that will be updated.</param>
    /// <param name="isWins"> The list of win results to add win stat item on that user.</param>
    public static void UpdateUserStatistic(string[] userIds, bool[] isWins, Action<Dictionary<string, List<string>>> successCallback)
    {
        List<UserStatItemIncrement> userStatItemList = new List<UserStatItemIncrement>();

        for(int i = 0; i < userIds.Length; i++)
        {
            UserStatItemIncrement userStatItem = new UserStatItemIncrement()
            {
                statCode = StatCodes[0],
                userId = userIds[i],
                inc = 1
            };

            userStatItemList.Add(userStatItem);

            // Skip if user is not winning the game
            if (!isWins[i]) continue;

            userStatItem = new UserStatItemIncrement()
            {
                statCode = StatCodes[1],
                userId = userIds[i],
                inc = 1
            };

            userStatItemList.Add(userStatItem);
        }

        // Update the data to statistic
        serverStatistic.IncrementManyUsersStatItems(userStatItemList.ToArray(), result => 
        {
            if (result.IsError)
            {
                Debug.Log($"Failed to update user stat items: error code: {result.Error.Code} message: {result.Error.Message}");
            }
            else
            {
                Debug.Log("Update stat item is successful");

                // prepare callback return value consists of userId (key) and list of unlocked achievements' code (value)
                Dictionary<string, List<string>> unlockedAchievementsDictionary = new Dictionary<string, List<string>>();

                // loop the result data
                for (int index = 0; index < result.Value.Length; index++)
                {
                    // current result data
                    StatItemOperationResult currentResult = result.Value[index];
                    UserStatItemIncrement currentStatItem = userStatItemList[index];

                    // prepare list for dictionary's value
                    List<string> achievementsCodeList = new List<string>();
                    if (unlockedAchievementsDictionary.ContainsKey(currentStatItem.userId))
                    {
                        // get current achievements code list
                        achievementsCodeList = unlockedAchievementsDictionary[currentStatItem.userId];
                    }
                    else
                    {
                        // initialize dictionary value
                        unlockedAchievementsDictionary.Add(currentStatItem.userId, achievementsCodeList);
                    }

                    // get the Current Updated Stat Item Value from details
                    Dictionary<string, object> detailsDictionary = JsonExtension.ToObject<Dictionary<string, object>>(currentResult.details.ToJsonString());
                    int currentStatValue = Convert.ToInt32(detailsDictionary["currentValue"]);
                    // count the old value of Stat Item before increment
                    int oldStatValue = currentStatValue - (int)currentStatItem.inc;

                    if (currentResult.statCode == "win" && currentStatValue >= 1)
                    {
                        // call the achievement unlocked's pop up notification
                        const string achievementCode = "win-first-time";
                        achievementsCodeList.Add(achievementCode);
                    }

                    if (currentResult.statCode == "match")
                    {
                        if (currentStatValue >= 5 && oldStatValue < 5)
                        {
                            const string achievementCode = "match-two-times";
                            achievementsCodeList.Add(achievementCode);
                        }

                        if (currentStatValue >= 2 && oldStatValue < 2)
                        {
                            const string achievementCode = "match-5-times";
                            achievementsCodeList.Add(achievementCode);
                        }
                    }

                    // update dictionary value 
                    unlockedAchievementsDictionary[currentStatItem.userId] = achievementsCodeList;
                }

                successCallback.Invoke(unlockedAchievementsDictionary);
            }
        });
    }
}
