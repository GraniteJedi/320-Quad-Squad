using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI Fields")]
    [SerializeField] private TextMeshProUGUI countTextBox;
    [SerializeField] private TextMeshProUGUI countBackground;
    [SerializeField] private RectTransform countTransform;
    [SerializeField] private TextMeshProUGUI speedTextBox;
    [SerializeField] private Countdown countdown;

    [Header("Display Customization")]
    [SerializeField] float fullScreenTime = 0.05f;
    [SerializeField] float countBackgroundAlpha;

    private bool isFullScreen;

    // Used for the initial position of the timer in the UI since it transitions to fullscreen
    private Vector2 anchorMin;
    private Vector2 anchorMax;

    // Class variables to avoid repeated instantiation in Update()
    private float currentTime;
    private float currentSpeed;
    private string timeText;

    // Start is called before the first frame update
    void Start()
    {
        anchorMin = new Vector2(
            countTransform.anchorMin.x,
            countTransform.anchorMin.y
            );

        anchorMax = new Vector2(
            countTransform.anchorMax.x,
            countTransform.anchorMax.y
            );
    }

    // Update is called once per frame
    void Update()
    {
        countBackground.alpha = countBackgroundAlpha;

        if (countdown.GetLenience() <= 0)
        {
            if (!isFullScreen)
                StartCoroutine(FullScreen());

            currentTime = countdown.GetTime();

            timeText = string.Format("{0:D2}.{1:D2}", (int)currentTime, (int)((currentTime - (int)currentTime) * 100f));
            countTextBox.text = timeText;

        }
        else if (!countdown.IsStopped() && isFullScreen)
        {
            Debug.Log("HUD State");
            StartCoroutine(HUDView());
        }

        currentSpeed = countdown.GetSpeed();

        speedTextBox.text = string.Format("{0,3}.{1:D2} m/s", (int)currentSpeed, (int)((currentSpeed - (int)currentSpeed) * 100f));
    }
    
    /// <summary>
    /// Shifts the view of the countdown timer to full screen when it begins ticking down
    /// </summary>
    /// 
    /// <returns>
    /// Coroutine that uses yield return
    /// </returns>
    public IEnumerator FullScreen()
    {
        if (isFullScreen)
            yield break;

        isFullScreen = true;

        Vector2 initMin = countTransform.anchorMin;
        Vector2 initMax = countTransform.anchorMax;
        float t;

        float elapsedTime = 0f;

        while (elapsedTime < fullScreenTime && isFullScreen)
        {
            t = elapsedTime / fullScreenTime;

            countTransform.anchorMin = Vector2.Lerp(initMin, Vector2.zero, t);
            countTransform.anchorMax = Vector2.Lerp(initMax, Vector2.one, t);

            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        if (isFullScreen)
        {
            countTransform.anchorMin = Vector2.zero;
            countTransform.anchorMax = Vector2.one;
        }
    }

    /// <summary>
    /// Shifts the view of the countdown timer inside the HUD container when the time isn't ticking down
    /// </summary>
    /// 
    /// <returns>
    /// Coroutine that uses yield return
    /// </returns>
    IEnumerator HUDView()
    {
        if (!isFullScreen)
            yield break;

        isFullScreen = false;

        Vector2 initMin = countTransform.anchorMin;
        Vector2 initMax = countTransform.anchorMax;
        float t;

        float elapsedTime = 0f;

        while (elapsedTime < fullScreenTime && !isFullScreen)
        {
            t = elapsedTime / fullScreenTime;

            countTransform.anchorMin = Vector2.Lerp(initMin, anchorMin, t);
            countTransform.anchorMax = Vector2.Lerp(initMax, anchorMax, t);

            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        if (!isFullScreen)
        {
            countTransform.anchorMin = anchorMin;
            countTransform.anchorMax = anchorMax;
        }
    }
}
