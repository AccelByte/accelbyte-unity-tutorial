// Copyright (c) 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using AccelByte.Api;
using AccelByte.Models;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class ItemDisplayPanel : MonoBehaviour
{
    [SerializeField]
    private Image itemPicture;
    [SerializeField]
    private Text itemNameText;
    [SerializeField]
    private Text itemDescriptionText;

    [SerializeField]
    private GameObject virtualCoinPanel;
    [SerializeField]
    private Text virtualCoinPriceText;
    [SerializeField]
    private Text usdPriceText;
    
    [SerializeField]
    private Button buyButton;

    private ItemInfo itemInfo;


    /// <summary>
    /// Update Item Infomation to the Item Display's UI
    /// </summary>
    /// <param name="info"> item information</param>
    public void Create(ItemInfo info, Action onBuyCallback)
    {
        itemInfo = info;

        // Get Item Image from url
        if (!gameObject.activeInHierarchy) return;

        StartCoroutine(ABUtilities.DownloadTexture2D(info.images?[0].smallImageUrl, result =>
        {
            itemPicture.sprite = Sprite.Create(result.Value, new Rect(0f, 0f, 64f, 64f), Vector2.zero);
        }));

        itemNameText.text = itemInfo.name;
        itemDescriptionText.text = itemInfo.description;

        ResetPricePanel();
        foreach (RegionDataItem itemData in itemInfo.regionData)
        {
            if (itemData.currencyType == "VIRTUAL")
            {
                // set active necessary text object
                virtualCoinPanel.SetActive(true);

                // set the price text
                virtualCoinPriceText.text = itemData.price.ToString();
            }

            if (itemData.currencyType == "REAL")
            {
                usdPriceText.gameObject.SetActive(true);

                // set the price text
                usdPriceText.text = "USD " + (itemData.price / 100).ToString("F2");
            }
        }

        // set up UI listener
        buyButton.onClick.AddListener(onBuyCallback.Invoke);
    }

    /// <summary>
    /// Set the all the Price Panel gameobjects to false
    /// </summary>
    private void ResetPricePanel()
    {
        virtualCoinPanel.SetActive(false);
        usdPriceText.gameObject.SetActive(false);
    }
}
