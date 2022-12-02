// Copyright (c) 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using AccelByte.Api;
using AccelByte.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QoSHandler : MonoBehaviour
{
    [SerializeField]
    private Dropdown regionDropdown;
    [SerializeField]
    private Button refreshButton;

    // AccelByte's Multi Registry references
    private QosManager qos;
    private Lobby lobby;

    // Store default region name and waiting time to refresh latency automatically
    private const string DefaultRegion = "Default Region";
    private const float RefreshTime = 120.0f;

    // Hold a value to prevent refresh latency automatically
    private static bool isWaiting = false;

    private bool isInitialized = false;

    // Store available region and current latency
    private Dictionary<string, int> regionDict;

    private void OnEnable()
    {
        // AccelByte's Multi Registry initialization
        ApiClient apiClient = MultiRegistry.GetApiClient();
        qos = apiClient.GetApi<QosManager, QosManagerApi>();
        lobby = apiClient.GetApi<Lobby, LobbyApi>();

        // Reset boolean when object changes into enable
        isWaiting = false;
    }

    private void Update()
    {
        // Daily refresh the latency
        if (isActiveAndEnabled && lobby.IsConnected && !isWaiting)
        {
            StartCoroutine(DailyRefreshLatency());
        }
    }

    public void SetupQoS()
    {
        if (isInitialized) return;
        isInitialized = true;

        refreshButton.onClick.AddListener(() => 
        {
            // Prevent spamming the button
            refreshButton.interactable = false;

            GetLatency();
        });

        GetLatency();
    }

    /// <summary>
    /// Get the up-to-date latency automatically
    /// </summary>
    private IEnumerator DailyRefreshLatency()
    {
        // Prevent the get latency from being called when the Async is not finished yet
        isWaiting = true;

        // Wait to call get latency
        yield return new WaitForSeconds(RefreshTime);

        GetLatency();

        // Set boolean so it can be called again in the Update
        isWaiting = false;
    }

    /// <summary>
    /// Get the available region and current latency
    /// </summary>
    public void GetLatency()
    {
        qos.GetServerLatencies(result => 
        {
            if (!result.IsError)
            {
                // Avoid to call the rest of the function if the object is not active/ disable
                if (!isActiveAndEnabled) return;

                string activeOption = "";
                int activeValue = 0;

                // Get active option and store selected dropdown value
                // This needed because the order of dictionary from endpoint can be different
                if (regionDropdown.value != 0)
                {
                    activeOption = regionDropdown.options[regionDropdown.value].text;
                }

                // Reset dropdown value
                regionDropdown.ClearOptions();

                // Reset dictionary and add the default region
                regionDict = new Dictionary<string, int>();
                regionDict.Add(DefaultRegion, 0);

                // Reset list for the dropdown options and add the default region
                List<string> dropdownList = new List<string>();
                dropdownList.Add(DefaultRegion);

                // Add the region and latency in dropdown and dictionary
                foreach (var dict in result.Value)
                {
                    regionDict.Add(dict.Key, dict.Value);
                    dropdownList.Add($"{dict.Key} ({dict.Value})");

                    // Check the value that same as selected value before
                    if (activeOption.Contains(dict.Key))
                    {
                        activeValue = dropdownList.Count - 1;
                    }
                }

                // Add the dropdown list into UI
                regionDropdown.AddOptions(dropdownList);

                // Reassign the selected dropdown value
                regionDropdown.value = activeValue;
            }

            // enable the refresh button
            refreshButton.interactable = true;
        });
    }

    /// <summary>
    /// Get the dictionary of region and latency that selected in the region dropdown.
    /// If Default Region is selected, it will select all the region
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, int> GetChoosenLatency()
    {
        string value = regionDropdown.options[regionDropdown.value].text;

        Dictionary<string, int> dict = new Dictionary<string, int>();

        // Return all region and latency except the default region
        if (regionDropdown.value == 0)
        {
            dict = regionDict;
            dict.Remove(DefaultRegion);

            return dict;
        }

        // Remove unselected region
        foreach (var region in regionDict)
        {
            if (value.Contains(region.Key))
            {
                dict.Add(region.Key, region.Value);

                break;
            }
        }

        return dict;
    }
}
