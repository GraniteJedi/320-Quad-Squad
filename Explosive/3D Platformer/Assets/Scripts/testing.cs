using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testing : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {

        Debug.Log("platform hit by: " + other);

    }
}
