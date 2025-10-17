using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombRemoval : MonoBehaviour
{
    [SerializeField] private Collider playerCollider;
    [SerializeField] private Countdown countdown;
    [SerializeField] private UIManager uiManager;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == playerCollider)
        {
            countdown.SetActive(false);
            uiManager.AddDialogue(new UIManager.Dialogue("Congratulations, you disabled the bomb and finished the tutorial!", 4));
            uiManager.AddDialogue(new UIManager.Dialogue("Reload the page to see if you can finish with more time.", 5));
        }

        gameObject.GetComponent<Collider>().enabled = false;
    }

}
