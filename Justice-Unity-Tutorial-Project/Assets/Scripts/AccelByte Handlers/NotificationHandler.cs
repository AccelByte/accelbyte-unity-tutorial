// Copyright (c) 2021 - 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using UnityEngine;
using AccelByte.Models;
using AccelByte.Core;

public class NotificationHandler : MonoBehaviour
{
    #region Notifications

    // Collection of connection notifications
    #region Connections
    /// <summary>
    /// Called when lobby is connected
    /// </summary>
    public void OnConnected()
    {
        Debug.Log("Lobby Connected");
    }

    /// <summary>
    /// Called when connection is disconnecting
    /// </summary>
    /// <param name="result"> Contains data of message</param>
    public void OnDisconnecting(Result<DisconnectNotif> result)
    {
        Debug.Log($"Lobby Disconnecting {result.Value.message}");
    }

    /// <summary>
    /// Called when connection is being disconnected
    /// </summary>
    /// <param name="result"> Contains data of websocket close code</param>
    public void OnDisconnected(WsCloseCode result)
    {
        Debug.Log($"Lobby Disconnected: {result}");
    }
    #endregion

    // Collection of party notifications
    #region Party
    /// <summary>
    /// Called when user gets party invitation
    /// </summary>
    /// <param name="result"> Contains data of inviter, party id, and invitation token</param>
    public void OnInvitedToParty(Result<PartyInvitation> result)
    {
        GetComponent<PartyHandler>().InvitePartyNotification(result.Value);
    }

    /// <summary>
    /// Called when user joins to the party
    /// </summary>
    /// <param name="result"> Contains data of joined user id</param>
    public void OnJoinedParty(Result<JoinNotification> result)
    {
        GetComponent<PartyHandler>().JoinedPartyNotification(result.Value);
    }

    /// <summary>
    /// Called when user is kicked by party leader
    /// </summary>
    /// <param name="result"> Contains data of party leader's user id, party id, and kicked user id</param>
    public void OnKickedFromParty(Result<KickNotification> result)
    {
        GetComponent<PartyHandler>().KickPartyNotification();
    }

    /// <summary>
    /// Called when user leaves from the party
    /// </summary>
    /// <param name="result"> Contains data of party leader's user id and leaver user id</param>
    public void OnLeaveFromParty(Result<LeaveNotification> result)
    {
        GetComponent<PartyHandler>().LeavePartyNotification(result.Value);
    }

    /// <summary>
    /// Called when user rejects party invitation
    /// </summary>
    /// <param name="result"> Contains data of party id, party leader's user id, and rejector user id</param>
    public void OnRejectedPartyInvitation(Result<PartyRejectNotif> result)
    {
        Debug.Log("[Party-Notification] Invitee rejected a party invitation");
    }

    /// <summary>
    /// Called when party data is updated
    /// </summary>
    /// <param name="result"> Contains data of updated party</param>
    public void OnPartyDataUpdateNotif(Result<PartyDataUpdateNotif> result)
    {
        GetComponent<PartyHandler>().DisplayPartyData(result);
    }
    #endregion

    // Collection of chat notifications
    #region Chat
    /// <summary>
    /// Called when receive a channel/ global chat
    /// </summary>
    /// <param name="result"> Contains data of sender/ from, payload, channel slug, and sent at</param>
    public void OnChannelChatReceived(Result<ChannelChatMessage> result)
    {
        GetComponent<ChatHandler>().OnGlobalChatReceived(result.Value);
    }

    /// <summary>
    /// Called when receive a private chat
    /// </summary>
    /// <param name="result"> Contains data of sender/ from, receiver/ to, payload, received at, etc</param>
    public void OnPrivateChatReceived(Result<ChatMessage> result)
    {
        GetComponent<ChatHandler>().OnPrivateChatReceived(result.Value);
    }

    /// <summary>
    /// Called when receive a party chat
    /// </summary>
    /// <param name="result"> Contains data of sender/ from, receiver/ to, payload, received at, etc</param>
    public void OnPartyChatReceived(Result<ChatMessage> result)
    {
        GetComponent<ChatHandler>().OnPartyChatReceived(result.Value);
    }
    #endregion

    // Collection of friend notifications
    #region Friends
    /// <summary>
    /// Called when friend status is changed
    /// </summary>
    /// <param name="result"> Contains data of user id, availability, status, etc</param>
    public void OnFriendsStatusChanged(Result<FriendsStatusNotif> result)
    {
        GetComponent<FriendsManagementHandler>().UpdateFriends(result.Value);
    }

    /// <summary>
    /// Called when friend request is accepted
    /// </summary>
    /// <param name="result"> Contains data of friend's user id</param>
    public void OnFriendRequestAccepted(Result<Friend> result)
    {
        Debug.Log($"Accepted a Friend Request from user {result.Value.friendId}");
    }

    /// <summary>
    /// Called when there is incomming friend request
    /// </summary>
    /// <param name="result"> Contains data of friend's user id</param>
    public void OnIncomingFriendRequest(Result<Friend> result)
    {
        Debug.Log($"Received a Friend Request from user {result.Value.friendId}");
    }

    /// <summary>
    /// Called when friend is unfriend
    /// </summary>
    /// <param name="result"> Contains data of friend's user id</param>
    public void OnUnfriend(Result<Friend> result)
    {
        Debug.Log($"Unfriended User {result.Value.friendId}");
    }

    /// <summary>
    /// Called when friend request is canceled
    /// </summary>
    /// <param name="result"> Contains data of sender user id</param>
    public void OnFriendRequestCanceled(Result<Acquaintance> result)
    {
        Debug.Log($"Cancelled a Friend Request from user {result.Value.userId}");
    }

    /// <summary>
    /// Called when friend request is rejected
    /// </summary>
    /// <param name="result"> Contains data of rejector user id</param>
    public void OnFriendRequestRejected(Result<Acquaintance> result)
    {
        Debug.Log($"Rejected a Friend Request from user {result.Value.userId}");
    }
    #endregion

    // Collection of friend notifications
    #region Matchmaking
    /// <summary>
    /// Called when matchmaking is found
    /// </summary>
    /// <param name="result"> Contains data of status and match id</param>
    public void OnMatchmakingCompleted(Result<MatchmakingNotif> result)
    {
        GetComponent<MatchmakingHandler>().MatchmakingCompletedNotification(result.Value);
    }

    /// <summary>
    /// Called when user send ready for match confirmation
    /// </summary>
    /// <param name="result"> Contains data of user id and match id</param>
    public void OnReadyForMatchConfirmed(Result<ReadyForMatchConfirmation> result)
    {
        GetComponent<MatchmakingHandler>().ReadyForMatchConfirmedNotification(result.Value);
    }

    /// <summary>
    /// Called when all user is already confirmed the readiness
    /// </summary>
    /// <param name="result"> Contains data of ds notification</param>
    public void OnDSUpdated(Result<DsNotif> result)
    {
        GetComponent<MatchmakingHandler>().DSUpdatedNotification(result.Value);
    }

    /// <summary>
    /// Called when there is user who not confirm the match
    /// - The party that has a user who did not confirm the match will get banned and need to start matchmaking again
    /// - The other party will start matchmaking automatically if ban duration is zero
    /// </summary>
    /// <param name="result"> Contains data of ban duration</param>
    public void OnRematchmakingNotif(Result<RematchmakingNotification> result)
    {
        GetComponent<MatchmakingHandler>().RematchmakingNotif(result.Value);
    }
    #endregion

    // Collection of notifications
    #region Notifications
    /// <summary>
    /// Called when there is message from notification service
    /// </summary>
    /// <param name="result"> Contains data of notification</param>
    public void OnNotification(Result<Notification> result)
    {

    }
    #endregion
    
    // Collection of block notifications
    #region Blocks
    /// <summary>
    /// Called when user get blocked by other user
    /// </summary>
    /// <param name="result"> Contains data of user id and blocked user id</param>
    public void OnPlayerBlockedNotif(Result<PlayerBlockedNotif> result)
    {
        Debug.Log($"Get blocked by: {result.Value.userId}");
    }

    /// <summary>
    /// Called when user get unblocked by other user
    /// </summary>
    /// <param name="result"> Contains data of user id and unblocked user id</param>
    public void OnPlayerUnblockedNotif(Result<PlayerUnblockedNotif> result)
    {
        Debug.Log($"Get unblocked by: {result.Value.userId}");
    }
    #endregion
    
    // Collection of ban notifications
    #region Bans
    /// <summary>
    /// Called when user get banned
    /// </summary>
    /// <param name="result"> Contains data of ban, reason, enable, etc</param>
    public void OnUserBannedNotification(Result<UserBannedNotification> result)
    {

    }
    #endregion
    #endregion
}
