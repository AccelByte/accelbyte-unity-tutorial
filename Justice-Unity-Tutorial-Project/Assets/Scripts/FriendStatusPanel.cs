// Copyright (c) 2021 - 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using AccelByte.Api;
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
    
    
    PublicUserData _userData;

    
    void SetOnlineStatus(FriendsStatusNotif notification)
    {
        switch (notification.availability)
        {
            case "offline":
                onlineStatusText.text = "Offline";
                statusDisplay.color = Color.black;
                break;
            case "online":
                onlineStatusText.text = "Online";
                statusDisplay.color = Color.green;
                break;
            case "busy":
                onlineStatusText.text = "Busy";
                statusDisplay.color = Color.yellow;
                break;
            case "invisible":
                onlineStatusText.text = "Offline";
                statusDisplay.color = Color.black;
                break;
            default:
                onlineStatusText.text = $"INVALID UNHANDLED {notification.availability}";
                statusDisplay.color = Color.magenta;
                break;
        }
        
        Debug.Log($"Friend Status for {notification.userID} changed to {notification.availability}");
    }

    public void Create(PublicUserData pud)
    {
        _userData = pud;
        displayNameText.text = _userData.displayName;
        
        RetrieveProfilePicture();
    }

    void RetrieveProfilePicture()
    {
        AccelBytePlugin.GetUserProfiles().GetUserAvatar(_userData.userId,
            result =>
            {
                if (!result.IsError)
                {
                    profilePicture.sprite = Sprite.Create(result.Value,new Rect(0f,0f,64f,64f),Vector2.zero);
                }
                else
                {
                    Debug.LogWarning($"Unable to retrieve Avatar for User {_userData.userId}: {result.Error}");
                }
            }
        );
    }
    
    public void UpdateUser(FriendsStatusNotif notification)
    {
        SetOnlineStatus(notification);
        
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
            AccelBytePlugin.GetLobby().Unfriend(_userData.userId, result =>
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
            AccelBytePlugin.GetLobby().BlockPlayer(_userData.userId, result =>
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
