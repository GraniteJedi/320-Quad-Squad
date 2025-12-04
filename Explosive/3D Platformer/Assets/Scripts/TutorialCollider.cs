using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialCollider : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;
    [SerializeField] private Collider triggerCollider;
    [SerializeField] private string tutorialText;
    [SerializeField] private float displayTime;
    [SerializeField] private bool slowTime = false;

    // Start is called before the first frame update
    void Start()
    {
        if (uiManager == null)
        {
            uiManager = GameObject.FindAnyObjectByType<UIManager>();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// When the player collides with this trigger, an informative message is sent to the UI Manager
    ///     to be printed out for the player, and the collider component is disabled
    /// 
    /// </summary>
    /// <param name="other">
    /// The object (hopefully the player) that came into contact with this trigger
    /// </param>
    void OnTriggerEnter(Collider other)
    {
        uiManager.AddDialogue(new UIManager.Dialogue(tutorialText, true, displayTime));

        if (slowTime)
            StartCoroutine(uiManager.SlowTime());

        triggerCollider.enabled = false;
    }
}
