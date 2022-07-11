// Copyright (c) 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using AccelByte.Models;
using UnityEngine;

public class OrderHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject orderPopUpPrefab;

    [SerializeField]
    private Transform storeTransform;

    /// <summary>
    /// Spawn order popup panel
    /// </summary>
    /// <param name="itemInfo"> item to be purchased </param>
    public void SpawnOrderPopUp(ItemInfo itemInfo)
    {
        OrderPanel orderPanel = Instantiate(orderPopUpPrefab, storeTransform).GetComponent<OrderPanel>();
        orderPanel.Setup(itemInfo);
        orderPanel.SetSuccessCallback(() =>
        {
            GetComponent<WalletHandler>().UpdateWallet();
        });
    }
}
