using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Countdown : MonoBehaviour
{
    [Header("Initial Attributes")]
    [SerializeField] Rigidbody playerBody = null;
    [SerializeField] float time = 10f;
    [SerializeField] TextMeshProUGUI textBox;
    [SerializeField] TextMeshProUGUI backdropBox;
    [SerializeField] string timeText = "";
    [SerializeField] float speedThreshold = 5f;
    [SerializeField] float currentSpeed;
    [SerializeField] float initLenience;
    float lenience;

    [Header("Display Customization")]
    [SerializeField] float alpha = 1f;
    [SerializeField] float backdropAlpha = 0.2f;
    [SerializeField] bool fullScreen = true;
    [SerializeField] float leftRight = 0.85f;
    [SerializeField] float downUp = 0.85f;
    [SerializeField] float resize = 0.7f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //Update the current speed solely display purposes
        currentSpeed = GetSpeed();

        if (IsStopped())
        {
            lenience -= Time.deltaTime;

            if (lenience <= 0)
            {
                ShowTimer();

                if (time - Time.deltaTime > 0)
                {
                    time -= Time.deltaTime;
                }
                else
                {
                    time = 0;
                    //TRIGGERS FUNCTION TO KILL/RESET/RESPAWN PLAYER
                }
            }
        }
        else
        {
            lenience = initLenience;
            HideTimer();
        }

        timeText = string.Format("{0:D2}.{1:D2}", (int)time, (int)((time - (int)time) * 100f));

        textBox.text = timeText;

        if (fullScreen)
        {
            backdropBox.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f);
            backdropBox.GetComponent<RectTransform>().localScale = Vector3.one;
        }
        else
        {
            backdropBox.GetComponent<RectTransform>().anchorMin = new Vector2(leftRight, downUp);
            backdropBox.GetComponent<RectTransform>().localScale = Vector3.one * resize;
        }
    }

    void SetTime(float time)
    {
        this.time = time;
    }

    /// <summary>
    /// Fetches the current speed of the player (REPLACE WITH FUNCTION FROM CUSTOM PHYSICS CLASS IF NECESSARY)
    /// </summary>
    /// 
    /// <returns>
    /// The current speed of the player, or a zero vector if N/A
    /// </returns>
    float GetSpeed()
    {
        if (playerBody != null)
            return playerBody.velocity.magnitude;
        else
            return 0f;
    }

    /// <summary>
    /// Determines if the player is or is nearly stopped based on if their speed is below a threshold
    /// </summary>
    /// 
    /// <returns>
    /// Returns TRUE when the player's speed is too low and FALSE when the player's speed is past the threshold
    /// </returns>
    bool IsStopped()
    {
        return GetSpeed() < speedThreshold;
    }

    /// <summary>
    /// Hide the timer from the player's camera.
    /// </summary>
    void HideTimer()
    {
        textBox.alpha = 0f;
        backdropBox.alpha = 0f;
    }

    /// <summary>
    /// Show the timer to the player's camera.
    /// </summary>
    void ShowTimer()
    {
        textBox.alpha = alpha;
        backdropBox.alpha = backdropAlpha;
    }     
}