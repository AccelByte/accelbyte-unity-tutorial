// Copyright (c) 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using AccelByte.Api;
using AccelByte.Core;
using UnityEngine;
using UnityEngine.UI;

public class WalletHandler : MonoBehaviour
{
    [SerializeField]
    private Text walletText;

    public static int balance;
    private const string currencyCode = "VC";

    private Wallet wallet;

    private void Start()
    {
        // AccelByte's Multi Registry initialization
        ApiClient apiClient = MultiRegistry.GetApiClient();
        wallet = apiClient.GetApi<Wallet, WalletApi>();
    }

    /// <summary>
    /// Get user's wallet information and update balance
    /// </summary>
    public void UpdateWallet()
    {
        wallet.GetWalletInfoByCurrencyCode(currencyCode, result =>
        {
            if (result.IsError)
            {
                Debug.Log($"Failed to get wallet information: {result.Error.Message}, code: {result.Error.Code}");
            }
            else
            {
                Debug.Log($"Success to get wallet information");

                balance = result.Value.balance;
                walletText.text = balance.ToString();
            }
        });
    }
}
