using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    /// <summary>
    ///     Settings for the In-Game UI elements such as the Compass, Timer, Speedometer, and the general HUD
    ///     </summary>
    [Header("===== In-Game UI Settings =====")]

    [SerializeField] private AudioManager audioManager;
    [SerializeField] private PlayerManager playerManager;

    // Used to make the timer clicks happen based on a time interval instead of every frame
    private float lastClickTime;

    /// <summary>
    ///     References to major components/objects that the HUD relies on
    ///     </summary>
    [Header("UI Fields")]
    [SerializeField] private TextMeshProUGUI countTextBox;
    [SerializeField] private TextMeshProUGUI countBackground;
    [SerializeField] private RectTransform countTransform;
    [SerializeField] private TextMeshProUGUI speedTextBox;
    [SerializeField] private Slider speedSlider;
    [SerializeField] private float speedSliderSensitivity;
    [SerializeField] private Image speedFillImage;
    [SerializeField] private Image speedContainerImage;
    [SerializeField] private Countdown countdown;
    [SerializeField] private GameObject dialogueContainer;
    [SerializeField] private TextMeshProUGUI dialogueTextBox;
    [SerializeField] private InputActionAsset uiInputAsset;
    private InputAsset uiControls;
    private InputAction continueAction;

    /// <summary>
    ///     Customizable attributes specifically for the In-Game HUD elements
    ///     </summary>
    [Header("Display Customization")]
    [SerializeField] float fullScreenTime = 0.05f;
    [SerializeField] float countBackgroundAlpha;
    [SerializeField] float countForegroundAlpha;
    [SerializeField] float dialogueDisplayTime = 5f;
    [SerializeField] float dialogueCharTime = 0.001f;
    [SerializeField] float dialogueCloseTime = 0.1f;
    [SerializeField] float safeSpeed = 10f;

    // Determines whether or not the countdown is being displayed in the full screen view
    private bool isFullScreen;

    // Used for the initial position of the timer in the UI since it transitions to fullscreen
    private Vector2 anchorMin;
    private Vector2 anchorMax;

    // Class variables to avoid repeated instantiation in Update()
    private float currentTime;
    private float currentSecond;
    private float currentSpeed;
    private string timeText;

    // The transition color for the Speed UI element
    private float colorT;

    // Used for the dialogue system of the UI
    private Queue<Dialogue> dialogueQueue = new Queue<Dialogue>();
    private bool printingDialogue = false;

    /// <summary>
    ///     Settings for the Pause Menu UI elements such as the Menu Containers and Settings Sliders
    ///     </summary>
    [Header("===== Pause UI Settings =====")]
    [SerializeField] private GameObject pauseUIContainer;
    [SerializeField] private GameObject pauseMenuContainer;
    [SerializeField] private GameObject optionsMenuContainer;
    [SerializeField] private Slider cameraSensitivity;
    [SerializeField] private TextMeshProUGUI sensitivityValue;
    [SerializeField] private Slider cameraFOV;
    [SerializeField] private TextMeshProUGUI FOVValue;
    
    //Needed to get/set sensitivity
    private Camera playerCamera;

    // Start is called before the first frame update
    void Start()
    {
        // Initializes variable used to know when the countdown timer should audibly tick
        currentSecond = (int)countdown.GetTime();

        // Sets the initial anchor points for when the countdown timer is in the HUD view vs full screen view 
        anchorMin = new Vector2(
            countTransform.anchorMin.x,
            countTransform.anchorMin.y
            );
        anchorMax = new Vector2(
            countTransform.anchorMax.x,
            countTransform.anchorMax.y
            );

        // Disables the dialogue container
        dialogueContainer.SetActive(false);

        #region OUTSIDE PREFAB CHECKS
        /// Checking if variables that can't be saved within a prefab have been set in the current scene
        ///     and printing an error message otherwise
        if (audioManager == null)
        {
            Debug.LogError("AUDIO MANAGER hasn't been set on the UI MANAGER script");
        }

        if (uiInputAsset == null)
        {
            Debug.LogError("UI INPUT ASSET hasn't been set on the UI MANAGER script");
        }
        else
        {
            continueAction = uiInputAsset.FindActionMap("UI").FindAction("Continue");
            continueAction.Disable();
        }
        #endregion

        uiControls = new InputAsset();

        // Initializes the player camera for changing FOV when necessary
        playerCamera = Camera.main;

        // Hiding the mouse from the user while playing
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Initializing settings sliders
        cameraFOV.value = playerCamera.fieldOfView;
        SetFOV();
        cameraSensitivity.value = playerManager.GetSensitivity();
        SetSensitivity();

        // Starting the game paused
        StartCoroutine(PauseRoutine());
        playerManager.SwitchMap("UI");

        // Initializing the speed limits for the speed slider in the HUD
        speedSlider.minValue = countdown.GetThreshold();
        speedSlider.maxValue = GetMaxSpeed();
    }

    // Update is called once per frame
    void Update()
    {
        // Adjust the transparency of the number backdrop of the countdown
        countBackground.alpha = countBackgroundAlpha;

        // If the countdown's lenience has passed zero and the timer is active
        if (countdown.GetLenience() <= 0 && countdown.IsActive())
        {
            // Set the view of the timer to full screen if it isn't already
            if (!isFullScreen)
                StartCoroutine(FullScreen());

            // Update the current time
            currentTime = countdown.GetTime();
            timeText = string.Format("{0:D2}.{1:D2}", (int)currentTime, (int)((currentTime - (int)currentTime) * 100f));
            countTextBox.text = timeText;

            // Play SFX
            PlayCountdownClicks();
            PlayCountdownTick();

            // Update the current second (this is used for pacing SFX)
            currentSecond = (int)currentTime;
        }
        // If the timer's lenience is above zero and the timer view is still in fullscreen, or the timer is disabled
        else if ((countdown.GetLenience() > 0 && isFullScreen) || !countdown.IsActive())
        {
            StartCoroutine(HUDView());
        }

        if (countdown.IsActive())
        {
            countTextBox.color = Color.red;
        }
        else
        {
            countTextBox.color = Color.HSVToRGB(69f/360f,62f/100f,100f/100f);
        }

        // Update the current Speed
        currentSpeed = countdown.GetSpeed();

        //speedTextBox.text = string.Format("{0,3}.{1:D2} m/s", (int)currentSpeed, (int)((currentSpeed - (int)currentSpeed) * 100f));

        // Update the Speed UI bar based on the current speed
        speedSlider.value = Mathf.Lerp(speedSlider.value, Mathf.Lerp(countdown.GetThreshold(), GetMaxSpeed(), countdown.GetSpeed() / GetMaxSpeed()), speedSliderSensitivity);

        // Update the color of the Speed UI elements based on the current speed
        SpeedRecolor();
    }

    // Handles dialogue in fixed update so that players do not get text based on frame rate
    void FixedUpdate()
    {
        if (dialogueQueue.Count > 0)
        {
            if (printingDialogue && dialogueQueue.Peek().IsPriority())
            {
                printingDialogue = false;
            }
            else if (!printingDialogue)
            {
                StartCoroutine(ReadDialogue(dialogueQueue.Dequeue()));
            }
        }
    }
    
    #region IN-GAME UI FUNCTIONS ====================================================================================

    /// <summary>
    ///     Shifts the view of the countdown timer to full screen when it begins ticking down
    ///     </summary>
    /// 
    /// <returns>
    ///     Coroutine that uses yield return
    ///     </returns>
    public IEnumerator FullScreen()
    {
        if (isFullScreen)
            yield break;

        isFullScreen = true;
        countTextBox.alpha = countForegroundAlpha;

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
    ///     Shifts the view of the countdown timer inside the HUD container when the time isn't ticking down
    ///     </summary>
    /// 
    /// <returns>
    ///     Coroutine that uses yield return
    ///     </returns>
    IEnumerator HUDView()
    {
        if (!isFullScreen)
            yield break;

        isFullScreen = false;
        countTextBox.alpha = 1f;

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

    /// <summary>
    ///     Used to smoothly transition the color of the player's speed text in the UI based on
    ///     how close their speed is to the speed threshold they must pass
    ///     </summary>
    void SpeedRecolor()
    {
        colorT = Mathf.Clamp01(Mathf.Lerp(0, 1, (currentSpeed - countdown.GetThreshold()) / safeSpeed));

        if(!countdown.IsActive())
        {
            Color shiftedColor = Color.white;
            speedFillImage.color = shiftedColor;
            speedContainerImage.color = shiftedColor;
            return;
        }

        if (colorT < 0.5)
        {
            //speedTextBox.color = Color.Lerp(Color.red, Color.yellow, colorT / 0.5f);
            Color shiftedColor = Color.Lerp(Color.red, Color.yellow, colorT / 0.5f);
            speedFillImage.color = shiftedColor;
            speedContainerImage.color = shiftedColor;
        }
        else
        {
            //speedTextBox.color = Color.Lerp(Color.yellow, Color.white, (colorT - 0.5f) / 0.5f);
            Color shiftedColor = Color.Lerp(Color.yellow, Color.white, colorT / 0.5f);
            speedFillImage.color = shiftedColor;
            speedContainerImage.color = shiftedColor;
        }
    }

    /// <summary>
    ///     Manually sets the player's countdown timer to active
    ///     </summary>
    public void ActivateCountdown()
    {
        countdown.SetActive(true);
    }

    /// <summary>
    ///     Manually sets the player's countdown timer to inactive
    ///     </summary>
    public void DeactivateCountdown()
    {
        countdown.SetActive(false);
    }

    #endregion

    #region DIALOGUE SYSTEM FUNCTIONS ===============================================================================
    /// =================================
    /// === DIALOGUE SYSTEM FUNCTIONS ===
    /// =================================

    /// <summary>
    ///     Object representative of dialogue to be written in the player's UI
    ///     </summary>
    /// 
    /// <dialogue>
    ///     The actual text to be written out to the UI
    ///     </dialogue>
    /// 
    /// <isPriority>
    ///     Whether or not this dialogue should skip over any current dialogue being written
    ///     </isPriority>
    /// 
    /// <displayTime>
    ///     The amount of time this dialogue should be displayed in the UI
    ///     </displayTime>
    public class Dialogue
    {
        string dialogue;
        bool isPriority;
        float displayTime;

        public Dialogue(string dialogue)
        {
            this.dialogue = dialogue;
            this.isPriority = false;
            this.displayTime = 0;
        }

        public Dialogue(string dialogue, bool isPriority)
        {
            this.dialogue = dialogue;
            this.isPriority = isPriority;
            this.displayTime = 0;
        }

        public Dialogue(string dialogue, float displayTime)
        {
            this.dialogue = dialogue;
            this.isPriority = false;
            this.displayTime = displayTime;
        }

        public Dialogue(string dialogue, bool isPriority, float displayTime)
        {
            this.dialogue = dialogue;
            this.isPriority = isPriority;
            this.displayTime = displayTime;
        }

        public string GetDialogue()
        {
            return dialogue;
        }

        public float GetDisplayTime()
        {
            return displayTime;
        }

        public void SetDisplayTime(float displayTime)
        {
            this.displayTime = displayTime;
        }

        public bool IsPriority()
        {
            return isPriority;
        }
    }

    /// <summary>
    ///     A likely tutorial specific function that can be used to slow down time for the
    ///     player to give them a chance to read crucial tutorial text. This would likely
    ///     only be triggered by a TutorialCollider object
    ///     </summary>
    /// 
    /// <returns>
    ///     Utilizes yield return since it's a coroutine
    ///     </returns>
    public IEnumerator SlowTime()
    {
        //Debug.Log("Slowing Time");

        if (dialogueQueue.Count > 0)
        {
            if (printingDialogue && dialogueQueue.Peek().IsPriority())
            {
                printingDialogue = false;
                StartCoroutine(ReadDialogue(dialogueQueue.Dequeue()));
            }
            else
            {
                StartCoroutine(ReadDialogue(dialogueQueue.Dequeue()));
            }
        }

        Time.timeScale = 0.05f;
        TimeShift(0.05f);
        continueAction.Enable();

        yield return new WaitUntil(() => continueAction.triggered);

        //Debug.Log("Key Hit");
        Time.timeScale = 1f;
        TimeShift(1);
        continueAction.Disable();
    }

    /// <summary>
    ///     Reads the dialogue from a Dialogue object and uses its stored displayTime to display the text
    ///     for as long as necessary
    ///     </summary>
    /// 
    /// <param name="dialogue">
    ///     The Dialogue object that stores the dialogue text, the length of time the dialogue is displayed,
    ///     and whether or not this dialogue should be prioritized and overwrite current dialogue
    ///     </param>
    /// 
    /// <returns>
    ///     Utilizes yield return since it's a coroutine
    ///     </returns>
    public IEnumerator ReadDialogue(Dialogue dialogue)
    {
        printingDialogue = true;

        ActivateDialogueBox();

        float elapsedTime = 0;
        int index = 0;
        int maxIndex = dialogue.GetDialogue().Length;
        int currentJump;

        if (dialogue.GetDisplayTime() == 0)
        {
            dialogue.SetDisplayTime(dialogueDisplayTime);
        }

        while (elapsedTime < dialogue.GetDisplayTime() && printingDialogue)
        {
            currentJump = (int)(Time.fixedDeltaTime / dialogueCharTime);

            if (index + currentJump < maxIndex)
            {
                dialogueTextBox.text += dialogue.GetDialogue().Substring(index, currentJump);
                index += currentJump;
            }
            else if (index < maxIndex)
            {
                dialogueTextBox.text += dialogue.GetDialogue()[index++];
            }

            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForSecondsRealtime(0.02f);
        }

        printingDialogue = false;

        if (dialogueQueue.Count <= 0 && !printingDialogue)
        {
            DeactivateDialogueBox();
        }
    }

    /// <summary>
    ///     Adds a Dialogue object to the queue of dialogue to be read out in the UI
    ///     </summary>
    /// 
    /// <param name="dialogue">
    ///     The Dialogue object that stores the dialogue text, the length of time the dialogue is displayed,
    ///     and whether or not this dialogue should be prioritized and overwrite current dialogue
    ///     </param>
    public void AddDialogue(Dialogue dialogue)
    {
        dialogueQueue.Enqueue(dialogue);
    }

    /// <summary>
    ///     Activates the textbox where dialogue is written in the UI
    ///     </summary>
    void ActivateDialogueBox()
    {
        dialogueContainer.SetActive(true);
        dialogueContainer.transform.localScale = Vector3.one;
        dialogueTextBox.text = "";
    }

    /// <summary>
    ///     Deactivates the textbox where dialogue is written in the UI through a transition in a Coroutine
    ///     </summary>
    void DeactivateDialogueBox()
    {
        StartCoroutine(CloseDialogueBox());
    }

    /// <summary>
    ///     Coroutine used to smoothly transition closing the textbox that dialogue is written out to
    ///     </summary>
    /// 
    /// <returns>
    ///     Utilizes yield return since it's a coroutine
    ///     </returns>
    IEnumerator CloseDialogueBox()
    {
        Vector3 newScale = Vector3.up + Vector3.forward;
        float elapsedTime = 0;

        while (elapsedTime < dialogueCloseTime && !printingDialogue)
        {
            dialogueContainer.transform.localScale = Vector3.Lerp(Vector3.one, newScale, 1 - Mathf.Pow(1 - (elapsedTime / dialogueCloseTime), 3f));

            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        if (!printingDialogue)
        {
            dialogueContainer.transform.localScale = newScale;
            dialogueContainer.SetActive(false);
        }
    }

    #endregion

    #region AUDIO MANAGER FUNCTIONS =================================================================================
    /// ===============================
    /// === AUDIO MANAGER FUNCTIONS ===
    /// ===============================

    /// <summary>
    ///     Checks if the timer has changed seconds since the last second tick so that
    ///     it can play the SFX at a proper pace
    ///     </summary>
    void PlayCountdownTick()
    {
        if (audioManager != null && SecondTicked())
            audioManager.PlayCountdownTick();
    }

    /// <summary>
    ///     Checks if 0.01 seconds has passed since the last countdown click so that
    ///     it can play the SFX at a proper pace
    ///     </summary>
    void PlayCountdownClicks()
    {
        if (audioManager != null && Time.time - lastClickTime > 0.01f && countdown.GetTime() > 0)
        {
            lastClickTime = Time.time;
            audioManager.PlayCountdownClicks();
        }
    }

    /// <summary>
    ///     Slows down time for audio since the UI is currently slowing time
    ///     </summary>
    /// 
    /// <param name="time">
    ///     The new timeScale (1 is when time moves normally, 0 means everything is stopped)
    ///     </param>
    void TimeShift(float time)
    {
        if (audioManager != null)
            audioManager.TimeShift(time);
    }

    /// <summary>
    ///     Determine if the countdown has decreased by a full second. This will be used to trigger
    ///     SFX
    ///     </summary>
    /// 
    /// <returns>
    ///     True if the last reported second (currentSecond) is not equal to the new current second ((int)currentTime),
    ///     and false otherwise
    ///     </returns>
    bool SecondTicked()
    {
        return (int)currentTime != currentSecond;
    }

    #endregion
    
    #region PAUSE UI FUNCTIONS ======================================================================================
    /// ==========================
    /// === PAUSE UI FUNCTIONS ===
    /// ==========================

    public void HighlightText(TextMeshProUGUI text)
    {
        Button textButton = text.transform.GetComponentInParent<Button>();

        if (textButton == null)
            return;

        text.color = textButton.colors.highlightedColor;
        text.text = "> <u>" + text.text + "</u> <";
    }

    public void NormalText(TextMeshProUGUI text)
    {
        Button textButton = text.transform.GetComponentInParent<Button>();
        if (!text.text.Contains("<u>") || textButton == null)
            return;

        text.color = textButton.colors.normalColor;
        text.text = text.text.Substring(5, text.text.Length - 11);
    }

    public void Pause(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (!pauseMenuContainer.activeSelf)
            {
                StartCoroutine(PauseRoutine());
            }
            else
            {
                Resume();
            }
        }
    }

    public void Pause()
    {
        if (!pauseMenuContainer.activeSelf)
        {
            StartCoroutine(PauseRoutine());
        }
        else
        {
            Resume();
        }
    }

    private float GetMaxSpeed()
    {
        return 50;
    }

    private IEnumerator PauseRoutine()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yield return new WaitForEndOfFrame();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        pauseUIContainer.SetActive(true);

        pauseMenuContainer.SetActive(true);
        optionsMenuContainer.SetActive(false);
        Time.timeScale = 0;
    }

    public void Options()
    {
        StartCoroutine(OptionsRoutine());
    }

    private IEnumerator OptionsRoutine()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yield return new WaitForEndOfFrame();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        pauseMenuContainer.SetActive(false);
        optionsMenuContainer.SetActive(true);
        Time.timeScale = 0;
    }

    public void Resume()
    {
        StartCoroutine(ResumeRoutine());
    }

    private IEnumerator ResumeRoutine()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yield return new WaitForEndOfFrame();

        playerManager.Resume();
        
        pauseUIContainer.SetActive(false);
        pauseMenuContainer.SetActive(false);
        optionsMenuContainer.SetActive(false);
        Time.timeScale = 1;
    }

    public void SetFOV()
    {
        playerCamera.fieldOfView = cameraFOV.value;
        FOVValue.text = cameraFOV.value.ToString("F2");
    }

    public void SetSensitivity()
    {
        playerManager.SetSensitivity(cameraSensitivity.value);
        sensitivityValue.text = cameraSensitivity.value.ToString("F2");
    }

    public void Quit()
    {
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    
    #endregion
}
