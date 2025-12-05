using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileScript : MonoBehaviour
{

    float lifeSpan = 5.0f;
    float counter = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        counter += Time.deltaTime;
        if (counter > lifeSpan)
        {
            Destroy(this.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    { 
        if (other.gameObject.tag == "Player" || other.gameObject.tag == "Wall" || other.gameObject.tag == "Ground")
        {
            Destroy(this.gameObject);
        }
       
    }

    

}
