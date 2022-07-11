// Copyright (c) 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using AccelByte.Api;
using AccelByte.Models;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class EntitlementItemsDisplayPanel : MonoBehaviour
{
    [SerializeField]
    private Image entitlementImage;

    [SerializeField]
    private Text entitlementNameText;

    [SerializeField]
    private Text entitlementDescriptionText;

    [SerializeField]
    private GameObject entitlementStackItemsPanel;

    [SerializeField]
    private Text entitlementQuantityText;

    [SerializeField]
    private Button useButton;

    /// <summary>
    /// Update Goods Information to the Goods Display's UIs
    /// </summary>
    /// <param name="info"> Entitlement/Goods Detail Information </param>
    /// <param name="amount"> Amount of Stack of one Entitlement/Goods </param>
    /// <param name="entitlementId"> Id of one Entitlement/Goods </param>
    public void Create(PopulatedItemInfo info, int amount, string entitlementId)
    {
        // Get Item Image from url
        if (!gameObject.activeInHierarchy) return;

        StartCoroutine(ABUtilities.DownloadTexture2D(info.images?[0].smallImageUrl, result =>
        {
            entitlementImage.sprite = Sprite.Create(result.Value, new Rect(0f, 0f, 64f, 64f), Vector2.zero);
        }));

        entitlementNameText.text = info.name;
        entitlementDescriptionText.text = info.description;

        if (info.entitlementType == EntitlementType.CONSUMABLE)
        {
            entitlementStackItemsPanel.SetActive(true);
            entitlementQuantityText.text = amount.ToString();
            useButton.gameObject.SetActive(true);
        }
        else
        {
            entitlementStackItemsPanel.SetActive(false);
            useButton.gameObject.SetActive(false);
        }

        useButton.onClick.AddListener(() => {
            ConsumeGoods(entitlementId);
        });
    }

    /// <summary>
    /// When the player consume the entitlement/goods
    /// </summary>
    /// <param name="id"> Id of one Entitlement </param>
    private void ConsumeGoods(string id)
    {
        // Request to consume 1 then retrieve the new information of entitlement
        AccelBytePlugin.GetEntitlement().ConsumeUserEntitlement(id, 1, consumeResult =>
        {
            entitlementQuantityText.text = consumeResult.Value.useCount.ToString();

            if (consumeResult.Value.useCount == 0)
            {
                LobbyHandler.Instance.entitlementHandler.GetEntitlementItems();
            }
        });
    }
}