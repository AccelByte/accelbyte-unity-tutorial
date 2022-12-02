// Copyright (c) 2021 - 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;


public class FriendStatusPanel : MonoBehaviour
{
    [SerializeField]
    private Image profilePicture;
    [SerializeField]
    private Image statusDisplay;

    [SerializeField]
    private Button chatButton;
    [SerializeField]
    private Button inviteToPartyButton;
    [SerializeField]
    private Button unfriendButton;
    [SerializeField]
    private Button blockButton;

    [SerializeField]
    private Text displayNameText;
    [SerializeField]
    private Text onlineStatusText;

    // AccelByte's Multi Registry references
    private Lobby lobby;
    private UserProfiles userProfiles;

    private BaseUserInfo _userData;

    private void OnEnable()
    {
        // AccelByte's Multi Registry initialization
        ApiClient apiClient = MultiRegistry.GetApiClient();
        lobby = apiClient.GetApi<Lobby, LobbyApi>();
        userProfiles = apiClient.GetApi<UserProfiles, UserProfilesApi>();
    }

    /// <summary>
    /// Set online status in the panel
    /// </summary>
    /// <param name="notification"> Contains data of user id, availability, activty, and last seen</param>
    public void SetOnlineStatus(FriendsStatusNotif notification)
    {
        switch (notification.availability)
        {
            case UserStatus.Offline:
                onlineStatusText.text = "Offline";
                statusDisplay.color = Color.grey;
                break;
            case UserStatus.Online:
                onlineStatusText.text = "Online";
                statusDisplay.color = Color.green;
                break;
            case UserStatus.Busy:
                onlineStatusText.text = "Busy";
                statusDisplay.color = Color.red;
                break;
            case UserStatus.Invisible:
                onlineStatusText.text = "Offline";
                statusDisplay.color = Color.grey;
                break;
            default:
                onlineStatusText.text = $"INVALID UNHANDLED {notification.availability}";
                statusDisplay.color = Color.black;
                break;
        }

        switch (notification.activity)
        {
            case "Away":
                onlineStatusText.text = notification.activity;
                statusDisplay.color = Color.yellow;
                break;
            case "In-Game":
                onlineStatusText.text = notification.activity;
                statusDisplay.color = Color.blue;
                break;
            default:
                break;
        }
        
        Debug.Log($"Friend availability for {notification.userID} is {onlineStatusText.text}");
    }

    public void Create(BaseUserInfo pud)
    {
        _userData = pud;
        displayNameText.text = _userData.displayName;
        
        RetrieveProfilePicture();
    }

    void RetrieveProfilePicture()
    {
        userProfiles.GetUserAvatar(_userData.userId,
            result =>
            {
                if (!result.IsError)
                {
                    if (profilePicture != null)
                    {
                        profilePicture.sprite = Sprite.Create(result.Value, new Rect(0f, 0f, 64f, 64f), Vector2.zero);
                    } 
                }
                else
                {
                    Debug.LogWarning($"Unable to retrieve Avatar for User {_userData.userId}: {result.Error}");
                }
            }
        );
    }

    /// <summary>
    /// Setup UI Button Listener
    /// </summary>
    public void SetupButton()
    {
        chatButton.onClick.AddListener(() => 
        {
            LobbyHandler.Instance.chatHandler.AddPrivateTabButton(_userData.userId, _userData.displayName);
        });
        inviteToPartyButton.onClick.AddListener(() => 
        {
            LobbyHandler.Instance.partyHandler.InviteToParty(_userData.userId);
        });
        unfriendButton.onClick.AddListener(() =>
        {
            lobby.Unfriend(_userData.userId, result =>
            {
                if (result.IsError)
                {
                    Debug.Log($"Failed to unfriend a friend: code: {result.Error.Code}, message: {result.Error.Message}");
                }
                else
                {
                    Debug.Log("Successfully unfriend a friend!");
                    LobbyHandler.Instance.friendsHandler.RefreshFriendsList();
                }
            });
        });
        blockButton.onClick.AddListener(() =>
        {
            lobby.BlockPlayer(_userData.userId, result =>
            {
                if (result.IsError)
                {
                    Debug.Log($"Failed to block a friend code: {result.Error.Code}, message: {result.Error.Message}");
                }
                else
                {
                    Debug.Log("Successfully block a friend!");
                    LobbyHandler.Instance.friendsHandler.RefreshFriendsList();
                    LobbyHandler.Instance.friendsHandler.RefreshBlockedList();
                }
            });
        });
    }
}
