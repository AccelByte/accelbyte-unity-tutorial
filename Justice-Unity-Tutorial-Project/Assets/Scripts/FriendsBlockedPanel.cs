// Copyright (c) 2021 - 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
public class FriendsBlockedPanel : MonoBehaviour
{
    
    [SerializeField]
    private Image profilePicture;
    [SerializeField]
    private Text displayNameText;
    [SerializeField]
    private Button unblockButton;

    // AccelByte's Multi Registry references
    private Lobby lobby;
    private UserProfiles userProfiles;

    private PublicUserData _userData;

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
    /// <param name="userData">The PublicUserData that is required to Populate the UI</param>
    public void Create(PublicUserData userData)
    {
        //Cache the PublicUserData
        _userData = userData;
        //Set the Display Name
        displayNameText.text = _userData.displayName;
        //Setup the Button to Unblock a User
        unblockButton.onClick.AddListener(() =>
        {
            //Make the Call to Unblock the given User using the cached PublicUserData UserID
            lobby.UnblockPlayer(_userData.userId, result =>
            {
                if (result.IsError)
                {
                    //We would probably want to display some user-feedback if the request is not successful
                    Debug.Log($"Failed to unblock a player error code: {result.Error.Code} message: {result.Error.Message}");
                }
                else
                {
                    Debug.Log("Successfully unblock a player!");
                    //Destroy the UI Piece if the request was successful to remove it from the list
                    Destroy(gameObject);
                    //Check the current blocked panel state if reset panel needed
                    LobbyHandler.Instance.friendsHandler.ResetPanelState(transform.parent, FriendsManagementHandler.PanelMode.Blocked);
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
        userProfiles.GetUserAvatar(_userData.userId,
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
