// Copyright (c) 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Collections;
using System.IO;
using System.Text;
using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class CloudStorageHandler : MonoBehaviour
{
    #region UI binding

    [SerializeField]
    private GameObject galleryPanel;
    [SerializeField]
    private GameObject contentPanel;
    [SerializeField]
    private GameObject contentEmptyPanel;
    
    [SerializeField]
    private Text screenshotKeyText;
    [SerializeField]
    private Text screenshotNameText;

    [SerializeField]
    private Button backToMainMenu;

    [SerializeField]
    private Image screenshotImage;

    #endregion

    // AccelByte's Multi Registry references
    private User user;
    private CloudStorage cloudStorage;

    private const string ScreenshotLabel = "screenshot-label";
    private const string ScreenshotTag = "screenshot-tag";
    private const string ScreenshotDir = "Saved/Screenshot/";
    private const string ScreenshotKeyBind = "f2";
    
    private string screenshotSlotId;
    private string screenshotSlotName;

    private void Start()
    {
        // AccelByte's Multi Registry initialization
        ApiClient apiClient = MultiRegistry.GetApiClient();
        user = apiClient.GetApi<User, UserApi>();
        cloudStorage = apiClient.GetApi<CloudStorage, CloudStorageApi>();
    }

    public void Setup()
    {
        galleryPanel.SetActive(true);
        
        // reset UI and binding
        screenshotKeyText.text = ScreenshotKeyBind;
        backToMainMenu.onClick.RemoveAllListeners();
        backToMainMenu.onClick.AddListener(() =>
        {
            galleryPanel.SetActive(false);
            GetComponent<MenuHandler>().Menu.gameObject.SetActive(true);
        });
        
        CheckImage();
    }
    
    /// <summary>
    /// Check whether the screenshot slot already exist or not, if not, create one.
    /// </summary>
    /// <param name="onSuccess"> Callback upon completion </param>
    private void CheckSlot(Action onSuccess)
    {
        // check if screenshot slot exist or not
        cloudStorage.GetAllSlots(result =>
        {
            if (result.IsError)
            {
                Debug.Log("Fail to get all slots");
            }
            else
            {
                Debug.Log("Success to get all slots");

                foreach (Slot slot in result.Value)
                {
                    if (slot.label == ScreenshotLabel)
                    {
                        screenshotSlotId = slot.slotId;
                        screenshotSlotName = slot.originalName;
                        
                        onSuccess.Invoke();
                        return;
                    }
                }
                
                // if screenshot slot does not exist, create one
                byte[] placeholderByte = Encoding.ASCII.GetBytes("placeholder");
                cloudStorage.CreateSlot(placeholderByte, "", createResult =>
                {
                    if (createResult.IsError)
                    {
                        Debug.Log("Fail to create slot");
                    }
                    else
                    {
                        Debug.Log("Success to create slot");

                        screenshotSlotId = createResult.Value.slotId;
                        screenshotSlotName = createResult.Value.originalName;
                
                        UpdateSlotMetaData();
                
                        onSuccess.Invoke();
                    }
                });
            }
        });
    }
    
    /// <summary>
    /// Upload screenshot data to screenshot slot
    /// </summary>
    /// <param name="screenshotName"> file name to be uploaded </param>
    private void UploadData(string screenshotName)
    {
        CheckSlot(() =>
        {
            string dir = Path.Combine(Application.dataPath, ScreenshotDir, screenshotName);
            
            byte[] imageByte = File.ReadAllBytes(dir);
            cloudStorage.UpdateSlot(screenshotSlotId, imageByte, screenshotName, result =>
            {
                if (result.IsError)
                {
                    Debug.Log("Fail to save data");
                }
                else
                {
                    Debug.Log("Success to save data");

                    UpdateSlotMetaData();
                }
            });
        });
    }
    
    /// <summary>
    /// Update metadata of the screenshot slot in cloud storage
    /// </summary>
    private void UpdateSlotMetaData()
    {
        string[] tags = 
        {
            ScreenshotTag
        };
        cloudStorage.UpdateSlotMetadata(screenshotSlotId, tags, ScreenshotLabel, "", 
            updateResult =>
            {
                if (updateResult.IsError)
                {
                    Debug.Log("Fail to update slot metadata");
                }
                else
                {
                    Debug.Log("Success to update slot metadata");
                }
            });
    }

    /// <summary>
    /// Check if current image on the screenshot slot already exist locally or not
    /// if not, get image
    /// if already exist, load the local one
    /// </summary>
    private void CheckImage()
    {
        CheckSlot(() =>
        {
            bool isImageEmpty = string.IsNullOrEmpty(screenshotSlotName);
            contentPanel.SetActive(!isImageEmpty);
            contentEmptyPanel.SetActive(isImageEmpty);
            
            if (isImageEmpty)
            {
                return;
            }
            
            CheckScreenshotDir();
            
            string dir = Path.Combine(Application.dataPath, ScreenshotDir, screenshotSlotName);

            if (File.Exists(dir))
            {
                Debug.Log("Found latest image in local");
                
                DisplayImage();
                return;
            }
            
            cloudStorage.GetSlot(screenshotSlotId, result =>
            {
                if (result.IsError)
                {
                    Debug.Log("Fail to retrieve data from cloud storage");
                }
                else
                {
                    Debug.Log("Success to retrieve data from cloud storage");
                    
                    // save file to storage
                    File.WriteAllBytes(dir, result.Value);
                    
                    // show in UI
                    DisplayImage();
                }
            }); 
        });
    }
    
    /// <summary>
    /// Display downloaded image to UI
    /// </summary>
    private void DisplayImage()
    {
        string dir = Path.Combine(Application.dataPath, ScreenshotDir, screenshotSlotName);
        byte[] imageData = File.ReadAllBytes(dir);
        
        Texture2D imageTexture = new Texture2D(0,0);
        imageTexture.LoadImage(imageData);
        
        screenshotImage.sprite = Sprite.Create(imageTexture, new Rect(0f, 0f, imageTexture.width, imageTexture.height), Vector2.zero);
        
        contentPanel.SetActive(true);
        contentEmptyPanel.SetActive(false);
        screenshotNameText.text = screenshotSlotName;
    }
    
    /// <summary>
    /// Create screenshot local directory if doesn't exist
    /// </summary>
    private static void CheckScreenshotDir()
    {
        string dir = Path.Combine(Application.dataPath, ScreenshotDir);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }
    
    /// <summary>
    /// Take screenshot and upload the file to screenshot slot on cloud storage
    /// </summary>
    /// <returns></returns>
    private IEnumerator TakeScreenshot()
    {
        string imageName = $"{DateTime.Now:yyyy-M-d_h-mm-ss}.jpg";
        string dir = Path.Combine(Application.dataPath, ScreenshotDir, imageName);
        
        CheckScreenshotDir();
        
        ScreenCapture.CaptureScreenshot(dir);
        Debug.Log($"Screenshot taken: {imageName}");
        
        while (!File.Exists(dir))
        {
            yield return 0;
        }
        
        UploadData(imageName);
    }
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Screenshot") && user.Session.IsValid())
        {
            Debug.Log($"taking screenshot");
            StartCoroutine(TakeScreenshot());
        }
    }
}
