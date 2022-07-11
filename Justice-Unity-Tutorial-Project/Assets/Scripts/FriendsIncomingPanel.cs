// Copyright (c) 2021 - 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using AccelByte.Api;
using AccelByte.Models;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

/// <summary>
/// Used to control the UI for the Incoming Pending Friend Request
/// </summary>
public class FriendsIncomingPanel : MonoBehaviour
{
    [SerializeField]
    private Image profilePicture;

    [SerializeField]
    private Text displayNameText;
    [SerializeField]
    private Button acceptFriendButton;
    [SerializeField]
    private Button declineFriendButton;
    [SerializeField]
    private Button blockFriendButton;

    private PublicUserData _userData;
    
    /// <summary>
    /// An initialisation Function required to be called to Populate the UI appropriately
    /// </summary>
    /// <param name="userData">The PublicUserData that is required to Populate the UI</param>
    public void Create(PublicUserData userData)
    {
        //Cache the PublicUserData
        _userData = userData;
        //Set the Display Name
        displayNameText.text = _userData.displayName;
        //Setup the Button to Accept a Friend Request
        acceptFriendButton.onClick.AddListener(() =>
        {
            //Make the Call to Accept the Friend Request using the cached PublicUserData UserID
            AccelBytePlugin.GetLobby().AcceptFriend(_userData.userId, result =>
            {
                if (result.IsError)
                {
                    //We would probably want to display some user-feedback if the request is not successful
                    Debug.Log($"Failed to accept a friend request: error code: {result.Error.Code} message: {result.Error.Message}");
                }
                else
                {
                    Debug.Log("Accepted Friend Request");
                    //Destroy the UI Piece if the request was successful to remove it from the list
                    Destroy(gameObject);
                    //Check the current incoming panel state if reset panel needed
                    LobbyHandler.Instance.friendsHandler.ResetPanelState(transform.parent, FriendsManagementHandler.PanelMode.Incoming);
                }
            });
            
        });
        //Setup the Button to Decline a Friend Request
        declineFriendButton.onClick.AddListener(() =>
        {
            //Make the Call to Decline the Friend Request using the cached PublicUserData UserID
            AccelBytePlugin.GetLobby().RejectFriend(_userData.userId, result =>
            {
                if (result.IsError)
                {
                    //We would probably want to display some user-feedback if the request is not successful
                    Debug.Log($"Failed to decline a friend request: error code: {result.Error.Code} message: {result.Error.Message}");
                }
                else
                {
                    Debug.Log("Declined Friend Request");
                    //Destroy the UI Piece if the request was successful to remove it from the list
                    Destroy(gameObject);
                    //Check the current incoming panel state if reset panel needed
                    LobbyHandler.Instance.friendsHandler.ResetPanelState(transform.parent, FriendsManagementHandler.PanelMode.Incoming);
                }
            });
            
        });
        //Setup the Button to Block a Player
        blockFriendButton.onClick.AddListener(() =>
        {
            //Make the Call to Block a Player using the cached PublicUserData UserID
            AccelBytePlugin.GetLobby().BlockPlayer(_userData.userId, result =>
            {
                if (result.IsError)
                {
                    //We would probably want to display some user-feedback if the request is not successful
                    Debug.Log($"Failed to block a friend request: error code: {result.Error.Code} message: {result.Error.Message}");
                }
                else
                {
                    Debug.Log("Block Friend Request");
                }
            });
            
        });
        //Request the Profile Picture of the User
        RetrieveProfilePicture();
    }
    
    /// <summary>
    /// Attempt to retrieve the User Avatar/Profile Picture for the Friend
    /// </summary>
    void RetrieveProfilePicture()
    {
        //Request the Avatar based on the UserID
        AccelBytePlugin.GetUserProfiles().GetUserAvatar(_userData.userId,
            result =>
            {
                //If it's not an error, convert the returned texture into a Sprite and Display it
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
    
}
