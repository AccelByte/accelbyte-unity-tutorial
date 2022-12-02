// Copyright (c) 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreHandler : MonoBehaviour
{
    public GameObject StoreWindow;

    [SerializeField]
    private Button backToMainMenuButton;

    [SerializeField]
    private Transform tabButtonPanel;
    [SerializeField]
    private Transform categoryContent;

    [SerializeField]
    private GameObject tabButtonPrefab;
    [SerializeField]
    private GameObject itemPrefab;

    private static Button firstCategoryButton;

    private Categories categories;
    private Items items;

    private static bool isInitialized = false;

    private void Start()
    {
        // AccelByte's Multi Registry initialization
        ApiClient apiClient = MultiRegistry.GetApiClient();
        categories = apiClient.GetApi<Categories, CategoriesApi>();
        items = apiClient.GetApi<Items, ItemsApi>();
    }

    /// <summary>
    /// Setup Store UI
    /// </summary>
    public void Setup()
    {
        GetCategories();
        backToMainMenuButton.onClick.AddListener(() =>
        {
            StoreWindow.SetActive(false);
            GetComponent<MenuHandler>().Menu.gameObject.SetActive(true);
        });
    }

    /// <summary>
    /// Get Categories Paths from Admin Portal
    /// </summary>
    public void GetCategories()
    {
        if (isInitialized) 
        {
            // Reset display to the first category when Store opened
            firstCategoryButton.onClick.Invoke();
            return; 
        }

        string language = "en";
        categories.GetRootCategories(language, result => 
        {
            if (result.IsError) 
            {
                Debug.Log($"Failed to get root categories: {result.Error.Message}, code: {result.Error.Code}");
            }
            else
            {
                Debug.Log($"Success to get root categories");

                bool firstItem = true;
                foreach (CategoryInfo category in result.Value)
                {
                    // instantiate category button to UI and update its text
                    Button categoryButton = Instantiate(tabButtonPrefab, tabButtonPanel).GetComponent<Button>();
                    categoryButton.GetComponent<CategoryTabButton>().updateText(category.displayName);

                   
                    // add listener for the current Category Button
                    categoryButton.onClick.AddListener(() =>
                    {
                        GetItems(category.categoryPath);
                    });

                    // save the First Category Button and show the First Category by default
                    if (firstItem)
                    {
                        firstCategoryButton = categoryButton;
                        firstCategoryButton.onClick.Invoke();
                        firstItem = false;
                    }
                }
            }
        });

        isInitialized = true;
    }

    /// <summary>
    /// Get Items Information for specific category
    /// </summary>
    /// <param name="category"> category name of the items</param>
    public void GetItems(string path)
    {
        ItemCriteria itemCriteria = new ItemCriteria
        {
            region = "US",
            language = "en",
            categoryPath = path
        };

        items.GetItemsByCriteria(itemCriteria, result =>
        {
            if (result.IsError)
            {
                Debug.Log($"Failed to get item by criteria: {result.Error.Message}, code: {result.Error.Code}");
            }
            else
            {
                LoopThroughTransformAndDestroy(categoryContent);

                Debug.Log($"Success to get item by criteria");
                foreach (ItemInfo itemInfo in result.Value.data)
                {
                    ItemDisplayPanel itemPanel = Instantiate(itemPrefab, categoryContent).GetComponent<ItemDisplayPanel>();
                    itemPanel.Create(itemInfo, () =>
                    {
                        GetComponent<OrderHandler>().SpawnOrderPopUp(itemInfo);
                    });
                }
            }
        });
    }

    /// <summary>
    /// A utility function to Destroy all Children of the parent transform. Optionally do not remove a specific Transform
    /// </summary>
    /// <param name="parent">Parent Object to destroy children</param>
    /// <param name="doNotRemove">Optional specified Transform that should NOT be destroyed</param>
    private static void LoopThroughTransformAndDestroy(Transform parent, Transform doNotRemove = null)
    {
        //Loop through all the children and add them to a List to then be deleted
        List<GameObject> toBeDeleted = new List<GameObject>();
        foreach (Transform t in parent)
        {
            //except the Do Not Remove transform if there is one
            if (t != doNotRemove)
            {
                toBeDeleted.Add(t.gameObject);
            }
        }
        //Loop through list and Delete all Children
        for (int i = 0; i < toBeDeleted.Count; i++)
        {
            Destroy(toBeDeleted[i]);
        }
    }
}
