// Copyright (c) 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using UnityEngine;
using UnityEngine.UI;

public class OrderPricePanel : MonoBehaviour
{
    [SerializeField]
    private Text orderItemCurrencyCodeText;
    [SerializeField]
    private Text orderItemPrice;

    public void Setup(string currencyCode, string price)
    {
        orderItemCurrencyCodeText.text = currencyCode;
        orderItemPrice.text = price;
    }
}
