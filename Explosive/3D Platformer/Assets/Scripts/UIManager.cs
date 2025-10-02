using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    [SerializeField] private AudioManager audioManager;

    [Header("UI Fields")]
    [SerializeField] private TextMeshProUGUI countTextBox;
    [SerializeField] private TextMeshProUGUI countBackground;
    [SerializeField] private RectTransform countTransform;
    [SerializeField] private TextMeshProUGUI speedTextBox;
    [SerializeField] private Countdown countdown;
    [SerializeField] private GameObject dialogueContainer;
    [SerializeField] private TextMeshProUGUI dialogueTextBox;
    [SerializeField] private InputActionAsset uiInputAsset;
    private InputAction continueAction;

    [Header("Display Customization")]
    [SerializeField] float fullScreenTime = 0.05f;
    [SerializeField] float countBackgroundAlpha;
    [SerializeField] float countForegroundAlpha;
    [SerializeField] float dialogueDisplayTime = 5f;
    [SerializeField] float dialogueCharTime = 0.001f;
    [SerializeField] float dialogueCloseTime = 0.1f;
    [SerializeField] float safeSpeed = 10f;

    private bool isFullScreen;

    // Used for the initial position of the timer in the UI since it transitions to fullscreen
    private Vector2 anchorMin;
    private Vector2 anchorMax;

    // Class variables to avoid repeated instantiation in Update()
    private float currentTime;
    private float currentSecond;
    private float currentSpeed;
    private string timeText;
    private float colorT;

    // Used for the dialogue system of the UI
    private Queue<Dialogue> dialogueQueue = new Queue<Dialogue>();
    private bool printingDialogue = false;

    // Start is called before the first frame update
    void Start()
    {
        currentSecond = (int)countdown.GetTime();

        anchorMin = new Vector2(
            countTransform.anchorMin.x,
            countTransform.anchorMin.y
            );

        anchorMax = new Vector2(
            countTransform.anchorMax.x,
            countTransform.anchorMax.y
            );

        dialogueContainer.SetActive(false);

        //Dialogue initDialogue = new Dialogue("Cheese danishes smell like ordurves. Or something along those lines.");

        //dialogueQueue.Enqueue(initDialogue);

        continueAction = uiInputAsset.FindActionMap("UI").FindAction("Continue");
        continueAction.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        countBackground.alpha = countBackgroundAlpha;

        if (countdown.GetLenience() <= 0 && countdown.IsActive())
        {
            if (!isFullScreen)
                StartCoroutine(FullScreen());

            currentTime = countdown.GetTime();

            timeText = string.Format("{0:D2}.{1:D2}", (int)currentTime, (int)((currentTime - (int)currentTime) * 100f));
            countTextBox.text = timeText;

            PlayCountdownClicks();

            if (SecondTicked())
            {
                PlayCountdownTick();
            }

            currentSecond = (int)currentTime;
        }
        else if ((countdown.GetLenience() > 0 && isFullScreen) || !countdown.IsActive())
        {
            //Debug.Log("HUD State");
            StartCoroutine(HUDView());
        }

        currentSpeed = countdown.GetSpeed();

        speedTextBox.text = string.Format("{0,3}.{1:D2} m/s", (int)currentSpeed, (int)((currentSpeed - (int)currentSpeed) * 100f));

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
            else
            {
                StartCoroutine(ReadDialogue(dialogueQueue.Dequeue()));
            }
        }
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
    /// Reads the dialogue from a Dialogue object and uses its stored displayTime to display the text
    ///     for as long as necessary
    /// </summary>
    /// 
    /// <param name="dialogue">
    /// The Dialogue object that stores the dialogue text, the length of time the dialogue is displayed,
    ///     and whether or not this dialogue should be prioritized and overwrite current dialogue
    /// </param>
    /// 
    /// <returns>
    /// Utilizes yield return since it's a coroutine
    /// </returns>
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

        if (dialogueQueue.Count <= 0)
        {
            DeactivateDialogueBox();
        }
    }

    /// <summary>
    /// Adds a Dialogue object to the queue of dialogue to be read out in the UI
    /// </summary>
    /// 
    /// <param name="dialogue">
    /// The Dialogue object that stores the dialogue text, the length of time the dialogue is displayed,
    ///     and whether or not this dialogue should be prioritized and overwrite current dialogue
    /// </param>
    public void AddDialogue(Dialogue dialogue)
    {
        dialogueQueue.Enqueue(dialogue);
    }

    /// <summary>
    /// Activates the textbox where dialogue is written in the UI
    /// </summary>
    void ActivateDialogueBox()
    {
        dialogueContainer.SetActive(true);
        dialogueContainer.transform.localScale = Vector3.one;
        dialogueTextBox.text = "";
    }

    /// <summary>
    /// Deactivates the textbox where dialogue is written in the UI through a transition in a Coroutine
    /// </summary>
    void DeactivateDialogueBox()
    {
        StartCoroutine(CloseDialogueBox());
    }

    /// <summary>
    /// Used to smoothly transition the color of the player's speed text in the UI based on
    ///     how close their speed is to the speed threshold they must pass
    /// </summary>
    void SpeedRecolor()
    {
        colorT = Mathf.Clamp01(Mathf.Lerp(0, 1, (currentSpeed - countdown.GetThreshold()) / safeSpeed));

        if (colorT < 0.5)
        {
            speedTextBox.color = Color.Lerp(Color.red, Color.yellow, colorT / 0.5f);
        }
        else
        {
            speedTextBox.color = Color.Lerp(Color.yellow, Color.white, (colorT - 0.5f) / 0.5f);
        }
    }

    /// <summary>
    /// Coroutine used to smoothly transition closing the textbox that dialogue is written out to
    /// </summary>
    /// 
    /// <returns>
    /// Utilizes yield return since it's a coroutine
    /// </returns>
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

    /// <summary>
    /// Determine if the countdown has decreased by a full second. This will be used to trigger
    ///     SFX
    /// </summary>
    /// 
    /// <returns>
    /// True if the last reported second (currentSecond) is not equal to the new current second ((int)currentTime),
    ///     and false otherwise
    /// </returns>
    bool SecondTicked()
    {
        return (int)currentTime != currentSecond;
    }

    /// <summary>
    /// Manually sets the player's countdown timer to active
    /// </summary>
    public void ActivateCountdown()
    {
        countdown.SetActive(true);
    }

    /// <summary>
    /// Manually sets the player's countdown timer to inactive
    /// </summary>
    public void DeactivateCountdown()
    {
        countdown.SetActive(false);
    }

    /// <summary>
    /// Object representative of dialogue to be written in the player's UI
    /// </summary>
    /// 
    /// <dialogue>
    /// The actual text to be written out to the UI
    /// </dialogue>
    /// 
    /// <isPriority>
    /// Whether or not this dialogue should skip over any current dialogue being written
    /// </isPriority>
    /// 
    /// <displayTime>
    /// The amount of time this dialogue should be displayed in the UI
    /// </displayTime>
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
    /// A likely tutorial specific function that can be used to slow down time for the
    ///     player to give them a chance to read crucial tutorial text. This would likely
    ///     only be triggered by a TutorialCollider object
    /// </summary>
    /// 
    /// <returns>
    /// Utilizes yield return since it's a coroutine
    /// </returns>
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

    /// ===============================
    /// === AUDIO MANAGER FUNCTIONS ===
    /// ===============================

    void PlayCountdownTick()
    {
        audioManager.PlayCountdownTick();
    }

    void PlayCountdownClicks()
    {
        audioManager.PlayCountdownClicks();
    }

    void TimeShift(float time)
    {
        audioManager.TimeShift(time);
    }
}
