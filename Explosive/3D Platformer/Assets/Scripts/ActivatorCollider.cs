using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivatorCollider : MonoBehaviour
{
    [SerializeField] List<MonoBehaviour> enable = null;
    [SerializeField] List<MonoBehaviour> disable = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        if (enable != null)
        {
            foreach(MonoBehaviour component in enable)
            {
                component.enabled = true;
            }
        }

        if (disable != null)
        {
            foreach(MonoBehaviour component in disable)
            {
                component.enabled = false;
            }
        }

        gameObject.SetActive(false);
    }
}
