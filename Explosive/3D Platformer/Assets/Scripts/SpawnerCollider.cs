using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerCollider : MonoBehaviour
{
    [SerializeField] List<GameObject> spawn = null;
    [SerializeField] List<GameObject> despawn = null;

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
        if (spawn != null)
        {
            foreach(GameObject gameObject in spawn)
            {
                gameObject.SetActive(true);
            }
        }

        if (despawn != null)
        {
            foreach(GameObject gameObject in despawn)
            {
                gameObject.SetActive(false);
            }
        }

        gameObject.SetActive(false);
    }
}
