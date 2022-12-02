// Copyright (c) 2021 - 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyInvitationPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject invitationPopUp;
    [SerializeField]
    private Text invitationText;
    [SerializeField]
    private Button acceptInvitationButton;
    [SerializeField]
    private Button rejectInvitationButton;

    // AccelByte's Multi Registry references
    private User user;
    private Lobby lobby;

    private void OnEnable()
    {
        // AccelByte's Multi Registry initialization
        ApiClient apiClient = MultiRegistry.GetApiClient();
        user = apiClient.GetApi<User, UserApi>();
        lobby = apiClient.GetApi<Lobby, LobbyApi>();
    }

    /// <summary>
    /// Setup PartyInvitation's Popup UI and event listener 
    /// </summary>
    /// <param name="partyInvitation"> contains Party Invitation's data, consists of sender's userId, partyId, and invitationToken</param>
    public void Setup(PartyInvitation partyInvitation)
    {
        user.GetUserByUserId(partyInvitation.from, result => 
        {
            invitationText.text = result.Value.displayName + " invite you to join their party\nAccept invitation?";
        });
        
        acceptInvitationButton.onClick.AddListener(() => { JoinParty(partyInvitation);});
        rejectInvitationButton.onClick.AddListener(() => { RejectPartyInvitation(partyInvitation);});
    }

    /// <summary>
    /// Accept the invitation and join the party
    /// </summary>
    /// <param name="partyInvitation"> contains Party Invitation's data, consists of sender's userId, partyId, and invitationToken</param>
    public void JoinParty(PartyInvitation partyInvitation)
    {
        lobby.JoinParty(partyInvitation.partyID, partyInvitation.invitationToken, result => 
        {
            if (result.IsError)
            {
                Debug.Log($"Failed to join party: error code: {result.Error.Code} message: {result.Error.Message}");
            }
            else
            {
                Debug.Log("Successfully join a party");
            }
        });

        Destroy(invitationPopUp);
    }

    /// <summary>
    /// Reject the party invitation
    /// </summary>
    /// <param name="partyInvitation"> contains Party Invitation's data, consists of sender's userId, partyId, and invitationToken</param>
    public void RejectPartyInvitation(PartyInvitation partyInvitation)
    {
        lobby.RejectPartyInvitation(partyInvitation.partyID, partyInvitation.invitationToken, result => 
        {
            if (result.IsError)
            {
                Debug.Log($"Failed to reject party invitation: error code: {result.Error.Code} message: {result.Error.Message}");
            }
            else
            {
                Debug.Log("Successfully reject a party invitation");
            }
        });

        Destroy(invitationPopUp);
    }
}
