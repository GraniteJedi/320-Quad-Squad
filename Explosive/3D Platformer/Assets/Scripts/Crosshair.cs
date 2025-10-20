using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    [SerializeField] private GameObject subject;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Image crosshair;
    [SerializeField] private float grappleRange;
    [SerializeField] private string[] tags;

    private RaycastHit hit;
    private Color crosshairActive;
    private Color crosshairInactive;

    // Start is called before the first frame update
    void Start()
    {
        crosshairActive = crosshairInactive= crosshair.color;
        crosshairInactive.a = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, grappleRange))
        {
            foreach (string tag in tags)
            {
                if (hit.collider.CompareTag(tag))
                {
                    subject = hit.collider.gameObject;
                    crosshair.color = crosshairActive;
                    return;
                }
            }

            crosshair.color = crosshairInactive;
            subject = null;
        }
        else
        {
            crosshair.color = crosshairInactive;
            subject = null;
        }
    }
}
