// Copyright (c) 2021 - 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using AccelByte.Api;
using UnityEngine;
using UnityEngine.UI;

public class MenuHandler : MonoBehaviour
{
    public Transform Menu;

    public Button LobbyButton;
    public Button FriendsButton;
    public Button LeaderboardButton;
    public Button StoreButton;
    public Button SettingsButton;
    public Button GalleryButton;
    public Button InventoryButton;
    public Button AchievementButton;
    public Button QuitButton;

    #region Profile

    [Header("Profile Display")]
    [SerializeField]
    private Text displayNameText;
    [SerializeField]
    private Image profileImage;

    #endregion

    private bool isInitialized = false;


    /// <summary>
    /// Initialize the Main Menu's UI
    /// </summary>
    public void Create()
    {
        // Check if Main Menu has been initialized
        if (isInitialized) return;

        isInitialized = true;

        // Initialized Screen
        GetComponent<CloudSaveHandler>().SetupCloudSave();

        LobbyButton.onClick.AddListener(() =>
        {
            GetComponent<PartyHandler>().SetupParty();
            GetComponent<MatchmakingHandler>().SetupMatchmaking();
            GetComponent<ChatHandler>().SetupChat();
            GetComponent<QoSHandler>().SetupQoS();

            Menu.gameObject.SetActive(false);
            GetComponent<LobbyHandler>().LobbyWindow.SetActive(true);
        });

        FriendsButton.onClick.AddListener(() =>
        {
            GetComponent<FriendsManagementHandler>().Setup(FriendsManagementHandler.ExitMode.Menu);
            Menu.gameObject.SetActive(false);
            GetComponent<FriendsManagementHandler>().FriendsManagementWindow.SetActive(true);
        });

        LeaderboardButton.onClick.AddListener(() =>
        {
            GetComponent<LeaderboardHandler>().Setup();
            Menu.gameObject.SetActive(false);
            GetComponent<LeaderboardHandler>().LeaderboardWindow.SetActive(true);
        });

        StoreButton.onClick.AddListener(() => 
        {
            GetComponent<StoreHandler>().Setup();
            GetComponent<WalletHandler>().UpdateWallet();
            Menu.gameObject.SetActive(false);
            GetComponent<StoreHandler>().StoreWindow.SetActive(true);
        });

        SettingsButton.onClick.AddListener(() =>
        {
            Menu.gameObject.SetActive(false);
            GetComponent<CloudSaveHandler>().settingWindow.SetActive(true);
        });

        GalleryButton.onClick.AddListener(() =>
        {
            Menu.gameObject.SetActive(false);
            GetComponent<CloudStorageHandler>().Setup();
        });

        InventoryButton.onClick.AddListener(() => 
        {
            GetComponent<EntitlementHandler>().Setup();
            Menu.gameObject.SetActive(false);
            GetComponent<EntitlementHandler>().EntitlementWindow.SetActive(true);
        });

        AchievementButton.onClick.AddListener(() => 
        {
            GetComponent<AchievementHandler>().Setup();
            Menu.gameObject.SetActive(false);
            GetComponent<AchievementHandler>().AchievementWindow.SetActive(true);
        });

        QuitButton.onClick.AddListener(() =>
        {
            GetComponent<LoginHandler>().OnLogoutClicked();
            Menu.gameObject.SetActive(false);
        });
    }

    /// <summary>
    /// Display Current Player's Profile from the User Profile Data
    /// </summary>
    public void DisplayProfile()
    {
        AccelBytePlugin.GetUserProfiles().GetUserProfile(result =>
        {
            // check this is not an error
            if (!result.IsError)
            {
                // get the player's display name
                AccelBytePlugin.GetUser().GetUserByUserId(result.Value.userId, userResult =>
                {
                    if (!userResult.IsError)
                    {
                        displayNameText.text = userResult.Value.displayName.ToString();
                    }
                });

                // retrieve profile's avatar image
                StartCoroutine(ABUtilities.DownloadTexture2D(result.Value.avatarUrl, imageResult =>
                {
                    profileImage.sprite = Sprite.Create(imageResult.Value, new Rect(0f, 0f, imageResult.Value.width, imageResult.Value.height), Vector2.zero);
                }));
            }
            else
            {
                Debug.Log($"Error GetUserProfile, Error Code: {result.Error.Code} Error Message: {result.Error.Message}");
            }
        });
    }
}
