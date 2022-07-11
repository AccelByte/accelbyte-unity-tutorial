// Copyright (c) 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;

public static class ConnectionHandler
{
    // Get/ set ip in string
    public static string ip = "localhost";
    // Get/ set port in integer 
    public static int port = 7777;

    // Get port in ushort format 
    public static ushort uPort => Convert.ToUInt16(port);

    /// <summary>
    /// Get the local command line argument for local test
    /// </summary>
    /// <returns> Return true if local build detected</returns>
    public static bool GetLocalArgument()
    {
        bool isLocal = false;

        // Get Local Argument from the system
        // You can run local by adding -local when executing the game/ server
        string[] args = System.Environment.GetCommandLineArgs();
        foreach (var arg in args)
        {
            if (arg.Contains("local"))
            {
                isLocal = true;
                break;
            }
        }

        return isLocal;
    }
}
