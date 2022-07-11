// Copyright (c) 2021 - 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using UnityEngine;
using AccelByte.Api;
using AccelByte.Core;
using UnityEngine.UI;
using AccelByte.Models;
using UnityEngine.SceneManagement;

public class LoginHandler : MonoBehaviour
{
    /// <summary>
    /// Private Instance
    /// </summary>
    static LoginHandler _instance;
    /// <summary>
    /// The Instance Getter
    /// </summary>
    public static LoginHandler Instance => _instance;
    private void Awake()
    {
#if UNITY_SERVER
        // Auto go to gameplay scene for server
        SceneManagementHandler.ChangeScene(SceneManagementHandler.SceneName.WatchTime);
#endif
    }
    
    [SerializeField]
    Button loginButton;
    [SerializeField]
    Text statusText;
    [SerializeField]
    InputField usernameInputField;
    [SerializeField]
    InputField passwordInputField;
    [SerializeField]
    RectTransform loginPanel;
    
    private void OnEnable()
    {
        //When we Activate, set the Text of the Login Status
        //and add the Login Call to the Button's listener
        statusText.text = "Please Login";
        loginButton.onClick.AddListener(() =>
        {
            statusText.text = "Attempting Login";
            OnLoginClick(usernameInputField.text, passwordInputField.text);
        });
    }

    private void Start()
    {
        // If session is still valid, go to lobby menu
        if (AccelBytePlugin.GetUser().Session.IsValid())
        {
            LobbyHandler.Instance.ConnectToLobby();
            loginPanel.gameObject.SetActive(false);
        }
        else
        {
            // Always start with the window screen
            Screen.fullScreen = false;
        }
    }
    private void OnDisable()
    {
        //When we disable, clear all of the Listeners from the Login Button
        loginButton.onClick.RemoveAllListeners();
    }
    
    /// <summary>
    /// Function called to Login to AccelByte's IAM services
    /// </summary>
    /// <param name="username">The Username (typically an email address) of the user</param>
    /// <param name="password">The password of the user</param>
    public void OnLoginClick(string username, string password)
    {
        //Disable Interaction with the Login Button so the player cannot spam click it and send multiple requests
        loginButton.interactable = false;
        statusText.text = "Logging in...";
        //Grab a reference to the current User, even though they have not been logged in yet. 
        //This also acts as the initialisation point for the whole AccelByte plugin.
        User user = AccelBytePlugin.GetUser();
        //Calling the Login Function and supplying a callback to act upon based upon success or failure
        //You will almost certainly want to extend this functionality further
        //Note that this callback is asynchronous
        user.LoginWithUsername(username, password,
            (Result<TokenData, OAuthError> result) =>
            {
                if (result.IsError)
                {
                    //If we error, grab the Error Error and Description to print in the Log
                    Debug.Log($"Login failed : {result.Error.error} Description : {result.Error.error_description}");
                    //Set the Status Text to display the Error if there is any
                    statusText.text = $"Login failed : {result.Error.error} Description : {result.Error.error_description}";
                }
                else
                {
                    Debug.Log("Login successful");
                    
                    // check agreement eligibility
                    loginPanel.gameObject.SetActive(false);
                    GetComponent<AgreementHandler>().Setup();
                }

                //Enable interaction with the Button again
                loginButton.interactable = true;
            }
        );
    }

    /// <summary>
    /// Function called to Log Out from AccelByte's IAM services
    /// </summary>
    public void OnLogoutClicked()
    {
        Debug.Log($"Logging out...");
        //Grab a User reference
        User user = AccelBytePlugin.GetUser();

        // Call Lobby Handler to disconnect from lobby
        LobbyHandler.Instance.DisconnectFromLobby();

        //Calling the Logout Function and supplying a callback
        user.Logout((Result result) => {
            if (result.IsError)
            {
                //If we error, grab the Error Code and Message to print in the Log
                Debug.Log($"Logout failed : {result.Error.Code} Description : {result.Error.Message}");
            }
            else
            {
                Debug.Log("Logout successful");

                // Call Lobby Handler to remove the listeners
                LobbyHandler.Instance.RemoveLobbyListeners();

                // Open Login UI
                loginPanel.gameObject.SetActive(true);

                Scene scene = SceneManager.GetActiveScene();
                SceneManager.LoadScene(scene.name);
            }
        });
    }
}