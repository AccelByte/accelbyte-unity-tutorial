// Copyright (c) 2021 - 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AccelByte.Api;
using AccelByte.Models;
using UnityEngine.UI;

/// <summary>
/// A Handler Class to deal with the Creation of <see cref="PublicUserData"/> if a User Does NOT have one
/// </summary>
public class ProfileCreationHandler : MonoBehaviour
{
    //Define all the Input Fields
    #region Input Fields
    [SerializeField]
    private InputField firstNameInputField;
    [SerializeField]
    private InputField lastNameInputField;
    [SerializeField]
    private InputField yearDateInputField;
    [SerializeField]
    private InputField monthDateInputField;
    [SerializeField]
    private InputField dayDateInputField;
    #endregion

    [SerializeField]
    private Dropdown languageDropdown;
    [SerializeField]
    private Dropdown timeZoneDropdown; 
    [SerializeField]
    private Dropdown countryDropdown;

    [SerializeField]
    private Button createProfileButton;

    [SerializeField]
    private Text statusText;

    [SerializeField]
    private GameObject profilePanel;
    
    
    public void Setup()
    {
        //Reset all of the Dropdown Options and InputFields in case they have changed between users 
        timeZoneDropdown.ClearOptions();
        languageDropdown.ClearOptions();
        countryDropdown.ClearOptions();
        firstNameInputField.text = "";
        lastNameInputField.text = "";
        yearDateInputField.text = "";
        monthDateInputField.text = "";
        dayDateInputField.text = "";
        statusText.text = "Status:";
        //Ensure that the Profile Button is Interactable
        createProfileButton.interactable = true;
        //Remove and then Add the onClick Listener
        createProfileButton.onClick.RemoveAllListeners();
        createProfileButton.onClick.AddListener(SubmitProfile);
        
        //Start a coroutine to download and populate the Language and TimeZones
        StartCoroutine(AwaitRequests());
        
    }

    /// <summary>
    /// Validate the Data entered into the Profile Creation Window
    /// </summary>
    /// <returns>True is Valid, False if invalid</returns>
    bool ValidateData()
    {
        if (firstNameInputField.text.Length == 0 || lastNameInputField.text.Length == 0 ||
            yearDateInputField.text.Length == 0 || monthDateInputField.text.Length == 0 ||
            dayDateInputField.text.Length == 0 ||
            yearDateInputField.text.Length < 4 || int.Parse(yearDateInputField.text) <= 0 ||
            int.Parse(monthDateInputField.text) <= 0 || int.Parse(dayDateInputField.text) <= 0)
        {
            return false;
        }

        return true;
    }
    
    /// <summary>
    /// Attempt to submit a Create User Profile request
    /// </summary>
    private void SubmitProfile()
    {
        //Validate that the Entered Data is valid
        if (!ValidateData())
        {
            Debug.LogWarning("Entered Information Failed Validation");
            statusText.text = "Failed Validation";
            return;
        }
        //Set the button to be Non-Interactable whilst we wait for the request to be completed
        createProfileButton.interactable = false;

        //Create and populate the CreateUserProfileRequest
        CreateUserProfileRequest request = new CreateUserProfileRequest()
        {
            firstName = firstNameInputField.text,
            lastName = lastNameInputField.text,
            //Note that the dateOfBirth has to be in ISO 8601 format (yyyy-mm-dd)
            dateOfBirth = $"{yearDateInputField.text}-{monthDateInputField.text}-{dayDateInputField.text}",
            //Grab the selected option from the Dropdown
            language = languageDropdown.options[languageDropdown.value].text,
            timeZone = timeZoneDropdown.options[timeZoneDropdown.value].text,
            //Here we will add a Temporary Avatar URL (The AccelByte Logo), so that we can display something in the Friends Panel
            //Note that any Avatar must be either a PNG or JPG
            avatarUrl = "https://avatars.githubusercontent.com/u/25496952"
        };
        
        //Send the Request with the Constructed payload
        AccelBytePlugin.GetUserProfiles().CreateUserProfile(request, result =>
        {
            //If there is an error, display the error information and set the button back to be interactable
            if (result.IsError)
            {
                createProfileButton.interactable = true;

                Debug.Log($"Failed to Create User Profile: error code: {result.Error.Code} message: {result.Error.Message}");
                statusText.text =
                    $"Failed to Create User Profile: error code: {result.Error.Code} message: {result.Error.Message}";
            }
            else
            {
                Debug.Log($"Created User Profile for {result.Value.userId}");
                //Set the Profile Panel to be inactive
                profilePanel.SetActive(false);
                //Call to the Lobby Handler to connect
                LobbyHandler.Instance.ConnectToLobby();
            }
        });
    }
    
    /// <summary>
    /// Gather all the information to display in the UI
    /// </summary>
    /// <returns></returns>
    private IEnumerator AwaitRequests()
    {
        //Create three bools to keep track of all of the requests simultaneously 
        bool loadedLanguages = false, loadedCountries = false, loadedTimeZones = false;
        //Get Valid Languages
        AccelBytePlugin.GetMiscellaneous().GetLanguages(result =>
        {
            if (result.IsError)
            {
                Debug.LogWarning($"Unable to get Languages Code:{result.Error.Code}, Message:{result.Error.Message}");
                //Add a fallback language of en English
                languageDropdown.options.Add(new Dropdown.OptionData("en"));
            }
            else
            {
                //Loop through returned data and add to the Dropdown Options
                List<Dropdown.OptionData> data = new List<Dropdown.OptionData>();
                foreach (KeyValuePair<string,string> keyValuePair in result.Value)
                {
                    data.Add(new Dropdown.OptionData(keyValuePair.Key));
                }
                languageDropdown.AddOptions(data);
            }

            loadedLanguages = true;
        } 
            );
        //Get Valid Countries
        AccelBytePlugin.GetMiscellaneous().GetCountryGroups(result =>
        {
            if (result.IsError)
            {
                Debug.LogWarning($"Unable to get Countries Code:{result.Error.Code}, Message:{result.Error.Message}");
                //Add a fallback language of en English
                countryDropdown.options.Add(new Dropdown.OptionData("us"));
            }
            else
            {
                //Loop through returned data and add to the Dropdown Options
                List<Dropdown.OptionData> data = new List<Dropdown.OptionData>();
                foreach (Country country in result.Value)
                {
                    data.Add(new Dropdown.OptionData(country.code));
                }
                countryDropdown.AddOptions(data);
            }

            loadedCountries = true;
        });
        //Get Valid Time Zones
        AccelBytePlugin.GetMiscellaneous().GetTimeZones(result =>
        {
            if (result.IsError)
            {
                Debug.LogWarning($"Unable to get Time Zones Code:{result.Error.Code}, Message:{result.Error.Message}");
                //Add a fallback Timezone of en English
                timeZoneDropdown.options.Add(new Dropdown.OptionData("Europe/London"));
            }
            else
            {
                //Loop through returned data and add to the Dropdown Options
                List<Dropdown.OptionData> data = new List<Dropdown.OptionData>();
                foreach (string s in result.Value)
                {
                    data.Add(new Dropdown.OptionData(s));
                }
                timeZoneDropdown.AddOptions(data);
            }

            loadedTimeZones = true;
        });

        //Wait whilst Requests are executed
        while (loadedCountries ==false || loadedLanguages==false||loadedTimeZones==false)
        {
            yield return null;
        }
        
        //Set the UI to be visible now that it's populated
        profilePanel.SetActive(true);
        yield return null;
    }

}
