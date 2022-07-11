// Copyright (c) 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AccelByte.Api;
using System;

public class CloudSaveHandler : MonoBehaviour
{
    [SerializeField]
    public GameObject settingWindow;
    [SerializeField]
    private GameObject backConfirmationPanel;

    [SerializeField]
    private Button saveChangesButton;
    [SerializeField]
    private Button backToMainMenuButton;

    [SerializeField]
    private Button keepEditingButton;
    [SerializeField]
    private Button forceBackToMainMenuButton;

    [SerializeField]
    private List<Dropdown> listDropdown;
    [SerializeField]
    private List<int> tempDropdownValueUnchanged;

    private bool isInitialized = false;

    public void SetupCloudSave()
    {
        if (isInitialized) return;
        isInitialized = true;

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

        GetUserRecord("Settings");
    }

    /// <summary>
    /// Set user record in admin portal.
    /// </summary>
    /// <param name="key">key param</param>
    /// <param name="recordRequest">record data to set.</param>
    public void SetUserRecord(string key, Dictionary<string, object> recordRequest)
   {
        AccelBytePlugin.GetCloudSave().SaveUserRecord(key, false, recordRequest, result =>
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
        AccelBytePlugin.GetCloudSave().GetUserRecord(key, result =>
        {
            if (result.IsError)
            {
                // Do something if GetUserRecord has an error
                Debug.Log($"Error GetUserRecord, Error Code: {result.Error.Code} Error Message: {result.Error.Message}");
            }
            else
            {
                // Do something if GetUserRecord has been successful
                Debug.Log("GetUserRecord success");

                foreach (var kvp in result.Value.value)
                {
                    Debug.Log($"key: {kvp.Key} Value: {kvp.Value}");

                    if (kvp.Key == "DisplayMode")
                    {
                        listDropdown[0].value = Convert.ToInt32(kvp.Value);
                        tempDropdownValueUnchanged.Add(Convert.ToInt32(kvp.Value));
                    }
                    if (kvp.Key == "ExampleState")
                    {
                        listDropdown[1].value = Convert.ToInt32(kvp.Value);
                        tempDropdownValueUnchanged.Add(Convert.ToInt32(kvp.Value));
                    }
                }

                ChangeScreen(listDropdown[0].value);
            }
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
        AccelBytePlugin.GetCloudSave().GetPublicUserRecord(key, userId, result =>
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
        AccelBytePlugin.GetCloudSave().ReplaceUserRecord(key, false, recordRequest, result =>
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
        Dictionary<string, object> testing = new Dictionary<string, object>
        {
            { "DisplayMode", listDropdown[0].value },
            { "ExampleState", listDropdown[1].value }
        };

        for (int i = 0; i < tempDropdownValueUnchanged.Count; i++)
        {
            tempDropdownValueUnchanged[i] = listDropdown[i].value;
        }

        SetUserRecord("Settings", testing);
        ChangeScreen(listDropdown[0].value);
    }

    /// <summary>
    /// Back to main menu
    /// </summary>
    private void BackToMainMenu()
    {
        for (int i = 0; i < tempDropdownValueUnchanged.Count; i++)
        {
            // return if nothing changed
            if (tempDropdownValueUnchanged[i] != listDropdown[i].value)
            {
                backConfirmationPanel.SetActive(true);
                return;
            }
        }

        GetComponent<MenuHandler>().Menu.gameObject.SetActive(true);
        settingWindow.SetActive(false);
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
        // revert value of dropdown when back to main menu.
        for (int i = 0; i < tempDropdownValueUnchanged.Count; i++)
        {
            listDropdown[i].value = tempDropdownValueUnchanged[i];
        }

        backConfirmationPanel.SetActive(false);
        GetComponent<MenuHandler>().Menu.gameObject.SetActive(true);
        settingWindow.SetActive(false);
    }

    /// <summary>
    /// Change the full screen application based on the value dropdown
    /// </summary>
    /// <param name="value"> Dropdown value to set full screen mode or not.</param>
    private void ChangeScreen(int value)
    {
        Screen.SetResolution(1024, 768, value == 1); 
    }
}
