// Copyright (c) 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using System;
using UnityEngine;

using Time = UnityEngine.Time;

public class PresenceHandler : MonoBehaviour
{
    // AccelByte's Multi Registry references
    private User user;
    private Lobby lobby;

    private const float maxIdleTime = 300f; // in seconds
    private float idleTime = 0;

    private bool isIdle = false;

    private void Start()
    {
        // AccelByte's Multi Registry initialization
        ApiClient apiClient = MultiRegistry.GetApiClient();
        user = apiClient.GetApi<User, UserApi>();
        lobby = apiClient.GetApi<Lobby, LobbyApi>();

        if (lobby.IsConnected)
        {
            // Set user presence to Online
            SetUserStatus(UserStatus.Online, "");
        }
    }

    private void FixedUpdate()
    {
        if (!user.Session.IsValid()) return;

        // Idle detector
        if (!Input.anyKey && Input.GetAxis("Mouse X") == 0 && Input.GetAxis("Mouse Y") == 0)
        {
            idleTime += Time.fixedDeltaTime;
        }
        else
        {
            // Reset idle time
            idleTime = 0;
        }

        // Set user's presence to away if user is idle
        if (!isIdle && idleTime > maxIdleTime)
        {
            isIdle = true;
            SetUserStatus(UserStatus.Online, "Away");
        }
        // Set user's presence to online if user is not idle
        if (isIdle && idleTime < maxIdleTime)
        {
            isIdle = false;
            SetUserStatus(UserStatus.Online);
        }
    }

    /// <summary>
    /// Change user status (availability and activity)
    /// </summary>
    /// <param name="availability"> availability value (ex: online, offline, etc)</param>
    /// <param name="activity"> activity value (ex: in-game, etc)</param>
    public void SetUserStatus(UserStatus availability, string activity = "")
    {
        lobby.SetUserStatus(availability, activity, result => 
        {
            if (result.IsError)
            {
                Debug.Log("Set user status is failed");
            }
            else
            {
                Debug.Log($"User availability: {availability} activity: {activity}");
                if (LobbyHandler.Instance != null)
                {
                    string status = string.IsNullOrEmpty(activity) ? availability.ToString() : activity;
                    LobbyHandler.Instance.friendsHandler.availabilityText.text = status;
                }
            }
        });
    }

    /// <summary>
    /// Get friend status (availabilit and activity) list
    /// </summary>
    /// <param name="friendStatusResult"> Contains data of friend status</param>
    public void UpdateFriendListStatus(Action<FriendsStatus> friendStatusResult)
    {
        lobby.ListFriendsStatus(result => 
        {
            if (result.IsError)
            {
                Debug.Log("Get friend status list is failed");
            }
            else
            {
                friendStatusResult.Invoke(result.Value);
            }
        });
    }

    /// <summary>
    /// Called when friend status (availability or activity) is changed
    /// </summary>
    /// <param name="notification"> Contains data of user id, availability, activity, and last seen </param>
    public void OnFriendsStatusChanged(FriendsStatusNotif notification)
    {
        //Find the friend and update it's UI
        if (LobbyHandler.Instance.friendsHandler.friendUIDictionary.ContainsKey(notification.userID))
        {
            LobbyHandler.Instance.friendsHandler.friendUIDictionary[notification.userID].SetOnlineStatus(notification);
        }
        //Otherwise We should handle this in some way, possibly creating a Friend UI Piece
        else
        {
            Debug.Log("Unregistered Friend received a Notification");
        }
    }
}
