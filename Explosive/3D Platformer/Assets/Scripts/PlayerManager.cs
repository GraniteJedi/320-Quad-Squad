using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Callbacks;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;

    [Header("Camera Settings")]
    private float lookPitch = 0f;
    private float lookYaw = 0f;
    private Vector3 cameraHeight;
    [SerializeField] float lookSensitivity = 0f;
    [SerializeField] float FOV = 90f;
    [SerializeField] Camera playerCamera;

    [Header("Move Settings")]
    [SerializeField] private Rigidbody playerBody;
    [SerializeField] private BoxCollider playerCollider;
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float speedMultiplyer = 0f;
    [SerializeField] private float gravBoost = 4f;
    [SerializeField] private float airResistance;
    [SerializeField] private int momentumLossDelay = 10;
    private int delayFrames = 0;

    [Header("Jump Settings")]
    [SerializeField] private float jumpSpeed = 10f;
    [SerializeField] private float wallJumpSpeed;
    [SerializeField] private float wallClingStrength;


    [Header("Slash Settings")]
    [SerializeField] private float slashDuration = 0.1f;
    [SerializeField] private float slashDistance = 5f;
    [SerializeField] private float slashSpeed = 150.2f;

    [Header("Slam/Slide Settings")]
    [SerializeField] private float slideTime = 2f;
    [SerializeField] private float slideSpeed = 15f;
    [SerializeField] private float slideCameraTime = 0.1f;
    [SerializeField] private float slideCameraHeight = 0.3f;
    [SerializeField] private float slideFriction = 0.5f;
    [SerializeField] private int frictionFrameDelay = 120;
    [SerializeField] private float slamSpeed = 30f;

    [Header("Speed Multiplyer Settings")]
    [SerializeField] private float slashMultiplyer = 1.8f;
    [SerializeField] private float slideMultiplyer = 1.5f;
    [SerializeField] private float slamMultiplyer = 2f;
    private Vector3 velocityVector;

    [Header("Grapple Settings")]
    [SerializeField] private float grappleRange;
    [SerializeField] private float grappleSpeed;
    [SerializeField] private float grappleStrength;

    [Header("Physics Materials")]
    [SerializeField] PhysicMaterial grounded;
    [SerializeField] PhysicMaterial onWall;

    private Quaternion worldToLocal;

    // Start is called before the first frame update
    void Start()
    {
        cameraHeight = playerCamera.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        worldToLocal = Quaternion.FromToRotation(Vector3.forward, playerBody.transform.forward);

        if (GetSpeed() <= moveSpeed && !inputManager.IsSliding())
        {
            delayFrames++;
            if (delayFrames >= momentumLossDelay)
            {
                speedMultiplyer = 1f;
                delayFrames = 0;
            }
        }
        else
        {
            delayFrames = 0;
        }

        if (!inputManager.IsSlashing() && !inputManager.IsSliding() && !inputManager.IsSlamming())
        {
            Vector3 maintainVelocity = Vector3.zero;

            if (velocityVector.x == 0)
            {
                maintainVelocity += playerBody.transform.right * Vector3.Dot(GetVelocity(), playerBody.transform.right);
            }
            if (velocityVector.z == 0)
            {
                maintainVelocity += playerBody.transform.forward * Vector3.Dot(GetVelocity(), playerBody.transform.forward);
            }

            SetVelocity(worldToLocal * (velocityVector * moveSpeed * speedMultiplyer) + maintainVelocity + GetVelocity().y * Vector3.up);
        }

        playerBody.AddForce(Vector3.down * gravBoost);
        playerBody.AddForce(Vector3.ProjectOnPlane(-GetVelocity(), playerBody.transform.up) * airResistance);
    }

    public void Move(Vector2 moveVector)
    {
        velocityVector = new Vector3(moveVector.x, 0, moveVector.y);
    }

    public void Jump()
    {
        inputManager.SetSlidingOff();
        inputManager.SetSlashingOff();
        SetVelocity(new Vector3(GetVelocity().x, 0f, GetVelocity().z) + Vector3.up * jumpSpeed);
    }

    public void WallJump(Vector3 wallNormal)
    {
        wallNormal = Vector3.ProjectOnPlane(wallNormal, playerBody.transform.up).normalized;

        SetVelocity(GetVelocity() + wallNormal * wallJumpSpeed);

        SetVelocity(new Vector3(GetVelocity().x, Mathf.Clamp(GetVelocity().y, 0f, 1f), GetVelocity().z) + Vector3.up * jumpSpeed);
    }

    public void Slide()
    {
        StartCoroutine(SlideMove());
    }

    public void Slam()
    {
        StartCoroutine(SlamMove());
    }

    public void SlideCancelled()
    {
        inputManager.SetSlidingOff();
    }

    public void GrappleCancel()
    {
        inputManager.SetGrapplingOff();
    }

    public void Grapple()
    {
        StartCoroutine(GrappleMove());
    }

    public void Swing()
    {
        ;
    }

    public void Look(Vector2 delta)
    {
        lookPitch -= delta.y * lookSensitivity;
        lookPitch = Mathf.Clamp(lookPitch, -90f, 90f);

        lookYaw = delta.x * lookSensitivity;

        playerCamera.transform.localRotation = Quaternion.Euler(lookPitch, 0f, 0f);
        playerBody.transform.rotation *= Quaternion.Euler(0f, lookYaw, 0f);
    }

    public void Slash()
    {
        StartCoroutine(SlashMove());
    }

    public void QuickMine()
    {
        ;
    }

    public void ToggleAltMode()
    {
        ;
    }

    public void Kamikaze()
    {
        ;
    }

    public IEnumerator SlashMove()
    {
        inputManager.SetSlammingOff();
        inputManager.SetSlidingOff();
        inputManager.SetSlashingOn();

        bool forcedNonzero = false;

        Vector3 initDirection;

        if (velocityVector == Vector3.zero)
        {
            forcedNonzero = true;
            initDirection = playerBody.transform.forward;
            velocityVector = playerBody.transform.forward;
        }
        else
        {
            initDirection = Quaternion.FromToRotation(Vector3.forward, playerBody.transform.forward) * velocityVector.normalized;
        }

        float elapsedTime = 0;
        speedMultiplyer = slashMultiplyer;
        delayFrames = 0;

        while (elapsedTime < slashDuration && inputManager.IsSlashing() && !inputManager.IsSliding())
        {
            SetVelocity((-2f * Mathf.Lerp(0, 0.5f, elapsedTime / slashDuration) + 2) * slashSpeed * initDirection);

            elapsedTime += Time.fixedDeltaTime;

            yield return new WaitForFixedUpdate();
        }

        if (forcedNonzero)
        {
            velocityVector = Vector3.zero;
        }

        inputManager.SetSlashingOff();
    }

    public IEnumerator SlideMove()
    {
        Vector3 slideDirection = playerBody.transform.forward;
        Vector3 groundNormal;
        Vector3 initCameraPos = playerCamera.transform.localPosition;
        RaycastHit hit;
        if(speedMultiplyer < slideMultiplyer)
            speedMultiplyer = slideMultiplyer;

        int frameCount = 0;

        float elapsedTime = 0;
        inputManager.SetSlidingOn();

        while (inputManager.IsSliding())
        {
            LowerCamera(initCameraPos, elapsedTime);

            if (!Physics.Raycast(playerCamera.transform.position, -playerBody.transform.up, out hit, 3.5f, LayerMask.GetMask("Ground")))
            {
                break;
            }

            groundNormal = hit.normal;

            Debug.Log(frameCount);
            if (frameCount >= frictionFrameDelay)
            {
                speedMultiplyer = Mathf.Clamp(speedMultiplyer - slideFriction * Time.fixedDeltaTime, 0, int.MaxValue);
                if (speedMultiplyer <= 0)
                    break;
            }

            SetVelocity(Vector3.ProjectOnPlane(slideDirection * slideSpeed * speedMultiplyer, groundNormal));

            elapsedTime += Time.fixedDeltaTime;
            frameCount += 1;
            yield return new WaitForFixedUpdate();
        }

        inputManager.SetSlidingOff();

        elapsedTime = 0;
        initCameraPos = playerCamera.transform.localPosition;

        while (elapsedTime < slideCameraTime)
        {
            RaiseCamera(initCameraPos, elapsedTime);

            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        playerCamera.transform.localPosition = cameraHeight;
    }

    public IEnumerator SlamMove()
    {
        inputManager.SetSlammingOn();

        speedMultiplyer = slamMultiplyer;
        delayFrames = 0;

        while (inputManager.IsSlamming())
        {
            SetVelocity(-playerBody.transform.up * slamSpeed);
            yield return new WaitForFixedUpdate();
        }
    }

    public IEnumerator GrappleMove()
    {
        Vector3 playerPos;
        Vector3 targetPos;
        Vector3 initPosition = playerBody.position;
        Vector3 direction = playerCamera.transform.forward;
        RaycastHit hit;

        if (!Physics.Raycast(initPosition, direction, out hit, grappleRange))
        {
            yield break;
        }

        playerPos = playerBody.position;
        targetPos = hit.collider.transform.position;

        while (inputManager.IsGrappling())
        {
            yield return new WaitForFixedUpdate();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            inputManager.SetOnWall(collision.contacts[0].normal);

            if (!inputManager.IsGrounded())
            {
                inputManager.SetSlashingOff();
                Debug.LogError("On Wall");
                playerCollider.material = onWall;
            }
        }
        else if (collision.gameObject.CompareTag("Ground") || collision.gameObject.layer == LayerMask.GetMask("Ground"))
        {
            inputManager.SetOnGround();
            Debug.LogError("On Ground");
            playerCollider.material = grounded;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            if(!inputManager.IsGrounded())
            {
                if (Vector3.Dot(-collision.contacts[0].normal, worldToLocal * velocityVector) >= 0)
                    speedMultiplyer = 0.1f;
                else
                    speedMultiplyer = 1.5f;

                delayFrames = 0;

                playerBody.AddForce(-collision.contacts[0].normal * wallClingStrength);
            }
        }
        else if (collision.gameObject.CompareTag("Ground") || collision.gameObject.layer == LayerMask.GetMask("Ground"))
        {
            inputManager.SetOnGround();
            playerCollider.material = grounded;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            inputManager.SetOffWall();
            Debug.LogError("Off Wall");
        }
        else if (collision.gameObject.CompareTag("Ground") || collision.gameObject.layer == LayerMask.GetMask("Ground"))
        {
            Debug.LogWarning("Off Ground");
            inputManager.SetOffGround();
        }
    }

    void SetVelocity(Vector3 newVelocity)
    {
        playerBody.velocity = newVelocity;
    }

    Vector3 GetVelocity()
    {
        return playerBody.velocity;
    }

    float GetSpeed()
    {
        return playerBody.velocity.magnitude;
    }

    void LowerCamera(Vector3 initPosition, float elapsedTime)
    {
        float clampedT = Mathf.Clamp01(elapsedTime / slideCameraTime);

        float t = Mathf.Lerp(0, 1, clampedT);

        playerCamera.transform.localPosition = Vector3.Lerp(initPosition, cameraHeight + Vector3.down * slideCameraHeight, t);
    }

    void RaiseCamera(Vector3 initPosition, float elapsedTime)
    {
        float clampedT = Mathf.Clamp01(elapsedTime / slideCameraTime);

        float t = Mathf.Lerp(0, 1, clampedT);

        playerCamera.transform.localPosition = Vector3.Lerp(initPosition, cameraHeight, t);
    }

    public void SetAirResistance(float newResistance)
    {
        airResistance = newResistance;
    }
}
