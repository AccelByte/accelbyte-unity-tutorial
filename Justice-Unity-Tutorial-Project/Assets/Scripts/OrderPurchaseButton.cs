// Copyright (c) 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using AccelByte.Api;
using AccelByte.Models;
using UnityEngine;
using UnityEngine.UI;

public class OrderPurchaseButton : MonoBehaviour
{
    [SerializeField]
    private Text orderItemCurrencyCodeText;

    [SerializeField]
    private Button orderItemPurchaseButton;

    public void Setup(ItemInfo itemInfo, ushort regionIndex, Action successCallback, Action<string> failureCallback, Action loadingCallback)
    {
        // reset UI and binding
        orderItemCurrencyCodeText.text = itemInfo.regionData[regionIndex].currencyCode;
        orderItemPurchaseButton.onClick.RemoveAllListeners();
        orderItemPurchaseButton.onClick.AddListener(() => CreateOrder(itemInfo, regionIndex, successCallback, failureCallback, loadingCallback));
    }

    /// <summary>
    /// Create item order
    /// </summary>
    /// <param name="itemInfo"> item information to be purchased </param>
    /// <param name="regionIndex"> determine which currency to use </param>
    /// <param name="successCallback"> callback function on creating order success </param>
    /// <param name="failureCallback"> callback function on creating order failure  </param>
    /// <param name="loadingCallback"> callback function on waiting create order server response </param>
    private void CreateOrder(ItemInfo itemInfo, ushort regionIndex, Action successCallback, Action<string> failureCallback, Action loadingCallback)
    {
        loadingCallback.Invoke();
        
        OrderRequest orderRequest = new OrderRequest()
        {
            currencyCode = itemInfo.regionData[regionIndex].currencyCode,
            discountedPrice = itemInfo.regionData[regionIndex].discountedPrice,
            itemId = itemInfo.itemId,
            language = itemInfo.language,
            price = itemInfo.regionData[regionIndex].price,
            quantity = 1,
            region = itemInfo.region
        };
        
        AccelBytePlugin.GetOrders().CreateOrder(orderRequest, result =>
        {
            if (result.IsError)
            {
                Debug.Log("Fail to create order");
                failureCallback.Invoke(result.Error.Message);
            }
            else
            {
                Debug.Log("Success to create order");
                successCallback.Invoke();


                if (result.Value.currency.currencyCode == "VC")
                {
                    // Update vc spending to statistic
                    LobbyHandler.Instance.statisticHandler.UpdateStatistic(result.Value.price);
                }
            }
        });
    }
}
