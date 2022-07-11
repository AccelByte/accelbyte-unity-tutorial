// Copyright (c) 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using UnityEngine;
using UnityEngine.UI;
using AccelByte.Api;
using AccelByte.Models;
using System.Collections.Generic;
using AccelByte.Core;

public enum ChatType
{
    Global,
    Party,
    Private
}

public class ChatHandler : MonoBehaviour
{
    [SerializeField]
    private Text chatBoxText;
    [SerializeField]
    private InputField chatInputField;
    [SerializeField]
    private Button sendButton;

    [SerializeField]
    private Transform tabPanel;
    [SerializeField]
    private Button leftArrowTabButton;
    [SerializeField]
    private Button rightArrowTabButton;
    [SerializeField]
    private ChatTabButton tabPrefab;

    // Hold the current active user id for private chat
    [HideInInspector]
    public string activeId;

    private bool isInitialized = false;

    // Set the maximum chat on the list
    private const int MaxChatList = 20;

    private Dictionary <string, List<string>> chatListDict;
    private Dictionary<string, string> playerDict;

    private List<ChatTabButton> tabList;

    /// <summary>
    /// Private Instance
    /// </summary>
    private ChatType _chatType;
    public ChatType chatType
    {
        get => _chatType;
        set
        {
            sendButton.onClick.RemoveAllListeners();
            _chatType = value;

            // Add listener for send button based on the current value
            switch (value)
            {
                case ChatType.Global:
                    sendButton.onClick.AddListener(OnSendGlobalChat);
                    break;
                case ChatType.Party:
                    sendButton.onClick.AddListener(OnSendPartyChat);
                    break;
                case ChatType.Private:
                    sendButton.onClick.AddListener(OnSendPrivateChat);
                    break;
                default:
                    break;
            }
        }
    }

    public void SetupChat()
    {
        if (isInitialized) return;
        isInitialized = true;

        playerDict = new Dictionary<string, string>();
        chatListDict = new Dictionary<string, List<string>>();
        tabList = new List<ChatTabButton>();

        // Join default chat channel to enable global chat
        AccelBytePlugin.GetLobby().JoinDefaultChatChannel(result => 
        {
            if (!result.IsError)
            {
                Debug.Log("Join Default Chat Channel is successfully");
            }
        });

        rightArrowTabButton.onClick.AddListener(MoveNextTab);
        leftArrowTabButton.onClick.AddListener(MovePreviousTab);

        // Start default chat into global chat
        CreateTab(ChatType.Global, ChatType.Global.ToString(), ChatType.Global.ToString());

        // Add party button if user is in party
        if (LobbyHandler.Instance.partyHandler.partyMembers != null)
        {
            AddPartyTabButton();
        }
    }

    #region Send Button Functions
    /// <summary>
    /// Send a chat to channel/ global channel
    /// </summary>
    private void OnSendGlobalChat()
    {
        // Prevent sending an empty message
        if (string.IsNullOrEmpty(chatInputField.text)) return;

        AccelBytePlugin.GetLobby().SendChannelChat(chatInputField.text, result => 
        {
            if (!result.IsError)
            {
                Debug.Log("Succesfully Send Global Chat");
                // Reset chat input field
                chatInputField.text = "";
            }
        });
    }

    private void OnSendPrivateChat()
    {
        // Prevent sending an empty message
        if (string.IsNullOrEmpty(chatInputField.text)) return;

        AccelBytePlugin.GetLobby().SendPersonalChat(activeId, chatInputField.text, result =>
        {
            if (!result.IsError)
            {
                Debug.Log("Succesfully Private Chat");

                // Write the current chat to the current user because the notification will be not sent to the sender 
                WriteInChatBox(activeId, $"<color=green>[Private] To {playerDict[activeId]}: {chatInputField.text}</color>");
                // Reset chat input field
                chatInputField.text = "";
            }
        });
    }

    private void OnSendPartyChat()
    {
        // Prevent sending an empty message
        if (string.IsNullOrEmpty(chatInputField.text)) return;

        AccelBytePlugin.GetLobby().SendPartyChat(chatInputField.text, result => 
        {
            if (!result.IsError)
            {
                Debug.Log("Succesfully Send Party Chat");

                // Get current user id and display name
                string myUserId = AccelBytePlugin.GetUser().Session.UserId;
                string myName = LobbyHandler.Instance.partyHandler.partyMembers[myUserId];

                // Write the current chat to the current user because the notification will be not sent to the sender 
                WriteInChatBox(ChatType.Party.ToString(), $"<color=blue>[Party] {myName}: {chatInputField.text}</color>");
                // Reset chat input field
                chatInputField.text = "";
            }
        });
    }
    #endregion

    #region Notification
    /// <summary>
    /// Called when receive a channel/ global chat
    /// </summary>
    /// <param name="result"> Contains data of sender/ from, payload, channel slug, and sent at</param>
    public void OnGlobalChatReceived(ChannelChatMessage result)
    {
        string from = result.from;
        string msg = result.payload;

        GetUserDisplayName(from, getResult =>
        {
            WriteInChatBox(ChatType.Global.ToString(), $"[Global] {playerDict[from]}: {msg}");
        });
    }

    /// <summary>
    /// Called when receive a private chat
    /// </summary>
    /// <param name="result"> Contains data of sender/ from, receiver/ to, payload, received at, etc</param>
    public void OnPrivateChatReceived(ChatMessage result)
    {
        string from = result.from;
        string msg = result.payload;

        GetUserDisplayName(from, getResult =>
        {
            WriteInChatBox(from, $"<color=green>[Private] From {playerDict[from]}: {msg}</color>");
        });
    }

    /// <summary>
    /// Called when receive a party chat
    /// </summary>
    /// <param name="result"> Contains data of sender/ from, receiver/ to, payload, received at, etc</param>
    public void OnPartyChatReceived(ChatMessage result)
    {
        string from = result.from;
        string msg = result.payload;

        GetUserDisplayName(from, getResult => 
        {
            WriteInChatBox(ChatType.Party.ToString(), $"<color=blue>[Party] {playerDict[from]}: {msg}</color>");
        });
    }
    #endregion

    /// <summary>
    /// Add party tab to the tab panel
    /// </summary>
    public void AddPartyTabButton()
    {
        // Setup the chat if it is called in the friend menu to prevent some errors
        SetupChat();

        if (tabList[1] != null) return;

        CreateTab(ChatType.Party, ChatType.Party.ToString(), ChatType.Party.ToString(), false);
    }

    /// <summary>
    /// Remove party tab from the tab panel
    /// </summary>
    public void RemovePartyTabButton()
    {
        Destroy(tabList[1].gameObject);
        tabList[1] = null;

        if (chatType == ChatType.Party)
        {
            // Invoke Global Chat Tab button to set current active tab
            tabList[0].tabButton.onClick.Invoke();
        }
    }

    /// <summary>
    /// Add private tab from the tab panel
    /// </summary>
    /// <param name="id"> the receiver's user id</param>
    /// <param name="displayName"> the receiver's display name</param>
    /// <param name="isForceOpen"> Force to open the private tab</param>
    public void AddPrivateTabButton(string id, string displayName, bool isForceOpen = true)
    {
        // Setup the chat if it is called in the friend menu to prevent some errors
        SetupChat();

        // Don't add tab button with same user id
        for (int i = 2; i < tabList.Count; i++)
        {
            if (tabList[i]?.GetUserId() == id)
            {
                return;
            }
        }

        // Check if player is not registered in dictionary, add it
        if (!playerDict.ContainsKey(id))
        {
            playerDict.Add(id, displayName);
        }

        CreateTab(ChatType.Private, displayName, id, isForceOpen);
    }

    /// <summary>
    /// Remove private tab button from tab list and destroy game object
    /// </summary>
    /// <param name="id"> User id that will be deleted</param>
    public void RemovePrivateTabButton(string id)
    {
        int count = GetNumberTabWithId(id);

        Destroy(tabList[count].gameObject);
        tabList.RemoveAt(count);


        // Set global chat tab as default if removed button is active
        if (activeId == id && chatType == ChatType.Private)
        {
            tabList[0].tabButton.onClick.Invoke();
        }
    }

    /// <summary>
    /// Reset all the tab buttons into active
    /// </summary>
    public void ResetTabButton()
    {
        foreach (var button in tabList)
        {
            if (button != null)
            {
                button.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Show selected chat to the chat box UI
    /// </summary>
    /// <param name="id"> The user id/ name to select the chat box that will be shown</param>
    public void ShowChatBox(string id)
    {
        // Reset chat box
        chatBoxText.text = "";

        if (!chatListDict.ContainsKey(id)) return;

        // Put all the chat into chat box
        foreach (var chat in chatListDict[id])
        {
            if (string.IsNullOrEmpty(chatBoxText.text))
            {
                chatBoxText.text = chat;
            }
            // Add next line to the chat box
            else
            {
                chatBoxText.text = chatBoxText.text + "\n" + chat;
            }
        }
    }

    /// <summary>
    /// Create tab based on chat type
    /// </summary>
    /// <param name="type"> the chat type for the new tab</param>
    /// <param name="name"> the name or display name for the new tab's text</param>
    /// <param name="id"> the user id for the new private tab only</param>
    /// <param name="isForcedOpen"> Force open selected tab</param>
    private void CreateTab(ChatType type, string name, string id, bool isForcedOpen = true)
    {
        // Create and setup the new tab
        ChatTabButton tab = GameObject.Instantiate(tabPrefab);
        tab.transform.SetParent(tabPanel);
        tab.SetupTab(type, name, id);

        switch (type)
        {
            // Assign global tab
            case ChatType.Global:
                tab.exitButton.gameObject.SetActive(false);

                tabList.Add(tab);
                tabList.Add(null);

                // Set the current active tab into global tab
                break;

            // Assign party tab
            case ChatType.Party:
                tab.exitButton.gameObject.SetActive(false);

                tabList[1] = tab;

                // Set the order after global tab
                tab.GetComponent<RectTransform>().SetSiblingIndex(1);
                break;

            // Assign private tab
            case ChatType.Private:
                // Prevent the private chat exceeding from the UI
                if (tabList.Count == 7)
                {
                    Destroy(tabList[2].gameObject);
                    tabList.RemoveAt(2);
                }

                tabList.Add(tab);
                break;

            default:
                break;
        }

        if (isForcedOpen)
        {
            tab.tabButton.onClick.Invoke();
        }
    }

    /// <summary>
    /// Move to the next tab if available
    /// </summary>
    private void MoveNextTab()
    {
        if (tabList.Count == 2 && tabList[1] == null) return;

        // Get next current tab
        int count = GetNumberTabWithId(activeId) + 1;

        // Skip party tab if it's not available
        if (count == 1 && tabList[1] == null)
        {
            count++;
        }

        // Back to first tab
        if (count >= tabList.Count)
        {
            count = 0;
        }

        tabList[count].tabButton.onClick.Invoke();
    }

    /// <summary>
    /// Move to the previous tab if available
    /// </summary>
    private void MovePreviousTab()
    {
        if (tabList.Count == 2 && tabList[1] == null) return;

        // Get previous current tab
        int count = GetNumberTabWithId(activeId) - 1;

        // Skip party tab if it's not available
        if (count == 1 && tabList[1] == null)
        {
            count--;
        }

        // Back to last tab
        if (count < 0)
        {
            count = tabList.Count - 1;
        }

        tabList[count].tabButton.onClick.Invoke();
    }

    /// <summary>
    /// Write the chat in the chat box
    /// </summary>
    /// <param name="id"> id to recognize chat</param>
    /// <param name="text"> Chat that will be written into chat box</param>
    private void WriteInChatBox(string id, string text)
    {
        if (!chatListDict.ContainsKey(id))
        {
            chatListDict.Add(id, new List<string>());
        }

        // Remove the old chat if it exceed from the max value
        if (chatListDict[id].Count >= MaxChatList)
        {
            chatListDict[id].RemoveAt(0);
        }

        chatListDict[id].Add(text);

        // Show to text box if active and show notification if inactive
        if (activeId == id)
        {
            ShowChatBox(id);
        }
        else
        {
            if (id != ChatType.Party.ToString() && id != ChatType.Global.ToString())
            {
                AddPrivateTabButton(id, playerDict[id], false);
            }
            tabList[GetNumberTabWithId(id)].SetUnread();
        }
    }

    /// <summary>
    /// Get number tab in tab list using user id
    /// </summary>
    /// <param name="id"> Id to search tab</param>
    /// <returns> Number of tab</returns>
    private int GetNumberTabWithId(string id)
    {
        for (int i = 0; i < tabList.Count; i++)
        {
            if (tabList[i]?.GetUserId() == id)
            {
                return i;
            }
        }

        return 0;
    }
    
    /// <summary>
    /// Get the user display name. If it's already in the dictionary, it will use that instead.
    /// </summary>
    /// <param name="id"> The user id to get the display name</param>
    /// <param name="callback"> Contain of display name</param>
    private void GetUserDisplayName(string id, ResultCallback<string> callback)
    {
        // Prevent error when player get chat notification in the main menu
        SetupChat();

        // Return display name from the dictionary if available
        if (playerDict.ContainsKey(id))
        {
            callback.TryOk(playerDict[id]);
            return;
        }

        // Return display name from the endpoint
        AccelBytePlugin.GetUser().GetUserByUserId(id, x =>
        {
            if (!x.IsError)
            {
                playerDict.Add(id, x.Value.displayName);

                callback.TryOk(playerDict[id]);
            }
        });
    }
}
