using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretProjectileMath : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] Rigidbody playerRigidbody;
    [SerializeField] PlayerManager playerManager;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] float projectileSpeed;
    [SerializeField] Transform projectileSpawnPoint;
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FireProjectile()
    {
        Vector3 interceptPoint = FindCollsionSpot();
        Vector3 fireDirection = (interceptPoint - projectileSpawnPoint.position).normalized;
       
        GameObject proj = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.LookRotation(fireDirection));

        Rigidbody projRb = proj.GetComponent<Rigidbody>();
        if (projRb != null)
        {
            projRb.velocity = fireDirection * projectileSpeed;
        }
    }

    Vector3 FindCollsionSpot()
    {
        Vector3 playerPos = playerRigidbody.position;
        Vector3 playerVel = playerManager.TotalVelocity;
        Vector3 turretPos = transform.position;
        Vector3 displacement = playerPos - turretPos;

        //quadratic formula to calaculate the intercept point 
        float a = Vector3.Dot(playerVel, playerVel) - projectileSpeed * projectileSpeed;
        float b = 2 * Vector3.Dot(displacement, playerVel);
        float c = 2 * Vector3.Dot(displacement, displacement);

        float middlePartOfQuad = b*b -4*a*c;
        if (middlePartOfQuad < 0)
        {
            //this should only happen if our projectile is REALLY slow
            return playerPos;
        }

        float time1 = (-b + Mathf.Sqrt(middlePartOfQuad) / 2*a);
        float time2 = (-b - Mathf.Sqrt(middlePartOfQuad) / 2 * a);

        float actualTime = Mathf.Min(time1, time2);
        if (actualTime < 0) { actualTime = Mathf.Max(time1, time2); }
        if (actualTime < 0) { actualTime = 0; } //intercept is not possible
         Vector3 interceptPoint = playerPos + playerVel * actualTime;
        return interceptPoint;
    }
}
