// Copyright (c) 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;

public enum GameMode
{
    None,
    versusOne,
    versusTwo
}

public static class GameModeHandler
{
    // This string is based on mode in Admin Portal
    private const string None = "None"; // This is default and not registered in AP
    private const string VersusOne = "1vs1";
    private const string VersusTwo = "2vs2";

    /// <summary>
    /// Parse Game Mode from enum to string
    /// </summary>
    /// <param name="mode"> Game Mode enum that will be parsed into string</param>
    /// <returns></returns>
    public static string GetString(this GameMode mode)
    {
        switch (mode)
        {
            case GameMode.versusOne:
                return VersusOne;
            case GameMode.versusTwo:
                return VersusTwo;

            case GameMode.None:
            default:
                return None;
        }
    }

    /// <summary>
    /// Parse Game Mode to return total players
    /// </summary>
    /// <param name="mode"> Game Mode enum that will be parsed into total players</param>
    /// <returns></returns>
    public static int GetTotalPlayers(this GameMode mode)
    {
        switch (mode)
        {
            case GameMode.versusOne:
                return 2;
            case GameMode.versusTwo:
                return 4;

            case GameMode.None:
            default:
                return 0;
        }
    }

    /// <summary>
    /// Parse Game Mode from string to GameMode
    /// </summary>
    /// <param name="mode"> Game Mode string that will be parsed into enum</param>
    /// <returns></returns>
    public static GameMode ToGameMode(this string mode)
    {
        switch (mode)
        {
            case VersusOne:
                return GameMode.versusOne;
            case VersusTwo:
                return GameMode.versusTwo;

            case None:
            default:
                return GameMode.None;
        }
    }
}