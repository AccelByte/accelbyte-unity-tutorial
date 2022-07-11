// Copyright (c) 2021 - 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using Mirror;
using System.Collections.Generic;

public struct PlayerInfo
{
    public string playerId;
    public string displayName;
    public double playerScoreTime;
    public bool isWin;
    public bool isPartyA;
}

public enum GameplayInterfaceState : byte
{
    None,
    Ready,
    Loading,
    Gameplay,
    Result
}

// Instantiate Struct to interact between server or client
#region Network Message

public struct ServerStartClientMessage : NetworkMessage
{
    public string playerId;
    public string displayName;
}

public struct ServerRequestStopTimerMessage : NetworkMessage { }

public struct ClientStartClientResponseMessage : NetworkMessage { }

public struct ClientUpdateCountdownTimerMessage : NetworkMessage
{
    public int time;
}

public struct ClientChangeToGameplayStateMessage : NetworkMessage
{
    public double targetTime;
}

public struct ClientUpdateMainTimeMessage : NetworkMessage
{
    public double mainTime;
}

public struct ClientOnAllPlayerStopTime : NetworkMessage
{
    public PlayerInfo[] allPlayerInfos;
    public double targetTime;
    public List<string> unlockedAchievementsCodeList;
}

#endregion