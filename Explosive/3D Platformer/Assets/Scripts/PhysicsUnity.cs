using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsUnity
{
    private Vector3 position;
    private Vector3 velocity;
    private Vector3 acceleration;
    private float gravity;
    private float mass;


    public Vector3 Position
    {
        get { return position; }
        set { position = value; }
    }

    public Vector3 Velocity
    {
        get { return velocity; }
    }

    public Vector3 Acceleration
    {
        get { return acceleration; }
    }

    public float Gravity
    {
        get { return gravity; }
        set { gravity = value; }
    }

    public float Mass
    {
        get { return mass; }
        set { mass = value; }
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

    public void ApplyVelocity(float time)
    { 
        this.position.x += velocity.x * time;
        this.position.y += velocity.y * time;
        this.position.z += velocity.z * time;
    }

    public void ApplyAcceleration(Vector3 magnitude, float time)
    {
        this.velocity.x += velocity.x*time;
        this.velocity.y += velocity.y*time;
        this.velocity.z += velocity.z*time;
    }

    public void ApplyForce(Vector3 force, float time)
    {
        acceleration.x = force.x / mass;
        acceleration.y = force.y / mass;
        acceleration.z = force.z / mass;
        velocity.x += acceleration.x * time;
        velocity.y += acceleration.y * time;
        velocity.z += acceleration.z * time;
    }

    public void ApplyGravity(float time)
    {
        ApplyForce(new Vector3(0, -mass * gravity, 0), time);
    }

    public void ApplyAirReisistance(float strength, float time)
    {
        ApplyForce(new Vector3(-velocity.x*strength, 0, -velocity.z*strength),time);
    }

    public void ZeroYVelocity()
    {
        velocity.y = 0;
    }
}
