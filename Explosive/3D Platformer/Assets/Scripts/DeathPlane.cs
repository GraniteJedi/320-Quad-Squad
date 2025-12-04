using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathPlane : MonoBehaviour
{
    [SerializeField] private Collider playerCollider;
    [SerializeField] private PlayerManager PlayerManager;

    void Start()
    {
        if(PlayerManager == null)
        {
            PlayerManager = GameObject.FindAnyObjectByType<PlayerManager>();
            playerCollider = PlayerManager.GetComponent<Collider>();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other == playerCollider)
        {
            PlayerManager.KillPlayer();
        }
    }
}
