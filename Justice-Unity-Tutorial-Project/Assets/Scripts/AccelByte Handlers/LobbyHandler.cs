// Copyright (c) 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using UnityEngine;
using AccelByte.Api;
using AccelByte.Core;

public class LobbyHandler : MonoBehaviour
{
    /// <summary>
    /// Private Instance
    /// </summary>
    static LobbyHandler _instance;

    /// <summary>
    /// The Instance Getter
    /// </summary>
    public static LobbyHandler Instance => _instance;

    /// <summary>
    /// The Instance Getter
    /// </summary>
    private Lobby _lobby;

    public GameObject LobbyWindow;
    
    #region Notification Box
    [Header("Notification Box")]
    [SerializeField]
    private Transform notificationBoxContentView;
    [SerializeField]
    private GameObject logMessagePrefab;
    #endregion

    #region Lobby Objects
    [HideInInspector]
    public PartyHandler partyHandler;
    [HideInInspector]
    public MatchmakingHandler matchmakingHandler;
    [HideInInspector]
    public NotificationHandler notificationHandler;
    [HideInInspector]
    public ChatHandler chatHandler;
    [HideInInspector]
    public QoSHandler qosHandler;
    [HideInInspector]
    public CloudSaveHandler cloudSaveHandler;
    [HideInInspector]
    public StatisticHandler statisticHandler;
    [HideInInspector]
    public EntitlementHandler entitlementHandler;
    [HideInInspector]
    public FriendsManagementHandler friendsHandler;
    [HideInInspector]
    public PresenceHandler presenceHandler;
    #endregion

    private void Awake()
    {
        //Check if another Instance is already created, and if so delete this one, otherwise destroy the object
        if (_instance != null && _instance != this)
        {
            Destroy(this);
            return;
        }
        else
        {
            _instance = this;
        }

        // Get the the object handler
        partyHandler = gameObject.GetComponent<PartyHandler>();
        matchmakingHandler = gameObject.GetComponent<MatchmakingHandler>();
        notificationHandler = gameObject.GetComponent<NotificationHandler>();
        chatHandler = gameObject.GetComponent<ChatHandler>();
        qosHandler = gameObject.GetComponent<QoSHandler>();
        cloudSaveHandler = gameObject.GetComponent<CloudSaveHandler>();
        statisticHandler = gameObject.GetComponent<StatisticHandler>();
        entitlementHandler = gameObject.GetComponent<EntitlementHandler>();
        friendsHandler = gameObject.GetComponent<FriendsManagementHandler>();
        presenceHandler = gameObject.GetComponent<PresenceHandler>();
    }

    private void OnEnable()
    {
        // AccelByte's Multi Registry initialization
        ApiClient apiClient = MultiRegistry.GetApiClient();
        //Get a reference to the instance of the Lobby
        _lobby = apiClient.GetApi<Lobby, LobbyApi>();
    }

    /// <summary>
    /// Disconnect from Lobby if it's connected
    /// </summary>
    public void DisconnectFromLobby()
    {
        if (_lobby.IsConnected)
        {
            _lobby.Disconnect();
        }
    }

    /// <summary>
    /// Connect to the <see cref="Lobby"/> and setup CallBacks
    /// </summary>
    public void ConnectToLobby()
    {
        //Init menu handler
        GetComponent<MenuHandler>().Create();
        GetComponent<MenuHandler>().Menu.gameObject.SetActive(true);
        GetComponent<MenuHandler>().DisplayProfile();

        //Connection
        _lobby.Connected += notificationHandler.OnConnected;
        _lobby.Disconnecting += notificationHandler.OnDisconnecting;
        _lobby.Disconnected += notificationHandler.OnDisconnected;

        //Party
        _lobby.InvitedToParty += notificationHandler.OnInvitedToParty;
        _lobby.JoinedParty += notificationHandler.OnJoinedParty;
        _lobby.KickedFromParty += notificationHandler.OnKickedFromParty;
        _lobby.LeaveFromParty += notificationHandler.OnLeaveFromParty;
        _lobby.RejectedPartyInvitation += notificationHandler.OnRejectedPartyInvitation;
        _lobby.PartyDataUpdateNotif += notificationHandler.OnPartyDataUpdateNotif;

        //Chat
        _lobby.ChannelChatReceived += notificationHandler.OnChannelChatReceived;
        _lobby.PersonalChatReceived += notificationHandler.OnPrivateChatReceived;
        _lobby.PartyChatReceived += notificationHandler.OnPartyChatReceived;

        //Friends
        _lobby.FriendsStatusChanged += notificationHandler.OnFriendsStatusChanged;
        _lobby.FriendRequestAccepted += notificationHandler.OnFriendRequestAccepted;
        _lobby.OnIncomingFriendRequest += notificationHandler.OnIncomingFriendRequest;
        _lobby.FriendRequestCanceled += notificationHandler.OnFriendRequestCanceled;
        _lobby.FriendRequestRejected += notificationHandler.OnFriendRequestRejected;
        _lobby.OnUnfriend += notificationHandler.OnUnfriend;

        //Matchmaking
        _lobby.MatchmakingCompleted += notificationHandler.OnMatchmakingCompleted;
        _lobby.ReadyForMatchConfirmed += notificationHandler.OnReadyForMatchConfirmed;
        _lobby.RematchmakingNotif += notificationHandler.OnRematchmakingNotif;
        _lobby.DSUpdated += notificationHandler.OnDSUpdated;

        //Notifications
        _lobby.OnNotification += notificationHandler.OnNotification;

        // Blocks
        _lobby.PlayerBlockedNotif += notificationHandler.OnPlayerBlockedNotif;
        _lobby.PlayerUnblockedNotif += notificationHandler.OnPlayerUnblockedNotif;

        // Bans
        _lobby.UserBannedNotification += notificationHandler.OnUserBannedNotification;
        
        //Connect to the Lobby
        if (!_lobby.IsConnected)
        {
            _lobby.Connect();
        }
        
    }

    /// <summary>
    /// Remove listener
    /// </summary>
    public void RemoveLobbyListeners()
    {
        //Remove delegate from Lobby
        //Connection
        _lobby.Connected -= notificationHandler.OnConnected;
        _lobby.Disconnecting -= notificationHandler.OnDisconnecting;
        _lobby.Disconnected -= notificationHandler.OnDisconnected;

        //Party
        _lobby.InvitedToParty -= notificationHandler.OnInvitedToParty;
        _lobby.JoinedParty -= notificationHandler.OnJoinedParty;
        _lobby.KickedFromParty -= notificationHandler.OnKickedFromParty;
        _lobby.LeaveFromParty -= notificationHandler.OnLeaveFromParty;
        _lobby.RejectedPartyInvitation -= notificationHandler.OnRejectedPartyInvitation;
        _lobby.PartyDataUpdateNotif -= notificationHandler.OnPartyDataUpdateNotif;

        //Chat
        _lobby.ChannelChatReceived -= notificationHandler.OnChannelChatReceived;
        _lobby.PersonalChatReceived -= notificationHandler.OnPrivateChatReceived;
        _lobby.PartyChatReceived -= notificationHandler.OnPartyChatReceived;

        //Friends
        _lobby.FriendsStatusChanged -= notificationHandler.OnFriendsStatusChanged;
        _lobby.FriendRequestAccepted -= notificationHandler.OnFriendRequestAccepted;
        _lobby.OnIncomingFriendRequest -= notificationHandler.OnIncomingFriendRequest;
        _lobby.FriendRequestCanceled -= notificationHandler.OnFriendRequestCanceled;
        _lobby.FriendRequestRejected -= notificationHandler.OnFriendRequestRejected;
        _lobby.OnUnfriend -= notificationHandler.OnUnfriend;

        //Matchmaking
        _lobby.MatchmakingCompleted -= notificationHandler.OnMatchmakingCompleted;
        _lobby.ReadyForMatchConfirmed -= notificationHandler.OnReadyForMatchConfirmed;
        _lobby.RematchmakingNotif -= notificationHandler.OnRematchmakingNotif;
        _lobby.DSUpdated -= notificationHandler.OnDSUpdated;

        //Notifications
        _lobby.OnNotification -= notificationHandler.OnNotification;

        // Blocks
        _lobby.PlayerBlockedNotif -= notificationHandler.OnPlayerBlockedNotif;
        _lobby.PlayerUnblockedNotif -= notificationHandler.OnPlayerUnblockedNotif;

        // Bans
        _lobby.UserBannedNotification -= notificationHandler.OnUserBannedNotification;
    }

    /// <summary>
    /// Called when player close the application
    /// </summary>
    private void OnApplicationQuit()
    {
        //Attempt to Disconnect from the Lobby when the Game Quits
        DisconnectFromLobby();
    }

    /// <summary>
    /// Write the log message on the notification box
    /// </summary>
    /// <param name="text"> text that will be shown in the party notification</param>
    public void WriteLogMessage(string text, Color color)
    {
        LogMessagePanel logPanel = Instantiate(logMessagePrefab, notificationBoxContentView).GetComponent<LogMessagePanel>();
        logPanel.UpdateNotificationUI(text, color);
    }
}
