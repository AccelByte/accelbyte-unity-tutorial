// Copyright (c) 2021 - 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using Mirror;
using System;
using UnityEngine;

public class WatchTimeNetworkManager : NetworkManager
{
    [SerializeField]
    private GameplayManager GameplayManager;

    public override void Start()
    {
        base.Start();

#if !UNITY_SERVER
        // Change ip and port based on DS info in the client
        networkAddress = ConnectionHandler.ip;
        gameObject.GetComponent<kcp2k.KcpTransport>().Port = ConnectionHandler.uPort;

        // Auto start the client connection
        StartClient();
#endif
    }

    #region Client System Callbacks

    /// <summary>
    /// Called on the client when connected to a server.
    /// <para>The default implementation of this function sets the client as ready and adds a player. Override the function to dictate what happens when the client connects.</para>
    /// </summary>
    /// <param name="conn">Connection to the server.</param>
    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        GameplayManager.OnPlayerStarted();
    }
    #endregion

    #region Start & Stop Callbacks

    /// <summary>
    /// Called when a server is started - including when a host is started.
    /// <para>StartServer has multiple signatures, but they all cause this hook to be called.</para>
    /// </summary>
    public override void OnStartServer()
    {
        base.OnStartServer();

        GameplayManager.OnAccelByteServerStarted(transport.ServerUri().Port);
        GameplayManager.OnServerStarted();
    }
    #endregion

    /// <summary>
    /// Called when the server stop the client connections
    /// </summary>
    public override void OnStopClient()
    {
        base.OnStopClient();
        GameplayManager.OnPlayerDisconnected();
    }
}
