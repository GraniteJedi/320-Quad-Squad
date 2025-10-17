using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathPlane : MonoBehaviour
{
    [SerializeField] private Collider playerCollider;
    [SerializeField] private PlayerManager PlayerManager;
    private void OnTriggerEnter(Collider other)
    {
        if (other == playerCollider)
        {
            PlayerManager.KillPlayer();
        }

        gameObject.GetComponent<Collider>().enabled = false;
    }
}
