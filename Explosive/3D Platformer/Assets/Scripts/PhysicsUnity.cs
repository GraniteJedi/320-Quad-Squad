using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsUnity
{
    private Vector3 position;
    private Vector3 velocity;
    private Vector3 acceleration;

    public Vector3 Position
    {
        get { return position; }
        set { position = value; }
    }

    public Vector3 Velocity
    {
        get { return velocity; }
    }

    public void Physics(float positionX, float positionY, float positionZ)
    {
        position = new Vector3(positionX, positionY, positionZ);
        velocity = new Vector3(0, 0, 0);
        acceleration = new Vector3(0, 0, 0);
    }
    public void ApplyVelocity(Vector3 velocity, float time)
    {
        this.velocity.x += velocity.x;
        this.velocity.y += velocity.y;
        this.velocity.z += velocity.z;

        this.position.x += velocity.x * time;
        this.position.y += velocity.y * time;
        this.position.z += velocity.z * time;
    }
}
