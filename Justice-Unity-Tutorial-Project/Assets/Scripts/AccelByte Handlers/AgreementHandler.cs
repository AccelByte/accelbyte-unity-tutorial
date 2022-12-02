// Copyright (c) 2021 - 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Collections.Generic;
using AccelByte.Api;
using AccelByte.Models;
using AccelByte.Core;
using UnityEngine;
using UnityEngine.UI;

public class AgreementHandler : MonoBehaviour
{
    private struct RequiredDoc
    {
        public string title;
        public string date;
        public string url;
        public string policyId;
        public string policyVersionId;
        public string localizedPolicyVersionId;
    }
    
    [SerializeField]
    private GameObject agreementPanel;
    
    [SerializeField]
    private Text agreementTitleTopText;
    [SerializeField]
    private Text agreementTitleBottomText;
    [SerializeField]
    private Text agreementDateText;
    [SerializeField]
    private Text agreementContentText;

    [SerializeField]
    private Toggle agreementAgreeToggle;

    [SerializeField]
    private Button agreementSubmitButton;

    private string language;

    private ushort currentDocsIndex;

    private List<RequiredDoc> requiredDocs;

    private User user;
    private UserProfiles userProfiles;
    private Agreement agreement;

    private void Start()
    {
        // AccelByte's Multi Registry initialization
        ApiClient apiClient = MultiRegistry.GetApiClient();
        user = apiClient.GetApi<User, UserApi>();
        userProfiles = apiClient.GetApi<UserProfiles, UserProfilesApi>();
        agreement = apiClient.GetApi<Agreement, AgreementApi>();
    }

    public void Setup()
    {
        if (user.Session.IsComply)
        {
            Debug.Log("User already agreed to the latest agreement");
            CheckUserProfile();
        }
        else
        {
            language = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            requiredDocs = new List<RequiredDoc>();
        
            agreementPanel.SetActive(true);

            // reset the UI and binding
            agreementAgreeToggle.isOn = false;
            agreementAgreeToggle.interactable = false;
            agreementSubmitButton.interactable = false;
            agreementSubmitButton.onClick.RemoveAllListeners();
            agreementAgreeToggle.onValueChanged.RemoveAllListeners();
            agreementSubmitButton.onClick.AddListener(OnSubmitAgreement);
            agreementAgreeToggle.onValueChanged.AddListener(toggleState =>
            {
                agreementSubmitButton.interactable = toggleState;
            });

            currentDocsIndex = 0;
            QueryLegalDocs();
        }
    }

    /// <summary>
    /// Check whether user is new or not to direct the user to profile creation or directly attempt to connect to lobby
    /// </summary>
    private void CheckUserProfile()
    {
        userProfiles.GetUserProfile(profileResult =>
        {
            if (profileResult.IsError)
            {
                Debug.Log("No User Profile is available for Logged In User " + profileResult.Error.Message);
                
                GetComponent<ProfileCreationHandler>().Setup();
            }
            else
            {
                Debug.Log($"UserID : {profileResult.Value.userId}");
                //Attempt to Connect to Lobby
                LobbyHandler.Instance.ConnectToLobby();
                //This is where we connect to Lobby
            }
        });
    }

    /// <summary>
    /// Query ACTIVE legal docs information
    /// </summary>
    private void QueryLegalDocs()
    {
        agreement.QueryLegalEligibilities(legalResult =>
        {
            if (legalResult.IsError)
            {
                Debug.Log("Fail to get legal eligibility");
            }
            else
            {
                Debug.Log("Success to get legal eligibility");
                
                // loop through each docs
                foreach (RetrieveUserEligibilitiesResponse policy in legalResult.Value)
                {
                    if (!policy.isMandatory)
                    {
                        continue;
                    }
                    
                    RequiredDoc requiredDoc = new RequiredDoc
                    {
                        policyId = policy.policyId,
                        title = policy.policyName
                    };

                    // loop through each versions
                    foreach (PolicyVersionWithLocalizedVersionObject policyVersion in policy.policyVersions)
                    {
                        if (!policyVersion.isInEffect)
                        {
                            continue;
                        }

                        DateTime updatedAt = Convert.ToDateTime(policyVersion.updatedAt);
                        requiredDoc.date = updatedAt.ToShortDateString();
                        requiredDoc.policyVersionId = policyVersion.id;

                        // loop through each locale
                        foreach (LocalizedPolicyVersionObject localizedPolicyVersion in policyVersion.localizedPolicyVersions)
                        {
                            if (localizedPolicyVersion.localeCode == language)
                            {
                                requiredDoc.localizedPolicyVersionId = localizedPolicyVersion.id;
                                requiredDoc.url = policy.baseUrls[0] + localizedPolicyVersion.attachmentLocation;
                            }
                        }
                        
                        // get first index if no locale match
                        if (string.IsNullOrEmpty(requiredDoc.localizedPolicyVersionId))
                        {
                            requiredDoc.localizedPolicyVersionId = policyVersion.localizedPolicyVersions[0].id;
                            requiredDoc.url = policy.baseUrls[0] + policyVersion.localizedPolicyVersions[0].attachmentLocation;
                        }
                    }
                    
                    requiredDocs.Add(requiredDoc);
                }
                
                DisplayLegalDoc();
            }
        });
    }

    /// <summary>
    /// Get the content of legal agreement document and display it to the UI
    /// </summary>
    private void DisplayLegalDoc()
    {
        agreementAgreeToggle.isOn = false;
        agreementAgreeToggle.interactable = false;
        
        agreement.GetLegalDocument(requiredDocs[currentDocsIndex].url, result =>
        {
            if (result.IsError)
            {
                Debug.Log($"Fail to get legal document [{currentDocsIndex}]");
            }
            else
            {
                Debug.Log($"Success to get legal document [{currentDocsIndex}]");
                
                agreementTitleTopText.text = requiredDocs[currentDocsIndex].title;
                agreementTitleBottomText.text = requiredDocs[currentDocsIndex].title;
                agreementDateText.text = requiredDocs[currentDocsIndex].date;
                agreementContentText.text = result.Value;
                agreementAgreeToggle.interactable = true;
            }
        });
    }

    /// <summary>
    /// Accept all legal agreements.
    /// </summary>
    private void OnSubmitAgreement()
    {
        if (currentDocsIndex >= requiredDocs.Count - 1)
        {
            // submit accept all agreements
            AcceptAgreementRequest[] acceptAgreementRequests = new AcceptAgreementRequest[requiredDocs.Count];
            for (int i = 0; i < requiredDocs.Count; ++i)
            {
                acceptAgreementRequests[i] = new AcceptAgreementRequest
                {
                    isAccepted = true,
                    policyId = requiredDocs[i].policyId,
                    policyVersionId = requiredDocs[i].policyVersionId,
                    localizedPolicyVersionId = requiredDocs[i].localizedPolicyVersionId
                };
            }
        
            agreement.BulkAcceptPolicyVersions(acceptAgreementRequests, result =>
            {
                if (result.IsError)
                {
                    Debug.Log("Fail to bulk accept legal agreement");
                }
                else
                {
                    Debug.Log("Success to bulk accept legal agreement");
                
                    // refresh login
                    user.RefreshSession((Result<TokenData, OAuthError> refreshResult) =>
                    {
                        if (refreshResult.IsError)
                        {
                            Debug.Log("Fail to refresh login:");
                        }
                        else
                        {
                            Debug.Log("Success to refresh login:");
                            
                            agreementPanel.SetActive(false);
                            CheckUserProfile();
                        }
                    });
                }
            });
        }
        else
        {
            // show next document if exist
            ++currentDocsIndex;
            DisplayLegalDoc();
        }
    }
}
