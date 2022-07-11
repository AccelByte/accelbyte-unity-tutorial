// Copyright (c) 2021 - 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System.Collections.Generic;
using UnityEngine;
using AccelByte.Api;
using AccelByte.Models;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

public class FriendsManagementHandler : MonoBehaviour
{
    private Dictionary<string, FriendStatusPanel> friendUIDictionary = new Dictionary<string, FriendStatusPanel>();

    [Header("Panels")]
    public GameObject FriendsManagementWindow;
    
    [FormerlySerializedAs("FriendsPanel")]
    [SerializeField]
    private RectTransform friendsPanel;
    [SerializeField]
    private RectTransform requestsPanel;
    [SerializeField]
    private RectTransform blockedPanel;
    
    #region Buttons
    [Header("Buttons")]

    [SerializeField]
    private Button friendsTabButton;
    [SerializeField]
    private Button pendingTabButton;
    [SerializeField]
    private Button blockedTabButton;

    [SerializeField]
    private Button exitButton;
    
    #endregion

    [Header("Friends Panel")]
    [SerializeField]
    private Transform friendsDisplayPanel;
    
    [FormerlySerializedAs("FriendsDisplayPrefab")]
    [SerializeField]
    private GameObject friendsDisplayPrefab;

    [SerializeField]
    private Button friendsSearchButton;

    [SerializeField]
    private Transform friendsDisplayPlaceholder;
    
    #region Search
    [Header("Search Panel")]
    [SerializeField]
    private GameObject addFriendsPrefab;
    [SerializeField]
    private Transform friendsSearchPanel;
    
    [SerializeField]
    private InputField friendsSearchInputField;
    [SerializeField]
    private RectTransform friendsSearchScrollView;
    [SerializeField]
    private Button friendsSearchQuitButton;
    
    [SerializeField]
    private Transform friendsSearchPlaceholder;

    #endregion
    
    #region Pending
    [Header("Pending Panel")]
    [SerializeField]
    private RectTransform pendingIncomingRequestsContent;
    [SerializeField]
    private RectTransform pendingOutgoingRequestsContent;
    
    [SerializeField]
    private GameObject pendingIncomingRequestsPrefab;
    [SerializeField]
    private GameObject pendingOutgoingRequestsPrefab;

    [SerializeField]
    private Transform pendingIncomingRequestPlaceholder;
    [SerializeField]
    private Transform pendingOutgoingRequestPlaceholder;
    
    [SerializeField]
    private Text pendingIncomingRequestText;
    [SerializeField]
    private Text pendingOutgoingRequestText;

    #endregion

    #region Blocked
    [Header("Blocked")]
    [SerializeField]
    private RectTransform blockedContent;
    
    [SerializeField]
    private GameObject blockedUserPrefab;
    
    [SerializeField]
    private Transform blockedDisplayPlaceholder;

    #endregion

    private static bool isInitialized = false;

    public enum PanelMode
    {
        Incoming,
        Outgoing,
        Blocked
    }

    #region FriendsMode
    public enum FriendsMode
    {
        Default,
        Friends,
        Pending,
        Blocked
    };

    private FriendsMode _displayMode = FriendsMode.Default;

    private FriendsMode DisplayMode
    {
        get => _displayMode;
        set
        {
            switch (value)
            {
                case FriendsMode.Default:
                    friendsPanel.gameObject.SetActive(true);
                    requestsPanel.gameObject.SetActive(false);
                    blockedPanel.gameObject.SetActive(false);
                    break;

                case FriendsMode.Friends:
                    friendsPanel.gameObject.SetActive(true);
                    requestsPanel.gameObject.SetActive(false);
                    blockedPanel.gameObject.SetActive(false);
                    GetFriends();
                    break;

                case FriendsMode.Pending:
                    requestsPanel.gameObject.SetActive(true);
                    friendsPanel.gameObject.SetActive(false);
                    blockedPanel.gameObject.SetActive(false);
                    DisplayPending();
                    break;

                case FriendsMode.Blocked:
                    blockedPanel.gameObject.SetActive(true);
                    friendsPanel.gameObject.SetActive(false);
                    requestsPanel.gameObject.SetActive(false);
                    DisplayBlocked();
                    break;
            }
        }
    }
    #endregion

    #region ExitMode
    public enum ExitMode
    {
        Menu,
        Lobby
    }

    private ExitMode ExitScreen
    {
        get => ExitScreen;
        set
        {
            switch (value)
            {
                case ExitMode.Menu:
                    FriendsManagementWindow.SetActive(false);
                    GetComponent<MenuHandler>().Menu.gameObject.SetActive(true);
                    break;

                case ExitMode.Lobby:
                    FriendsManagementWindow.SetActive(false);
                    GetComponent<LobbyHandler>().LobbyWindow.SetActive(true);
                    break;
            }
        }
    }
    #endregion

    public void UpdateFriends(FriendsStatusNotif notification)
    {
        //Find the friend and update it's UI
        if (friendUIDictionary.ContainsKey(notification.userID))
        {
            friendUIDictionary[notification.userID].UpdateUser(notification);
        }
        //Otherwise We should handle this in some way, possibly creating a Friend UI Piece
        else
        {
            Debug.Log("Unregistered Friend received a Notification");
        }
    }

    /// <summary>
    /// Refresh current Friends List in Friends Panel
    /// </summary>
    public void RefreshFriendsList()
    {
        GetFriends();
    }
    
    /// <summary>
    /// Refresh current Blocked List in Blocked Panel
    /// </summary>
    public void RefreshBlockedList()
    {
        DisplayBlocked();
    }

    /// <summary>
    /// Setup UI and prepare State
    /// </summary>
    /// <param name="exitType"> name of the destination panel</param>
    public void Setup(ExitMode exitType)
    {
        // reset the exit button's listener, then add the listener based on the exit screen type
        exitButton.onClick.RemoveAllListeners();
        exitButton.onClick.AddListener(() => ExitScreen = exitType);

        // Check whether the FriendsPanel already set up or not
        if (isInitialized)
        {
            DisplayMode = FriendsMode.Default;
            return;
        }
        // Run the setup if it still hasn't
        else
        {
            isInitialized = true;

            DisplayMode = FriendsMode.Friends;

            // reset listeners, so it won't triggered more than once
            friendsTabButton.onClick.RemoveAllListeners();
            pendingTabButton.onClick.RemoveAllListeners();
            blockedTabButton.onClick.RemoveAllListeners();

            // add the listeners
            friendsTabButton.onClick.AddListener(() => DisplayMode = FriendsMode.Friends);
            pendingTabButton.onClick.AddListener(() => DisplayMode = FriendsMode.Pending);
            blockedTabButton.onClick.AddListener(() => DisplayMode = FriendsMode.Blocked);
            friendsSearchPanel.gameObject.SetActive(false);
            friendsDisplayPlaceholder.gameObject.SetActive(true);
            friendsSearchQuitButton.onClick.AddListener(() => friendsSearchPanel.gameObject.SetActive(false));
            friendsSearchButton.onClick.AddListener(DisplaySearch);
            friendsSearchInputField.onEndEdit.AddListener(SearchForFriends);
            friendsSearchButton.onClick.AddListener(() => friendsSearchPanel.gameObject.SetActive(true));
        }
    }
    
    /// <summary>
    /// Get Friends and Display them
    /// </summary>
    private void GetFriends()
    {
        //Cleanup First
        LoopThroughTransformAndDestroy(friendsDisplayPanel.transform, friendsDisplayPlaceholder);

        AccelBytePlugin.GetLobby().LoadFriendsList(result =>
        { 
            //Check this is not an error
            if (!result.IsError)
            {
                //Check if no friends were returned
                if (result.Value.friendsId.Length <= 0)
                {
                    //Display the Placeholder Text
                    friendsDisplayPlaceholder.gameObject.SetActive(true);
                    return;
                }
                //Hide the Placeholder Text
                friendsDisplayPlaceholder.gameObject.SetActive(false);

                
                //Fire off Requests to create UI for each friend
                Debug.Log("Loaded Friends List Succesfully");
                foreach (string friendID in result.Value.friendsId)
                {
                    Debug.Log($"Friend : {friendID}");
                    AccelBytePlugin.GetUser().GetUserByUserId(friendID, x =>
                    {
                        CreateFriendUI(x.Value);
                    });
                }    
            }
            else
            {
                //Display the Placeholder
                friendsDisplayPlaceholder.gameObject.SetActive(true);
                Debug.LogWarning("Error in Getting Friends");
            }
            
        });
    }

    /// <summary>
    /// Create Friend Prefab With Player Detail
    /// </summary>
    /// <param name="userData">Player Detail</param>
    private void CreateFriendUI(PublicUserData userData)
    {
        FriendStatusPanel panel = Instantiate(friendsDisplayPrefab, friendsDisplayPanel).GetComponent<FriendStatusPanel>();
        panel.Create(userData);
        if (!friendUIDictionary.ContainsKey(userData.userId))
        {
            friendUIDictionary.Add(userData.userId, panel);
        }

        panel.SetupButton();
    }

    private void DisplaySearch()
    {
        friendsSearchPanel.gameObject.SetActive(true);  LoopThroughTransformAndDestroy(friendsSearchScrollView.transform, friendsSearchPlaceholder);
        friendsSearchPlaceholder.gameObject.SetActive(true);
    }
    
    private void SearchForFriends(string query)
    {
        AccelBytePlugin.GetUser().SearchUsers(query, result =>
        {
            if (!result.IsError)
            {
                ListQueriedusers(result.Value);
            }
            else
            {
                Debug.LogWarning($"Unable to Query Users Code: {result.Error.Code}, Message: {result.Error.Message}");
            }
        });
    }

    private void ListQueriedusers(PagedPublicUsersInfo pagedInfo)
    {
        //Cleanup First
        LoopThroughTransformAndDestroy(friendsSearchScrollView.transform, friendsSearchPlaceholder);

        if (pagedInfo.data.Length <=0)
        {
            friendsSearchPlaceholder.gameObject.SetActive(true);
        }
        else
        {
            friendsSearchPlaceholder.gameObject.SetActive(false);
            foreach (PublicUserInfo info in pagedInfo.data)
            {
                FriendsAddPanel addPanel = Instantiate(addFriendsPrefab,friendsSearchScrollView).GetComponent<FriendsAddPanel>();
                addPanel.Create(info);
            }
        }
    }

    private void DisplayPending()
    {
        //Cleanup First, remove all Children from the Contents OTHER than the Placeholders
        LoopThroughTransformAndDestroy(pendingIncomingRequestsContent.transform, pendingIncomingRequestPlaceholder);
        LoopThroughTransformAndDestroy(pendingOutgoingRequestsContent.transform, pendingOutgoingRequestPlaceholder);

        //Get all Incoming Friend Requests
        AccelBytePlugin.GetLobby().ListIncomingFriends(result =>
        {
            //Check for an Error
            if (result.IsError)
            {
                Debug.LogWarning($"Unable to get Incoming Requests Code: {result.Error.Code}, Message: {result.Error.Message}");
                //Set the Placeholder Text to be Active so it doesn't just look broken
                pendingIncomingRequestPlaceholder.gameObject.SetActive(true);
            }
            else
            {
                //If there are Zero Incoming Requests, set the PlaceHolder to be active
                if (result.Value.friendsId.Length <= 0)
                {
                    pendingIncomingRequestPlaceholder.gameObject.SetActive(true);
                }
                //Otherwise set the PlaceHolder to be inactive
                else
                {
                    pendingIncomingRequestPlaceholder.gameObject.SetActive(false);
                }
                
                //Loop through all the UserID's returned by the Friends callback and get their PublicUserData
                foreach (string userID in result.Value.friendsId)
                {
                    //Request the PublicUserData for the specific Friend
                    AccelBytePlugin.GetUser().GetUserByUserId(userID, userResult =>
                    {
                        //If it's an Error, report it and do nothing else
                        if (userResult.IsError)
                        {
                            Debug.LogWarning($"Unable to User Code: {userResult.Error.Code}, Message: {userResult.Error.Message}");
                        }
                        //If we have valid data, Instantiate the Prefab for the specific UI Piece and call relevant functions
                        else
                        {
                            FriendsIncomingPanel incomingPanel = Instantiate(pendingIncomingRequestsPrefab, pendingIncomingRequestsContent).GetComponent<FriendsIncomingPanel>();
                            //Pass the PublicUserData into this function
                            incomingPanel.Create(userResult.Value);
                            
                        }
                    });
                }
            }
        });
        
        AccelBytePlugin.GetLobby().ListOutgoingFriends(result =>
        {
            if (result.IsError)
            {
                Debug.LogWarning($"Unable to get Outgoing Requests Code: {result.Error.Code}, Message: {result.Error.Message}");
            }
            else
            {
                if (result.Value.friendsId.Length <= 0)
                {
                    pendingOutgoingRequestPlaceholder.gameObject.SetActive(true);
                }
                else
                {
                    pendingOutgoingRequestPlaceholder.gameObject.SetActive(false);
                }
                foreach (string userID in result.Value.friendsId)
                {
                    AccelBytePlugin.GetUser().GetUserByUserId(userID, userResult =>
                    {
                        if (userResult.IsError)
                        {
                            Debug.LogWarning($"Unable to User Code: {userResult.Error.Code}, Message: {userResult.Error.Message}");
                        }
                        else
                        {
                            FriendsOutgoingPanel outgoingPanel = Instantiate(pendingOutgoingRequestsPrefab, pendingOutgoingRequestsContent).GetComponent<FriendsOutgoingPanel>();
                            outgoingPanel.Create(userResult.Value);
                        }
                    
                    });
                     
                }
            }
        });
        
    }

    private void DisplayBlocked()
    {
        //Cleanup First
        LoopThroughTransformAndDestroy(blockedContent.transform,blockedDisplayPlaceholder);
        
        //Get Blocked List
        AccelBytePlugin.GetLobby().GetListOfBlockedUser(result =>
        {
            //Check for an Error
            if (result.IsError)
            {
                Debug.LogWarning($"Unable to get Blocked Player List Code: {result.Error.Code}, Message: {result.Error.Message}");

                blockedDisplayPlaceholder.gameObject.SetActive(true);
                
            }
            else
            {

                if (result.Value.data.Length <= 0) 
                {
                    blockedDisplayPlaceholder.gameObject.SetActive(true);
                    return;
                }

                blockedDisplayPlaceholder.gameObject.SetActive(false);
                //Loop through all the UserID's returned by the callback and get their PublicUserData
                foreach (BlockedData blockedUser in result.Value.data)
                {
                    //Request the PublicUserData for the specific User
                    AccelBytePlugin.GetUser().GetUserByUserId(blockedUser.blockedUserId, userResult =>
                    {
                        //If it's an Error, report it and do nothing else
                        if (userResult.IsError)
                        {
                            Debug.LogWarning($"Unable to User Code: {userResult.Error.Code}, Message: {userResult.Error.Message}");
                        }
                        //If we have valid data, Instantiate the Prefab for the specific UI Piece and call relevant functions
                        else
                        {
                            Debug.LogWarning($"You blocked: {userResult.Value.displayName}");
                            CreateBlockedUI(userResult.Value);
                        }
                    
                    });
                     
                }
            }
        });
        
        
    }

    /// <summary>
    /// Create Blocked Prefab With Player Detail
    /// </summary>
    /// <param name="userData">Player Detail</param>
    private void CreateBlockedUI(PublicUserData userData)
    {
        FriendsBlockedPanel blockedPanel = Instantiate(blockedUserPrefab, blockedContent).GetComponent<FriendsBlockedPanel>();
        //Pass the PublicUserData into this function
        blockedPanel.Create(userData);
    }


    /// <summary>
    /// A utility function to Destroy all Children of the parent transform. Optionally do not remove a specific Transform
    /// </summary>
    /// <param name="parent">Parent Object to destroy children</param>
    /// <param name="doNotRemove">Optional specified Transform that should NOT be destroyed</param>
    private static void LoopThroughTransformAndDestroy(Transform parent, Transform doNotRemove = null)
    {
        //Loop through all the children and add them to a List to then be deleted
        List<GameObject> toBeDeleted = new List<GameObject>();
        foreach (Transform t in parent)
        {
            //except the Do Not Remove transform if there is one
            if (t != doNotRemove)
            {
                toBeDeleted.Add(t.gameObject);
            }
        }
        //Loop through list and Delete all Children
        for (int i = 0; i < toBeDeleted.Count; i++)
        {
            Destroy(toBeDeleted[i]);
        }
    }

    /// <summary>
    /// Reset The Panel UI State to its default state which only contain the Placeholder Text
    /// </summary>
    /// <param name="parent">The Selected Panel's Transform which is parent of the prefab panels</param>
    /// <param name="panelMode">The Current Selected PanelMode</param>
    public void ResetPanelState(Transform parent, PanelMode panelMode)
    {
        Debug.Log("Refresh in progress");

        // If the Placeholder Text is the only child left in the Panel after Destroy() executed
        if (parent.transform.childCount - 1 == 1)
        {
            switch (panelMode)
            {
                case PanelMode.Incoming:
                    pendingIncomingRequestPlaceholder.gameObject.SetActive(true);
                    break;
                case PanelMode.Outgoing:
                    pendingOutgoingRequestPlaceholder.gameObject.SetActive(true);
                    break;
                case PanelMode.Blocked:
                    blockedDisplayPlaceholder.gameObject.SetActive(true);
                    break;
            }
        }
    }
}
