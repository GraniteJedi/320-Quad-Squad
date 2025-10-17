using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem.Controls;
using Unity.VisualScripting;
using System.Runtime.CompilerServices;
using System.Net.Mime;
public class PlayerManager : MonoBehaviour
{

    [Header("Camera Settings")]
    private float lookPitch = 0f;
    private float lookYaw = 0f;
    private Vector3 cameraHeight;
    private Vector3 cameraHeightReset;
    [SerializeField] float lookSensitivity = 0f;
    [SerializeField] float FOV = 90f;
    [SerializeField] Camera playerCamera;

    [Header("Player Settings")]
    [SerializeField] private Rigidbody playerBody;
    [SerializeField] private CapsuleCollider playerCollider;
    [SerializeField] private float gravityStrength;
    [SerializeField] private float generalAirResistance;
    private Vector3 totalVelocity;
    private Vector3 normalForce;

    public Vector3 TotalVelocity
    {
        get { return totalVelocity; }
    }


    [Header("Move Settings")]
    [SerializeField] private float moveSpeedMax = 12f;
    [SerializeField] private float walkAcceleration;
    [SerializeField] private float walkDecceleration;
    private Vector3 acceleration;
    private Vector2 directionWASD;
    private Vector3 walkVelocity;
    
    [Header("Jump Settings")]
    [SerializeField] private float jumpSpeed;
    private bool inAirJump = false;
    private Vector3 jumpVelocity;
    private LayerMask groundMask;
    private bool hittingWallForJump = false;

    [Header("Wall Jump Settings")]
    [SerializeField] private float wallJumpUpForce;
    [SerializeField] private float wallJumpSideForce;
    private Vector3 wallJumpVelocity;
    private LayerMask wallMask;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private bool wallLeft;
    private bool wallRight;

    [Header("Slide Settings")]
    [SerializeField] private float slideCameraHeight;
    [SerializeField] private float slideColliderHeight;
    [SerializeField] private float slideFriction;
    private bool sliding;

    [Header("Slash Settings")]
    [SerializeField] private float slashSpeed = 150f;
    private bool slashing;
    private Vector3 slashVector;

    private bool isGrounded = false;
    private bool isTouchingWall = false;
    private Vector3 currentWallNormal;
    /*



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


    */

    [Header("Other")]
    [SerializeField] UIManager uiManager;

    private Quaternion worldToLocal;
    private Vector3 spawnPoint;

    // Start is called before the first frame update
    void Start()
    {
        cameraHeight = playerCamera.transform.localPosition;
        cameraHeightReset = playerCamera.transform.localPosition;
        spawnPoint = playerBody.transform.position;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        jumpVelocity = Vector3.zero;
        walkVelocity = Vector3.zero;
        groundMask = LayerMask.GetMask("Ground");
        wallMask = LayerMask.GetMask("Wall");
        inAirJump = true;

        isGrounded = false;
        isTouchingWall = false;
}

    // Update is called once per frame
    void Update()
    {
       
    }

    void FixedUpdate()
    {
        HandleMovement();
        ApplyGravity();
        ApplyFrictionAndResistance();

        Vector3 totalVelocity = walkVelocity + jumpVelocity + wallJumpVelocity + slashVector;

        playerBody.velocity = totalVelocity;
       

        //Collisions
        /* Sasha
        RaycastHit hit;
        if (inAirJump && jumpVelocity.y < 0)
        {
            if (Physics.Raycast(playerBody.transform.position, -playerBody.transform.up, out hit, 1.6f, groundMask))
            {
                Debug.Log("hit");
                inAirJump = false;
                jumpVelocity = Vector3.zero;
                wallJumpVelocity = Vector3.zero;
            }
        }
        if (!inAirJump)
        {
            if (!Physics.Raycast(playerBody.transform.position, -playerBody.transform.up, out hit, 1.6f, groundMask))
            {

                jumpVelocity += -playerBody.transform.up * gravityStrength;
            }
        }
        if (inAirJump)
        {
            wallRight = Physics.Raycast(playerBody.transform.position, playerBody.transform.right, out rightWallHit, .5f, wallMask);
            wallLeft = Physics.Raycast(playerBody.transform.position, -playerBody.transform.right, out leftWallHit, .5f, wallMask);
        }

        if (wallLeft || wallRight)
        {
            hittingWallForJump = true;
        }
        else
        {
            hittingWallForJump = false;
        }


        //Movement
        totalVelocity = slashVector + wallJumpVelocity + jumpVelocity + walkVelocity;
        if (sliding)
        {
            cameraHeight.y = slideCameraHeight;
            playerCamera.transform.localPosition = cameraHeight;
            walkVelocity *= 1f - (slideFriction * Time.fixedDeltaTime);
            Debug.Log(walkVelocity.sqrMagnitude);
            if (walkVelocity.sqrMagnitude < .1f)
            {
                walkVelocity = Vector3.zero;
            }
        }
        else
        {
            playerCamera.transform.localPosition = cameraHeightReset;
            if (directionWASD.sqrMagnitude >= 1)
            {
                if (inAirJump)
                {
                    acceleration = playerBody.transform.forward * directionWASD.y * walkAcceleration / 4 +
                                   playerBody.transform.right * directionWASD.x * walkAcceleration / 4;
                    walkVelocity += acceleration * Time.deltaTime;
                    walkVelocity = Vector3.ClampMagnitude(walkVelocity, moveSpeedMax);
                }
                else
                {
                    acceleration = playerBody.transform.forward * directionWASD.y * walkAcceleration +
                   playerBody.transform.right * directionWASD.x * walkAcceleration;
                    walkVelocity += acceleration * Time.deltaTime;
                    walkVelocity = Vector3.ClampMagnitude(walkVelocity, moveSpeedMax);
                }
            }
            else
            {
                if (inAirJump)
                {
                    walkVelocity *= 1f - (walkDecceleration / 10 * Time.fixedDeltaTime);
                    if (walkVelocity.sqrMagnitude < .1f)
                    {
                        walkVelocity = Vector3.zero;
                    }
                }
                else
                {
                    walkVelocity *= 1f - (walkDecceleration * Time.fixedDeltaTime);
                    if (walkVelocity.sqrMagnitude < .1f)
                    {
                        walkVelocity = Vector3.zero;
                    }
                }
            }
            slashVector *= 1f - (generalAirResistance * Time.fixedDeltaTime);
            if(slashVector.sqrMagnitude < .1f)
            {
                slashVector = Vector3.zero;
            }
        
        }

        //The colission system so far
        //The idea is that when you come in contaxt with an object labeled as a wall
        //It applies a velocity to you that is equal and opposite to the one you apply to its normal face
        // If you're moving side to side it should only stop your forward movement
        RaycastHit hittingWallForward;
        RaycastHit hittingWallRight;
        if (Physics.Raycast(playerBody.transform.position, transform.forward, out hittingWallForward, .5f, wallMask))
        {
            totalVelocity.x = totalVelocity.x - hittingWallForward.normal.x * totalVelocity.x + 1;
            totalVelocity.z = totalVelocity.z - hittingWallForward.normal.z * totalVelocity.z + 1;
        }
        if(Physics.Raycast(playerBody.transform.position, transform.right, out hittingWallRight, .5f, wallMask))
        {
            Debug.Log("hit");
            totalVelocity.x = totalVelocity.x - (-hittingWallRight.normal.x) * totalVelocity.x + 1;
            totalVelocity.z = totalVelocity.z - (-hittingWallRight.normal.z) * totalVelocity.z + 1;
        }


        if (inAirJump)
        {
            jumpVelocity.y = jumpVelocity.y-(gravityStrength * Time.fixedDeltaTime);
            wallJumpVelocity.y = wallJumpVelocity.y - (gravityStrength * Time.fixedDeltaTime);
        }


        playerBody.transform.position = playerBody.transform.position + totalVelocity * Time.fixedDeltaTime;
        */
    }

    private void HandleMovement()
    {
        Vector3 acceleration = Vector3.zero;

        if (directionWASD.sqrMagnitude > 0.01f)
        {
            float currentAccel;

            if (inAirJump)
            {
                currentAccel = walkAcceleration * 0.25f;
            }
            else
            {
                currentAccel = walkAcceleration;
            }

            Vector3 forward = playerBody.transform.forward;
            Vector3 right = playerBody.transform.right;

            acceleration = (forward * directionWASD.y + right * directionWASD.x) * currentAccel;
            walkVelocity += acceleration * Time.fixedDeltaTime;
            walkVelocity = Vector3.ClampMagnitude(walkVelocity, moveSpeedMax);
        }
        else
        {
            float deccelFactor;

            if (inAirJump)
            {
                deccelFactor = walkDecceleration * 0.1f;
            }
            else
            {
                deccelFactor = walkDecceleration;
            }

            walkVelocity *= 1f - (deccelFactor * Time.fixedDeltaTime);

            if (walkVelocity.sqrMagnitude < 0.1f)
            {
                walkVelocity = Vector3.zero;
            }
        }

        if (sliding)
        {
            playerCamera.transform.localPosition = new Vector3(
                cameraHeightReset.x,
                slideCameraHeight,
                cameraHeightReset.z
            );

            walkVelocity *= 1f - (slideFriction * Time.fixedDeltaTime);

            if (walkVelocity.sqrMagnitude < 0.1f)
            {
                walkVelocity = Vector3.zero;
            }
        }
        else
        {
            playerCamera.transform.localPosition = cameraHeightReset;
        }
    }

    private void ApplyGravity()
    {
        if (!isGrounded)
        {
            
            jumpVelocity.y -= gravityStrength * Time.fixedDeltaTime;
            wallJumpVelocity.y -= gravityStrength * Time.fixedDeltaTime;
        }
    }

    private void ApplyFrictionAndResistance()
    {
        slashVector *= 1f - (generalAirResistance * Time.fixedDeltaTime);
        if (slashVector.sqrMagnitude < 0.1f)
        {
            slashVector = Vector3.zero;
        }
            
    }


    public void Moving(InputAction.CallbackContext context)
    {
        directionWASD = context.ReadValue<Vector2>();
       
    }


    public void Jump()
    {
        if (isGrounded)
        {
            jumpVelocity = new Vector3(0, jumpSpeed, 0);
            isGrounded = false;
            inAirJump = true;
        }
    }

    public void WallJump()
    {
        if (isTouchingWall)
        {
            wallJumpVelocity = Vector3.up * wallJumpUpForce + currentWallNormal * wallJumpSideForce;
            inAirJump = true;
        }
    }

    public void Slide(InputAction.CallbackContext context)
    {
        sliding = context.performed && isGrounded;
    }

    public void Slash()
    {
        slashVector = playerBody.transform.forward * slashSpeed;
        if(slashVector.y > 0 || !isGrounded)
        {
            inAirJump = true;
        }
        if (isGrounded && slashVector.y < 0)
        {
            slashVector.y = 0;
        }
    }

    public void Slam()
    {

    }

    public void GrappleCancel()
    {

    }

    public void Grapple()
    {

    }

    public void Look(InputAction.CallbackContext context)
    {
        Vector2 looking = context.ReadValue<Vector2>();
        lookPitch -= looking.y * lookSensitivity;
        lookPitch = Mathf.Clamp(lookPitch, -90f, 90f);
        lookYaw = looking.x * lookSensitivity;

        playerCamera.transform.localRotation = Quaternion.Euler(lookPitch, 0f, 0f);
        playerBody.transform.rotation *= Quaternion.Euler(0f, lookYaw, 0f);
    }

    void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            float angle = Vector3.Angle(contact.normal, Vector3.up);

            if (angle < 45f && collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                isGrounded = true;
                inAirJump = false;
                jumpVelocity = Vector3.zero;
                wallJumpVelocity = Vector3.zero;
            }

            if (angle >= 45f && collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
            {
                isTouchingWall = true;
                currentWallNormal = contact.normal;
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = false;
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            isTouchingWall = false;
        }
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
    /*
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
        if (speedMultiplyer < slideMultiplyer)
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
        float currentLength = 0;
        Vector3 initPosition;

        while (currentLength < grappleRange && inputManager.IsGrappling())
        {
            yield return new WaitForFixedUpdate();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall") && !inputManager.IsGrounded())
        {
            inputManager.SetOnWall(collision.contacts[0].normal);
            inputManager.SetSlashingOff();
            Debug.LogError("On Wall");
            playerCollider.material = onWall;

            //playerBody.walkVelocity -= Vector3.Project(playerBody.walkVelocity, -collision.contacts[0].normal);

            if (Vector3.Dot(-collision.contacts[0].normal, worldToLocal * velocityVector) >= 0)
                speedMultiplyer = 0.1f;
            else
                speedMultiplyer = 1.5f;

            playerBody.AddForce(-collision.contacts[0].normal * wallClingStrength);

            delayFrames = 0;
        }
        // else if (collision.gameObject.CompareTag("Ground"))
        // {
        //     inputManager.SetOnGround();
        //     Debug.LogError("On Ground");
        //     playerCollider.material = grounded;
        // }
    }
    
    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall") && !inputManager.IsGrounded())
        {
            inputManager.SetOnWall(collision.contacts[0].normal);
            //playerBody.walkVelocity -= Vector3.Project(playerBody.walkVelocity, -collision.contacts[0].normal);

            //SetVelocity(GetVelocity() + collision.contacts[0].normal * Mathf.Clamp(Vector3.Dot(GetVelocity(), -collision.contacts[0].normal), 0f, 100f));

            if (Vector3.Dot(-collision.contacts[0].normal, worldToLocal * velocityVector) >= 0)
                speedMultiplyer = 0.1f;
            else
                speedMultiplyer = 1.5f;

            playerBody.AddForce(-collision.contacts[0].normal * wallClingStrength);

            delayFrames = 0;
        }
        // if (collision.gameObject.CompareTag("Ground"))
        // {
        //     inputManager.SetOnGround();
        //     playerCollider.material = grounded;
        // }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            inputManager.SetOffWall();
            Debug.LogWarning("Off Wall");
        }
        // else if (collision.gameObject.CompareTag("Ground"))
        // {
        //     Debug.LogWarning("Off Ground");
        //     inputManager.SetOffGround();
        // }
    }

    void SetVelocity(Vector3 newVelocity)
    {
        playerBody.walkVelocity = newVelocity;
    }

    Vector3 GetVelocity()
    {
        return playerBody.walkVelocity;
    }

    float GetSpeed()
    {
        return playerBody.walkVelocity.magnitude;
    }

    void LowerCamera(Vector3 initPosition, float elapsedTime)
    {
        float clampedT = Mathf.Clamp01(elapsedTime / slideCameraTime);

        float t = Mathf.Lerp(0, 1, clampedT);

        playerCamera.transform.localPosition = Vector3.Lerp(initPosition, cameraHeight + Vector3.down * slideCameraHeight, t);
        float heightChange = playerCamera.transform.localPosition.y - initPosition.y;
        playerCollider.height = slideColliderHeight;
        //playerBody.transform.position -= Vector3.up * heightChange;
    }

    void RaiseCamera(Vector3 initPosition, float elapsedTime)
    {
        float clampedT = Mathf.Clamp01(elapsedTime / slideCameraTime);

        float t = Mathf.Lerp(0, 1, clampedT);

        playerCamera.transform.localPosition = Vector3.Lerp(initPosition, cameraHeight, t);
        playerCollider.height = 3;
    }

    public void SetAirResistance(float newResistance)
    {
        airResistance = newResistance;
    }

    public void SetOnGround()
    {
        //Debug.LogWarning("OnGround");
        playerCollider.material = grounded;
    }

    public void SetOffGround()
    {
        //Debug.LogWarning("OffGround");
        playerCollider.material = onWall;
    }
    */
    public void ResetPlayer()
    {
        uiManager.AddDialogue(new UIManager.Dialogue("You ran out of time. Try again.", true));
        playerBody.transform.position = spawnPoint;
        playerBody.velocity = Vector3.zero;
    }
    public void KillPlayer()
    {
        uiManager.AddDialogue(new UIManager.Dialogue("You Died. Try again.", true));
        playerBody.transform.position = spawnPoint;
        playerBody.velocity = Vector3.zero;
    }
}

    

