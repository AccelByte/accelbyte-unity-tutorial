// Copyright (c) 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Collections;
using AccelByte.Models;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class OrderPanel : MonoBehaviour
{
    private enum OrderPanelType
    {
        Detail,
        Loading,
        ResultSuccess,
        ResultFail
    }

    #region UI Binding

    [SerializeField]
    private GameObject orderPricePrefab;
    [SerializeField]
    private GameObject orderPurchasePrefab;
    [SerializeField]
    private GameObject orderDetail;
    [SerializeField] 
    private GameObject orderResult;
    [SerializeField]
    private GameObject orderLoading;
    [SerializeField]
    private GameObject orderResultSuccess;
    [SerializeField]
    private GameObject orderResultFail;
    
    [SerializeField]
    private Text orderItemNameText;
    [SerializeField]
    private Text orderResultItemName;
    [SerializeField]
    private Text orderResultFailText;

    [SerializeField] 
    private Transform orderPriceListPanel;
    [SerializeField] 
    private Transform orderPurchaseButtonListPanel;

    [SerializeField] 
    private Button orderClosePopUpButton;
    [SerializeField] 
    private Button orderResultClosePopUpButton;

    [SerializeField] 
    private Image itemImage;
    [SerializeField] 
    private Image loadingImage;

    #endregion

    private ItemInfo itemInfo;

    private Action successBuyCallback;

    private bool isWaiting;
    
    public void Setup(ItemInfo item)
    {
        itemInfo = item;
        isWaiting = false;

        StartCoroutine(GetItemPicture(itemInfo.images[0].smallImageUrl));

        // reset UI and binding
        SwitchPanel(OrderPanelType.Detail);
        orderDetail.SetActive(true);
        orderItemNameText.text = itemInfo.title;
        orderClosePopUpButton.onClick.RemoveAllListeners();
        orderClosePopUpButton.onClick.AddListener(() =>
        {
            Destroy(this.gameObject);
        });
        orderResultClosePopUpButton.onClick.RemoveAllListeners();
        orderResultClosePopUpButton.onClick.AddListener(() =>
        {
            Destroy(this.gameObject);
        });

        // populate price list and purchase button
        for (ushort i = 0; i < itemInfo.regionData.Length; ++i)
        {
            // format price
            int price = itemInfo.regionData[i].price;
            string priceString =
                itemInfo.regionData[i].currencyType == "REAL" ? (price / 100).ToString("F2") : price.ToString();
            
            OrderPricePanel orderPricePanel =
                Instantiate(orderPricePrefab, orderPriceListPanel).GetComponent<OrderPricePanel>();
            orderPricePanel.Setup(itemInfo.regionData[i].currencyCode, priceString);

            OrderPurchaseButton orderPurchaseButton =
                Instantiate(orderPurchasePrefab, orderPurchaseButtonListPanel).GetComponent<OrderPurchaseButton>();
            orderPurchaseButton.Setup(itemInfo, i,
                () =>
                {
                    isWaiting = false;
                    SwitchPanel(OrderPanelType.ResultSuccess);
                    orderResultItemName.text = itemInfo.title;
                    successBuyCallback.Invoke();
                },
                failureMessage =>
                {
                    isWaiting = false;
                    SwitchPanel(OrderPanelType.ResultFail);
                    orderResultFailText.text = failureMessage;
                },
                () => 
                { 
                    SwitchPanel(OrderPanelType.Loading); 
                    isWaiting = true; 
                    StartCoroutine(AnimateLoading()); 
                });
        }
    }

    /// <summary>
    /// Set callback to be called upon payment success
    /// </summary>
    /// <param name="call"> Callback function </param>
    public void SetSuccessCallback(Action call)
    {
        successBuyCallback = call;
    }

    /// <summary>
    /// Switch displayed panel
    /// </summary>
    /// <param name="orderPanelType"> Panel type enum </param>
    private void SwitchPanel(OrderPanelType orderPanelType)
    {
        orderDetail.SetActive(orderPanelType == OrderPanelType.Detail);
        orderLoading.SetActive(orderPanelType == OrderPanelType.Loading);
        orderResult.SetActive(orderPanelType == OrderPanelType.ResultSuccess || orderPanelType == OrderPanelType.ResultFail);
        orderResultSuccess.SetActive(orderPanelType == OrderPanelType.ResultSuccess);
        orderResultFail.SetActive(orderPanelType == OrderPanelType.ResultFail);
    }
    
    /// <summary>
    /// Get Image from URL
    /// </summary>
    /// <param name="url"> url of the image</param>
    /// <returns></returns>
    IEnumerator GetItemPicture(string url)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        Texture2D itemTexture = DownloadHandlerTexture.GetContent(request);
        itemImage.sprite = Sprite.Create(itemTexture, new Rect(0f, 0f, 64f, 64f), Vector2.zero);
    }

    /// <summary>
    /// Animate the loading each 0.1 second
    /// </summary>
    /// <returns></returns>
    private IEnumerator AnimateLoading()
    {
        while (isWaiting)
        {
            loadingImage.transform.Rotate(0, 0, -45);
            yield return new WaitForSeconds(0.1f);
        }
    }
}
