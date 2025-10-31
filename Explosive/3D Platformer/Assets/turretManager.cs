using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class turretManager : MonoBehaviour
{
    [SerializeField] TurretProjectileMath turretActual;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            turretActual.turretActive = true;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            turretActual.turretActive = false;
        }
    }
}
