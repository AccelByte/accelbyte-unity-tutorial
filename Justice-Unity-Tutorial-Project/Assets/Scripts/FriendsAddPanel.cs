// Copyright (c) 2021 - 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

/// <summary>
/// Controls the UI Piece associated with Adding a Friend from a Search
/// </summary>
public class FriendsAddPanel : MonoBehaviour
{
    [SerializeField]
    private Image profilePicture;

    [SerializeField]
    private Text displayNameText;
    [SerializeField]
    private Button addFriendButton;

    // AccelByte's Multi Registry references
    private Lobby lobby;
    private UserProfiles userProfiles;

    private PublicUserInfo _userData;

    private void OnEnable()
    {
        // AccelByte's Multi Registry initialization
        ApiClient apiClient = MultiRegistry.GetApiClient();
        lobby = apiClient.GetApi<Lobby, LobbyApi>();
        userProfiles = apiClient.GetApi<UserProfiles, UserProfilesApi>();
    }

    /// <summary>
    /// An initialisation Function required to be called to Populate the UI appropriately
    /// </summary>
    /// <param name="userInfo">The PublicUserInfo that is required to Populate the UI</param>
    public void Create(PublicUserInfo userInfo)
    {
        //Cache the PublicUserInfo
        _userData = userInfo;
        //Set the Display Name
        displayNameText.text = _userData.displayName;
        //Setup the Button to Request a Friend
        addFriendButton.onClick.AddListener(() =>
        {
            //Make the call to initiate the Friend Request
            lobby.RequestFriend(_userData.userId, result =>
            {
                if (result.IsError)
                {
                    Debug.Log($"Failed to send a friends request: error code: {result.Error.Code} message: {result.Error.Message}");
                }
                else
                {
                    Debug.Log("Sent Friends Request");
                    //If we were successful, set the button to be non-intractable and change the Text
                    addFriendButton.interactable = false;
                    addFriendButton.GetComponentInChildren<Text>().text = "Request Sent";
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
        userProfiles.GetUserAvatar(_userData.userId,
            result =>
            {
                //If it's not an error, convert the returned texture into a Sprite and Display it
                if (!result.IsError)
                {
                    profilePicture.sprite = Sprite.Create(result.Value,new Rect(0f,0f,result.Value.width,result.Value.height),Vector2.zero);
                }
                else
                {
                    Debug.LogWarning($"Unable to retrieve Avatar for User {_userData.userId}: {result.Error}");
                }
            }
        );
    }
}
