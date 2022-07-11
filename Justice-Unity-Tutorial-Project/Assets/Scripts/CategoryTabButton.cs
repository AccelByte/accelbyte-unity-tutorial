// Copyright (c) 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using UnityEngine;
using UnityEngine.UI;

public class CategoryTabButton : MonoBehaviour
{
    [SerializeField]
    private Text buttonText;

    
    public void updateText(string buttonName)
    {
        buttonText.text = buttonName;
    }
}
