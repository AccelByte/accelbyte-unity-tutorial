// Copyright (c) 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AccelByte.Api;
using System;
using AccelByte.Core;

public class CloudSaveHandler : MonoBehaviour
{
    [SerializeField]
    public GameObject cloudSaveWindow;

    [Header("Game Record")]
    [SerializeField]
    private Text gameAnnouncementText;

    [Header("Player Record")]
    [SerializeField]
    private Dropdown currentSettingDropdown;
    [SerializeField]
    private Dropdown displayModeDropdown;
    [SerializeField]
    private InputField mouseSensitivityInputField;
    [SerializeField]
    private Dropdown maxFpsDropdown;
    [SerializeField]
    private InputField otherSettingsInputField;

    [Header("Button Pannel")]
    [SerializeField]
    private Button saveChangesButton;
    [SerializeField]
    private Button backToMainMenuButton;

    [Header("Back Confirmation Panel")]
    [SerializeField]
    private GameObject backConfirmationPanel;
    [SerializeField]
    private Button keepEditingButton;
    [SerializeField]
    private Button forceBackToMainMenuButton;

    [Header("Notif Panel")]
    [SerializeField]
    private GameObject cloudSaveNotifPanel;
    [SerializeField]
    private Text cloudSaveNotifText;
    [SerializeField]
    private Button confirmNotifButton;

    private bool isInitialized = false;
    private bool isSettingsChanged = false;

    private Dictionary<string, object> currentGameSetting = new Dictionary<string, object>();

    private CloudSave cloudSave;

    #region Cloud Save's Default Value
    // Cloud Save Record Key Value Name
    private string gameAnnouncementKeyName = "GameAnnouncement";
    private string gameSettingsKeyName = "GameSettings";

    // Game Record's Json Key Name
    private string announcementName = "announcement";

    // Player Record's Json Keys Name
    private const string currentSettingKey = "current-setting";
    private const string displayModeKey = "display-mode";
    private const string mouseSensitivityKey = "mouse-sensitivity";
    private const string maxFpsKey = "max-fps";
    private const string otherSettingsKey = "other-settings";
    #endregion

    private void OnEnable()
    {
        // AccelByte's Multi Registry initialization
        ApiClient apiClient = MultiRegistry.GetApiClient();
        cloudSave = apiClient.GetApi<CloudSave, CloudSaveApi>();
    }

    public void SetupCloudSave()
    {
        // reset announcement text
        gameAnnouncementText.text = "";
        // get the game announcement data from game record with key name "GameAnnouncement"
        GetGameRecord(gameAnnouncementKeyName);
        // get the game settings data from player record with key name "GameSettings"
        GetUserRecord(gameSettingsKeyName);

        if (isInitialized) return;
        isInitialized = true;

        // listener on game settings value changes
        currentSettingDropdown.onValueChanged.AddListener((int value) => 
        {
            isSettingsChanged = true;
            
            // if Current Setting is "Default", set all setting to their default values
            if (value == 0)
            {
                Dictionary<string, object> defaultGameSetting = new Dictionary<string, object>
                {
                    { currentSettingKey, 0 },
                    { displayModeKey, 0 },
                    { mouseSensitivityKey, 1 },
                    { maxFpsKey, 1 },
                    { otherSettingsKey, "" },
                };
                UpdateCurrentGameSettingsOptions(defaultGameSetting);
            }
        });
        displayModeDropdown.onValueChanged.AddListener((int value) => 
        {
            isSettingsChanged = true;
            // if display mode's value is not default, change the Current Setting to "Custom"
            currentSettingDropdown.value = (value != 0) ? 1 : currentSettingDropdown.value;
        });
        mouseSensitivityInputField.onValueChanged.AddListener((string value) => 
        {
            // check if input is valid
            if (value == "")
            {
                cloudSaveNotifText.text = "Mouse Sensitivity value can't be empty. Please input any number";
                cloudSaveNotifPanel.SetActive(true);
                return;
            }

            isSettingsChanged = true;
            // if mouse sensitivity's value is not default, change the Current Setting to "Custom"
            currentSettingDropdown.value = (Convert.ToInt32(value) != 1) ? 1 : currentSettingDropdown.value;
        });
        maxFpsDropdown.onValueChanged.AddListener((int value) => 
        {
            isSettingsChanged = true;
            // if max fps's value is not default, change the Current Setting to "Custom"
            currentSettingDropdown.value = (value != 1) ? 1 : currentSettingDropdown.value;
        });
        otherSettingsInputField.onValueChanged.AddListener((string value) => 
        {
            isSettingsChanged = true;
            // if other settings's value is not default, change the Current Setting to "Custom"
            currentSettingDropdown.value = (value != "") ? 1 : currentSettingDropdown.value;
        });

        // listener for button UIs
        saveChangesButton.onClick.AddListener(() =>
        {
            SaveChanges();
        });

        backToMainMenuButton.onClick.AddListener(() =>
        {
            BackToMainMenu();
        });

        keepEditingButton.onClick.AddListener(() =>
        {
            KeepEditing();
        });

        forceBackToMainMenuButton.onClick.AddListener(() =>
        {
            ForceToMainMenu();
        });

        confirmNotifButton.onClick.AddListener(() => 
        {
            cloudSaveNotifPanel.SetActive(false);
        });
    }

    /// <summary>
    /// Get game record from admin portal.
    /// </summary>
    /// <param name="key">key name of game record registered in admin portal</param>
    public void GetGameRecord(string key)
    {
        cloudSave.GetGameRecord(key, result => 
        {
            if (result.IsError)
            {
                Debug.Log($"Error Get Game Record, Error Code: {result.Error.Code}, Error Message: {result.Error.Message}");
            }
            else
            {
                Debug.Log("Get Game Record success");

                gameAnnouncementText.text = result.Value.value[announcementName].ToString();
            }
        });
    }

    /// <summary>
    /// Set user record in admin portal.
    /// </summary>
    /// <param name="key">key param</param>
    /// <param name="recordRequest">record data to set.</param>
    public void SetUserRecord(string key, Dictionary<string, object> recordRequest)
   {
        cloudSave.SaveUserRecord(key, recordRequest, result =>
        {
            if (result.IsError)
            {
                // Do something if SaveUserRecord has an error
                Debug.Log($"Error SaveUserRecord, Error Code: {result.Error.Code} Error Message: {result.Error.Message}");
            }
            else
            {
                // Do something if SaveUserRecord has been successful
                Debug.Log("SaveUserRecord success. Check admin portal.");

            }
        });
   }

    /// <summary>
    /// Get user record from admin portal.
    /// </summary>
    /// <param name="key">key param</param>
    /// <param name="OnSuccess">callback when success.</param>
    public void GetUserRecord(string key)
    {
        cloudSave.GetUserRecord(key, result =>
        {
            if (result.IsError)
            {
                // Do something if GetUserRecord has an error
                Debug.Log($"Error GetUserRecord, Error Code: {result.Error.Code} Error Message: {result.Error.Message}");

                // set game setting "Default" values
                currentGameSetting = new Dictionary<string, object>();
                currentGameSetting.Add(currentSettingKey, 0); // 0 = Default
                currentGameSetting.Add(displayModeKey, 0); // 0 = Windowed
                currentGameSetting.Add(mouseSensitivityKey, 1);
                currentGameSetting.Add(maxFpsKey, 1); // 1 = 60 fps
                currentGameSetting.Add(otherSettingsKey, "");
            }
            else
            {
                // Do something if GetUserRecord has been successful
                Debug.Log("GetUserRecord success");
                currentGameSetting = result.Value.value;
            }

            // Update UI
            UpdateCurrentGameSettingsOptions(currentGameSetting);
            ChangeScreen(displayModeDropdown.value);

            // make sure to reset the isSettingsChanged value
            isSettingsChanged = false;
        });
    }

    /// <summary>
    /// Get public user record to obtain other
    /// player info from client.
    /// </summary>
    /// <param name="key">key param</param>
    /// <param name="userId">user id target</param>
    public void GetPublicUserRecord(string key, string userId)
    {
        cloudSave.GetPublicUserRecord(key, userId, result =>
        {
            if (result.IsError)
            {
                // Do something if GetPublicUserRecord has an error
                Debug.Log($"Error GetPublicUserRecord, Error Code: {result.Error.Code} Error Message: {result.Error.Message}");
            }
            else
            {
                // Do something if GetPublicUserRecord has been successful
                Debug.Log("GetPublicUserRecord success");
                foreach (var kvp in result.Value.value)
                {
                    Debug.Log($"key: {kvp.Key} Error Message: {kvp.Value}");
                }
            }
        });
    }

    /// <summary>
    /// Replace work same as SetUserRecord
    /// but this more safely to use for existing file
    /// when multiple user tried to save data.
    /// </summary>
    /// <param name="key">key param</param>
    /// <param name="recordRequest">record data to store</param>
    public void ReplaceUserRecord(string key, Dictionary<string, object> recordRequest)
    {
        cloudSave.ReplaceUserRecord(key, false, recordRequest, result =>
        {
            if (result.IsError)
            {
                // Do somtehing if ReplaceUserRecord has an error
                Debug.Log($"Error ReplaceUserRecord, Error Code: {result.Error.Code} Error Message: {result.Error.Message}");
            }
            else
            {
                // Do something if ReplaceUserRecord has been successful
                Debug.Log($"SaveUserRecord success. Check admin portal.");
            }
        });
    }

    /// <summary>
    /// Save the changes to variable and cloud save
    /// </summary>
    private void SaveChanges()
    {
        // check if mouse sensitivity's input is valid
        if (mouseSensitivityInputField.text == "")
        {
            cloudSaveNotifText.text = "Mouse Sensitivity value can't be empty. Please input any number";
            cloudSaveNotifPanel.SetActive(true);
            return;
        }

        Dictionary<string, object> unsavedGameSetting = new Dictionary<string, object>
        {
            { currentSettingKey, currentSettingDropdown.value },
            { displayModeKey, displayModeDropdown.value },
            { mouseSensitivityKey, Convert.ToInt32(mouseSensitivityInputField.text) },
            { maxFpsKey, maxFpsDropdown.value },
            { otherSettingsKey, otherSettingsInputField.text },
        };

        // Save the game setting to player record with key name "GameSettings"
        SetUserRecord("GameSettings", unsavedGameSetting);
        currentGameSetting = unsavedGameSetting;
        isSettingsChanged = false;

        ChangeScreen(displayModeDropdown.value);

        // notify player if saving process success
        cloudSaveNotifText.text = "Save Successful!";
        cloudSaveNotifPanel.SetActive(true);
    }

    /// <summary>
    /// Back to main menu
    /// </summary>
    private void BackToMainMenu()
    {
        if (isSettingsChanged)
        {
            backConfirmationPanel.SetActive(true);
        }
        else
        {
            // return if nothing changed
            GetComponent<MenuHandler>().Menu.gameObject.SetActive(true);
            cloudSaveWindow.SetActive(false);
        }
    }

    /// <summary>
    /// Keep editing in the setting
    /// </summary>
    private void KeepEditing()
    {
        backConfirmationPanel.SetActive(false);
    }

    /// <summary>
    /// Back to main menu even there are unsafe changes
    /// </summary>
    private void ForceToMainMenu()
    {
        // revert isSettingsChanged value when back to main menu.
        isSettingsChanged = false;

        backConfirmationPanel.SetActive(false);
        GetComponent<MenuHandler>().Menu.gameObject.SetActive(true);
        cloudSaveWindow.SetActive(false);
    }

    /// <summary>
    /// Change the full screen application based on the value dropdown
    /// </summary>
    /// <param name="value"> Dropdown value to set full screen mode or not.</param>
    private void ChangeScreen(int value)
    {
        // if value == 1, set to fullscreen
        Screen.SetResolution(1024, 768, value == 1); 
    }

    /// <summary>
    /// Update current Game Settings Options to UI
    /// </summary>
    /// <param name="displayedGameSetting">game setting that will be displayed</param>
    private void UpdateCurrentGameSettingsOptions(Dictionary<string, object> displayedGameSetting)
    {
        // update game settings value to Game Settings UI
        currentSettingDropdown.value = Convert.ToInt32(displayedGameSetting[currentSettingKey]);
        displayModeDropdown.value = Convert.ToInt32(displayedGameSetting[displayModeKey]);
        mouseSensitivityInputField.text = displayedGameSetting[mouseSensitivityKey].ToString();
        maxFpsDropdown.value = Convert.ToInt32(displayedGameSetting[maxFpsKey]);
        otherSettingsInputField.text = displayedGameSetting[otherSettingsKey].ToString();
    }
}
