using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private GameObject dialogueContainer;
    [SerializeField] private TextMeshProUGUI dialogueTextBox;

    [Header("Display Customization")]
    [SerializeField] float fullScreenTime = 0.05f;
    [SerializeField] float countBackgroundAlpha;
    [SerializeField] float dialogueDisplayTime = 5f;
    [SerializeField] float dialogueCharTime = 0.001f;
    [SerializeField] float periodCharDelay = 0.02f;

    [SerializeField] float elapsedTime;

    private bool isFullScreen;

    // Used for the initial position of the timer in the UI since it transitions to fullscreen
    private Vector2 anchorMin;
    private Vector2 anchorMax;

    // Class variables to avoid repeated instantiation in Update()
    private float currentTime;
    private float currentSecond;
    private float currentSpeed;
    private string timeText;

    private Queue<Dialogue> dialogueQueue = new Queue<Dialogue>();
    private bool printingDialogue = false;

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

        public Dialogue(string dialogue, bool isPriority, float displayTime)
        {
            this.dialogue = dialogue;
            this.isPriority = isPriority;
            this.displayTime = 0;
        }

        public string GetDialogue()
        {
            return dialogue;
        }
    }

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

        DeactivateDialogueBox();

        Dialogue initDialogue = new Dialogue("Cheese danishes smell like ordurves. Or something along those lines.");

        dialogueQueue.Enqueue(initDialogue);
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

            if (SecondTicked())
            {
                //Debug.Log("Second ticked from " + currentSecond + " to " + (int) currentTime);
            }

            currentSecond = (int)currentTime;
        }
        else if (!countdown.IsStopped() && isFullScreen)
        {
            Debug.Log("HUD State");
            StartCoroutine(HUDView());
        }

        currentSpeed = countdown.GetSpeed();

        speedTextBox.text = string.Format("{0,3}.{1:D2} m/s", (int)currentSpeed, (int)((currentSpeed - (int)currentSpeed) * 100f));

        if (dialogueQueue.Count != 0 && !printingDialogue)
        {
            StartCoroutine(ReadDialogue(dialogueQueue.Peek()));
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

    public IEnumerator ReadDialogue(Dialogue dialogue)
    {
        printingDialogue = true;

        ActivateDialogueBox();

        elapsedTime = 0;
        int index = 0;
        int maxIndex = dialogue.GetDialogue().Length;
        int currentJump;

        while (elapsedTime < dialogueDisplayTime && printingDialogue)
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
            yield return new WaitForFixedUpdate();
        }

        dialogueQueue.Dequeue();
        printingDialogue = false;

        if (dialogueQueue.Count <= 0)
        {
            DeactivateDialogueBox();
        }
    }

    public void AddDialogue(Dialogue dialogue)
    {
        dialogueQueue.Enqueue(dialogue);
    }

    void ActivateDialogueBox()
    {
        dialogueContainer.SetActive(true);
        dialogueTextBox.text = "";
    }

    void DeactivateDialogueBox()
    {
        dialogueContainer.SetActive(false);
    }

    bool SecondTicked()
    {
        return (int)currentTime != currentSecond;
    }
}
