// Copyright (c) 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using AccelByte.Api;
using AccelByte.Models;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EntitlementHandler : MonoBehaviour
{
    public GameObject EntitlementWindow;

    [SerializeField]
    private Button backToMainMenuButton;

    [SerializeField]
    private Transform entitlementsContent;

    [SerializeField]
    private GameObject entitlementsItemsPrefab;

    /// <summary>
    /// Set Up Inventory UI
    /// </summary>
    public void Setup()
    {
        GetEntitlementItems();
        backToMainMenuButton.onClick.AddListener(() =>
        {
            EntitlementWindow.SetActive(false);
            backToMainMenuButton.onClick.RemoveAllListeners();
            GetComponent<MenuHandler>().Menu.gameObject.SetActive(true);
        });
    }

    /// <summary>
    /// Get The User Entitlements From Server (Admin Portal)
    /// </summary>
    public void GetEntitlementItems()
    {
        // Remove all the children content inside Goods Content
        foreach (Transform entitlementsContentChild in entitlementsContent)
        {
            Destroy(entitlementsContentChild.gameObject);
        }

        // Request & retrieve the user entitlements
        AccelBytePlugin.GetEntitlement().QueryUserEntitlements("", "", 0, 99, EntitlementsResult =>
        {
            if (EntitlementsResult.IsError)
            {
                Debug.Log($"Failed to get entitlement: {EntitlementsResult.Error.Message}, code: {EntitlementsResult.Error.Code}");
            }
            else
            {
                Debug.Log($"Success to get entitlement");

                // Take the detail information of each entitlement
                foreach (EntitlementInfo entitlement in EntitlementsResult.Value.data)
                {
                    GetEntitlementDetails(entitlement);
                }
            }
        });
    }

    /// <summary>
    /// Get The Detail Information From Entitlement
    /// </summary>
    /// <param name="entitlement"> Contains General Information of Entitlement </param>
    public void GetEntitlementDetails(EntitlementInfo entitlement)
    {
        const string language = "en";
        const string region = "US";

        //Request & retrieve the information of entitlement's goods by entitlement id
        AccelBytePlugin.GetItems().GetItemById(entitlement.itemId, region, language, GoodsDetailResult =>
        {
            if (GoodsDetailResult.IsError)
            {
                Debug.Log($"Failed to get entitlement detail: {GoodsDetailResult.Error.Message}, code: {GoodsDetailResult.Error.Code}");
            }
            else
            {
                Debug.Log($"Success to get entitlement detail");

                // Display the detail information of entitlement's goods to the UI
                EntitlementItemsDisplayPanel itemPanel = Instantiate(entitlementsItemsPrefab, entitlementsContent).GetComponent<EntitlementItemsDisplayPanel>();
                itemPanel.Create(GoodsDetailResult.Value, entitlement.useCount, entitlement.id);
            }
        });
    }
}
