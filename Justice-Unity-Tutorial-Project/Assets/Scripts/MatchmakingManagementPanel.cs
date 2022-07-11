// Copyright (c) 2021 - 2022 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MatchmakingManagementPanel : MonoBehaviour
{
    [SerializeField]
    private Image loadingImage;

    [SerializeField]
    private Text countUpText;

    [SerializeField]
    private Text countDownText;

    private bool isWaiting;
    private float deltaTime;

    private void OnEnable()
    {
        // Reset boolean when object changes into enable
        isWaiting = false;
    }

    private void Update()
    {
        // Start loading animation
        if (!isWaiting)
        {
            StartCoroutine(StartLoadingAnimation());
        }

        // Start count down and count up timer
        if (countUpText.isActiveAndEnabled || countDownText.isActiveAndEnabled)
        {
            if (deltaTime >= 1)
            {
                deltaTime = 0;
                StartCountup();
                StartCountdown();
            }
            else
            {
                deltaTime += Time.deltaTime;
            }
        }
    }

    /// <summary>
    /// Animate the loading bar each 0.1 seconds
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartLoadingAnimation()
    {
        // Avoid the loading animation is being called when the Async is not finished yet
        isWaiting = true;

        // Animate the loading image
        loadingImage.transform.Rotate(0, 0, -45);

        // Wait 0.1 seconds before next animation is executed
        yield return new WaitForSeconds(0.1f);

        // Set boolean so it can be called again in the Update
        isWaiting = false;
    }

    /// <summary>
    /// Start count up timer to show time elapsed for waiting to find a match
    /// </summary>
    private void StartCountup()
    {
        if (!countUpText.isActiveAndEnabled) return;

        // Parse the time elapsed text into minutes and seconds (int)
        string time = countUpText.text.Substring(countUpText.text.IndexOf(':') + 2);
        int minutes = int.Parse(time.Substring(0, time.IndexOf(':')));
        int seconds = int.Parse(time.Substring(time.IndexOf(':') + 1));

        // Add 1 second increment
        int timer = minutes * 60 + seconds + 1;

        // Parse the time into text with format mm:ss
        countUpText.text = $"Time Elapsed: {string.Format("{0:00}", (timer / 60))}:{string.Format("{0:00}", (timer % 60))}";
    }

    /// <summary>
    /// Start the count down timer to show time remaining to set ready consent
    /// </summary>
    private void StartCountdown()
    {
        // Do nothing if the count down text is not active
        if (!countDownText.isActiveAndEnabled) return;

        // Parse from text into time (int)
        int timer = int.Parse(countDownText.text);

        // Decrease 1 second and parse into text
        countDownText.text = Mathf.Round(timer - 1).ToString();
    }
}
