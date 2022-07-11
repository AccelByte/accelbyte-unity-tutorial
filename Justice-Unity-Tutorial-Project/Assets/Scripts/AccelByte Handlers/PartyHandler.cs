// Copyright (c) 2021 - 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject LobbyWindow;

    [SerializeField]
    private Transform canvasTransform;
    [SerializeField]
    private Transform partyDisplayPanel;

    [SerializeField]
    private GameObject partyInvitationPrefab;

    #region Buttons

    [SerializeField]
    private Button friendsManagementButton;
    [SerializeField]
    private Button createPartyButton;
    [SerializeField]
    private Button leavePartyButton;
    [SerializeField]
    private Button exitButton;

    #endregion

    [SerializeField]
    private Text partyIdText;

    private bool isInitialized = false;

    public Dictionary<string, string> partyMembers { private set; get; }

    private void Start()
    {
        if (AccelBytePlugin.GetLobby().IsConnected)
        {
            AccelBytePlugin.GetLobby().GetPartyInfo(partyInfoResult =>
            {
                // Update PartyID in UI
                partyIdText.text = "PartyID: " + partyInfoResult.Value.partyID;

                ResetPlayerEntryUI();

                // Get all party members data based on _partyUserIds, then update data to UI
                AccelBytePlugin.GetUser().BulkGetUserInfo(partyInfoResult.Value.members, result =>
                {
                    if (result.IsError)
                    {
                        Debug.Log($"Failed to get party member's data: error code: {result.Error.Code} message: {result.Error.Message}");
                    }
                    else
                    {
                        // initialize dictionary
                        partyMembers = new Dictionary<string, string>();

                        // result data's order => reversed order of _partyIserIds
                        int _index = result.Value.data.Length;
                        foreach (BaseUserInfo user in result.Value.data)
                        {
                            _index -= 1;
                            // get transform of PlayerEntryDisplay, which is child of PartyListPanel
                            Transform playerEntryDisplay = partyDisplayPanel.GetChild(_index).transform;

                            if (user.userId == partyInfoResult.Value.leaderID)
                            {
                                // set LeaderStatusIndicator as active
                                Transform leaderStatusIndicator = playerEntryDisplay.GetChild(0).transform;
                                leaderStatusIndicator.gameObject.SetActive(true);
                            }
                            else
                            {
                                if (AccelBytePlugin.GetUser().Session.UserId == partyInfoResult.Value.leaderID)
                                {
                                    // set PartyLeaderButton (promote button) as active, then add listener when onclick button
                                    Transform partyLeaderButton = playerEntryDisplay.GetChild(1).transform;
                                    partyLeaderButton.gameObject.SetActive(true);
                                    partyLeaderButton.GetComponent<Button>().onClick.AddListener(() => 
                                    {
                                        PromotePartyLeader(user.userId);
                                    });

                                    // set KickPartyButton as active, then add listener when onclick button
                                    Transform kickPartyButton = playerEntryDisplay.GetChild(2).transform;
                                    kickPartyButton.gameObject.SetActive(true);
                                    kickPartyButton.GetComponent<Button>().onClick.AddListener(() => 
                                    {
                                        KickParty(user.userId);
                                    });
                                }
                            }

                            // set DisplayNameText as active, then change text to User's Display Name
                            Transform displayNameText = playerEntryDisplay.GetChild(3).transform;
                            displayNameText.gameObject.SetActive(true);
                            displayNameText.GetComponent<Text>().text = user.displayName;

                            partyMembers.Add(user.userId, user.displayName);
                        }
                    }
                });
            });
        }
    }

    /// <summary>
    /// Setup Party UI in Lobby and prepare State
    /// </summary>
    public void SetupParty()
    {
        if (isInitialized) return;
        isInitialized = false;

        friendsManagementButton.onClick.AddListener(() => 
        {
            GetComponent<FriendsManagementHandler>().Setup(FriendsManagementHandler.ExitMode.Lobby);
            LobbyWindow.SetActive(false);
            GetComponent<FriendsManagementHandler>().FriendsManagementWindow.SetActive(true);
        });
        createPartyButton.onClick.AddListener(() => {CreateParty(); });
        leavePartyButton.onClick.AddListener(() => { LeaveParty(); });
        exitButton.onClick.AddListener(() =>
        {
            LobbyWindow.SetActive(false);
            GetComponent<MenuHandler>().Menu.gameObject.SetActive(true);
        });
    }

    /// <summary>
    /// Create party and update partyID to UI
    /// </summary>
    public void CreateParty()
    {
        AccelBytePlugin.GetLobby().CreateParty(result => 
        {
            if (result.IsError)
            {
                Debug.Log($"Failed to create party: error code: {result.Error.Code} message: {result.Error.Message}");
            }
            else
            {
                Debug.Log("Successfully create a party");               
            }
        });
    }

    /// <summary>
    /// Invite friend to party
    /// </summary>
    /// <param name="inviteeUserId"> userId of the user that will received the invitation</param>
    public void InviteToParty(string inviteeUserId)
    {
        AccelBytePlugin.GetLobby().InviteToParty(inviteeUserId, result => 
        {
            if (result.IsError)
            {
                Debug.Log($"Failed to invite user to party: error code: {result.Error.Code} message: {result.Error.Message}");
            }
            else
            {
                Debug.Log("Successfully invite an invitee");
            }
        });
    }

    /// <summary>
    /// Kick user from party
    /// </summary>
    /// <param name="memberUserId"> userId of the member that will be kicked from the party</param>
    public void KickParty(string memberUserId)
    {
        AccelBytePlugin.GetLobby().KickPartyMember(memberUserId, result => 
        {
            if (result.IsError)
            {
                Debug.Log($"Failed to kick user from party: error code: {result.Error.Code} message: {result.Error.Message}");
            }
            else
            {
                Debug.Log($"Successfully kick member {memberUserId} from party");
            }
        });
    }

    /// <summary>
    /// Leave from party
    /// </summary>
    public void LeaveParty()
    {
        AccelBytePlugin.GetLobby().LeaveParty(result => 
        {
            if (result.IsError)
            {
                Debug.Log($"Failed to leave party: error code: {result.Error.Code} message: {result.Error.Message}");
            }
            else
            {
                Debug.Log("Successfully leave from party");

                ResetPartyId();
                ResetPlayerEntryUI();
            }
        });
    }

    /// <summary>
    /// Promote member to be a party leader
    /// </summary>
    /// <param name="memberUserId"> userId of the member that will be promoted as party leader</param>
    public void PromotePartyLeader(string memberUserId)
    {
        AccelBytePlugin.GetLobby().PromotePartyLeader(memberUserId, result =>
        {
            if (result.IsError)
            {
                Debug.Log($"Failed to promote member to be party leader: error code: {result.Error.Code} message: {result.Error.Message}");
                LobbyHandler.Instance.WriteLogMessage($"[Party] Failed to promote {partyMembers[memberUserId]} to be party leader", Color.black);
            }
            else
            {
                Debug.Log("Successfully promote member to be a party leader");
                LobbyHandler.Instance.WriteLogMessage($"[Party] Successfully promote {partyMembers[memberUserId]} to be party leader", Color.black);
            }
        });
    }

    /// <summary>
    /// Display all Party Data to PartyList UI
    /// </summary>
    /// <param name="partyDataNotifResult"> </param>
    public void DisplayPartyData(Result<PartyDataUpdateNotif> partyDataNotifResult)
    {
        // update PartyID in UI
        partyIdText.text = "PartyID: " + partyDataNotifResult.Value.partyId;

        ResetPlayerEntryUI();

        // Setup the party chat button
        LobbyHandler.Instance.chatHandler.AddPartyTabButton();

        // Get all party members data based on _partyUserIds, then update data to UI
        AccelBytePlugin.GetUser().BulkGetUserInfo(partyDataNotifResult.Value.members, result =>
        {
            if (result.IsError)
            {
                Debug.Log($"Failed to get party member's data: error code: {result.Error.Code} message: {result.Error.Message}");
            }
            else
            {
                // initialize dictionary
                partyMembers = new Dictionary<string, string>();

                // result data's order => reversed order of _partyIserIds
                int _index = result.Value.data.Length;
                foreach (BaseUserInfo user in result.Value.data)
                {
                    _index -= 1;
                    // get transform of PlayerEntryDisplay, which is child of PartyListPanel
                    Transform playerEntryDisplay = partyDisplayPanel.GetChild(_index).transform;

                    if (user.userId == partyDataNotifResult.Value.leader)
                    {
                        // set LeaderStatusIndicator as active
                        Transform leaderStatusIndicator = playerEntryDisplay.GetChild(0).transform;
                        leaderStatusIndicator.gameObject.SetActive(true);
                    }
                    else
                    {
                        if (AccelBytePlugin.GetUser().Session.UserId == partyDataNotifResult.Value.leader)
                        {
                            // set PartyLeaderButton (promote button) as active, then add listener when onclick button
                            Transform partyLeaderButton = playerEntryDisplay.GetChild(1).transform;
                            partyLeaderButton.gameObject.SetActive(true);
                            partyLeaderButton.GetComponent<Button>().onClick.AddListener(() => 
                            {
                                PromotePartyLeader(user.userId);
                            });

                            // set KickPartyButton as active, then add listener when onclick button
                            Transform kickPartyButton = playerEntryDisplay.GetChild(2).transform;
                            kickPartyButton.gameObject.SetActive(true);
                            kickPartyButton.GetComponent<Button>().onClick.AddListener(() => 
                            {
                                KickParty(user.userId);
                            });
                        }
                    }

                    // set DisplayNameText as active, then change text to User's Display Name
                    Transform displayNameText = playerEntryDisplay.GetChild(3).transform;
                    displayNameText.gameObject.SetActive(true);
                    displayNameText.GetComponent<Text>().text = user.displayName;

                    partyMembers.Add(user.userId, user.displayName);
                }
            }
        });
    }

    /// <summary>
    /// reset Party ID's UI
    /// </summary>
    public void ResetPartyId()
    {
        partyIdText.text = "PartyID: ###############################";
        partyMembers = null;

        // Remove party chat button
        LobbyHandler.Instance.chatHandler.RemovePartyTabButton();
    }

    /// <summary>
    /// Reset Party List's UI
    /// </summary>
    public void ResetPlayerEntryUI()
    {
        foreach(Transform playerEntryDisplay in partyDisplayPanel)
        {
            // set LeaderStatusIndicator as not active
            Transform leaderStatusIndicator = playerEntryDisplay.GetChild(0).transform;
            leaderStatusIndicator.gameObject.SetActive(false);

            // set PartyLeaderButton (promote button) as not active, then remove all listener on button
            Transform partyLeaderButton = playerEntryDisplay.GetChild(1).transform;
            partyLeaderButton.gameObject.SetActive(false);
            partyLeaderButton.GetComponent<Button>().onClick.RemoveAllListeners();

            // set KickPartyButton as not active, then remove all listener on button
            Transform kickPartyButton = playerEntryDisplay.GetChild(2).transform;
            kickPartyButton.gameObject.SetActive(false);
            kickPartyButton.GetComponent<Button>().onClick.RemoveAllListeners();

            // set DisplayNameText as not active and set value to default text
            Transform displayNameText = playerEntryDisplay.GetChild(3).transform;
            displayNameText.gameObject.SetActive(false);
            displayNameText.GetComponent<Text>().text = "PlayerUsername";
        }
    }

    /// <summary>
    /// Called on update when received a party invitation
    /// </summary>
    /// <param name="partyInvitation"> contains Party Invitation's data, consists of sender's userId, partyId, and invitationToken</param>
    public void InvitePartyNotification(PartyInvitation partyInvitation)
    {
        Debug.Log($"[Party-Notification] Invited by: {partyInvitation.from}");
        PartyInvitationPanel invitationPanel = Instantiate(partyInvitationPrefab, canvasTransform).GetComponent<PartyInvitationPanel>();
        invitationPanel.Setup(partyInvitation);
    }

    /// <summary>
    /// Called on update when kicked from party
    /// </summary>
    public void KickPartyNotification()
    {
        Debug.Log("[Party-Notification] You're kicked from party");
        ResetPartyId();
        ResetPlayerEntryUI();
    }

    /// <summary>
    /// Called on update when friend joined the party
    /// </summary>
    /// <param name="joinNotification"> contains data of the user who just joined the party, consists of user's userId</param>
    public void JoinedPartyNotification(JoinNotification joinNotification)
    {
        Debug.Log("[Party-Notification] Invitee join a party");
        AccelBytePlugin.GetUser().GetUserByUserId(joinNotification.userID, result =>
        {
            if (result.IsError)
            {
                Debug.Log($"Failed to get user data: error code: {result.Error.Code} message: {result.Error.Message}");
            }
            else
            {
                LobbyHandler.Instance.WriteLogMessage($"[Party] {result.Value.displayName} join the party", Color.black);
            }
        });

    }

    /// <summary>
    /// Called on update when friend left the party
    /// </summary>
    /// <param name="leaveNotification"> contains userId's data of the user who just left and the party leader's userId</param>
    public void LeavePartyNotification(LeaveNotification leaveNotification)
    {
        if (leaveNotification.userID != AccelBytePlugin.GetUser().Session.UserId)
        {
            Debug.Log($"{leaveNotification.userID} leave the party");
            LobbyHandler.Instance.WriteLogMessage($"[Party] {partyMembers[leaveNotification.userID]} leave the party", Color.black);
        }
    }
}
