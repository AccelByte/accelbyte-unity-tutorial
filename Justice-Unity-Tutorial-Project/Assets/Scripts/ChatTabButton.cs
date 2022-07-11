// Copyright (c) 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using UnityEngine;
using UnityEngine.UI;

public class ChatTabButton : MonoBehaviour
{
    public Button tabButton;
    public Button exitButton;
    public Text tabText;

    private string userId;
    private ChatType type;
    private bool isUnread;

    // Get tab user id
    public string GetUserId()
    {
        return userId;
    }

    // Mark the tab into unread
    public void SetUnread()
    {
        isUnread = true;
        tabButton.GetComponent<Image>().color = Color.green;
    }

    // Setup the tab value
    public void SetupTab(ChatType type, string name, string userId)
    {
        this.type = type;
        this.userId = userId;
        tabText.text = name;

        tabButton.onClick.AddListener(() =>
        {
            LobbyHandler.Instance.chatHandler.chatType = this.type;
            LobbyHandler.Instance.chatHandler.activeId = this.userId;

            LobbyHandler.Instance.chatHandler.ShowChatBox(this.userId);

            if (isUnread)
            {
                isUnread = false;
            }

            // Reset the other button's state to active
            LobbyHandler.Instance.chatHandler.ResetTabButton();
            SetActive(false);
        });

        exitButton.onClick.AddListener(() =>
        {
            LobbyHandler.Instance.chatHandler.RemovePrivateTabButton(this.userId);
        });
    }

    public void SetActive(bool isActive)
    {
        // Set the button into active so it can be clicked
        if (isActive)
        {
            tabButton.interactable = true;

            if (isUnread)
            {
                tabButton.GetComponent<Image>().color = Color.green;
            }
            else
            {
                tabButton.GetComponent<Image>().color = Color.white;
            }
        }
        // Set the button into inactive so it can't be clicked
        else
        {
            tabButton.interactable = false;
            tabButton.GetComponent<Image>().color = Color.grey;
        }
    }
}
