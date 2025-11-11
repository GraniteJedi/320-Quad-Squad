using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem.Controls;
using Unity.VisualScripting;
using System.Runtime.CompilerServices;
using System.Net.Mime;
using Unity.Mathematics;
using System.Security.Cryptography;
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
    [SerializeField] private float rotationSmoothTime;
    private Vector3 totalVelocity;
    private Vector3 normalForce;
    private float defaultCapsuleHeight;
    private float defaultCapsuleRadius;
    public Vector3 TotalVelocity
    {
        get { return totalVelocity; }
    }

    [Header("Projetile Settings")]
    [SerializeField] private float projectileDecreaseSpeed;
    [SerializeField] private float projectileMinimumForce;
    [SerializeField] private float projectileDamage;


    [Header("Move Settings")]
    [SerializeField] private float moveSpeedMax = 12f;
    [SerializeField] private float walkAcceleration;
    [SerializeField] private float walkDecceleration;
    private Vector3 acceleration;
    private Vector2 directionWASD;
    private Vector3 walkVelocity;

    [Header("Jump Settings")]
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float hitByAboveNormal;
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
    [SerializeField] private float slashCooldown = 1.5f;
    [SerializeField] private int slashes = 3;
    private int remainingSlashes;

    [Header("Grapple Settings")]
    [SerializeField] private float grappleRange;
    [SerializeField] private float grappleDelayTime;
    [SerializeField] private float grappleCooldown;
    private float grappleCooldownTimer;
    [SerializeField] private float grappleStrengthMultiplier; 
    [SerializeField] private float grappleOvershootYAxis;
    [SerializeField] private LineRenderer grapplingHook;
    [SerializeField] private Transform hookEnd;
    [SerializeField] private float grapplePullTime;
    private bool grapplingCalculation;
    private bool activeGrapple;
    private Vector3 currentGrapplePoint;
    private Vector3 grappleVelocity;
    private float pullTick;

    [Header("Colission Settings")]
    [SerializeField]private bool isGrounded = false;
    [SerializeField] private bool isTouchingWall = false;
    [SerializeField] private DashCooldown dashCooldownListener;
    [SerializeField] private float onGroundLenience = 0.3f;
    private float elapsedSlashCooldown = 0;
    private Vector3 currentWallNormal;
    private Vector3 currentGroundNormal;

    private Vector3 projectileVector;
    private Vector3 looking;
    /*



    [Header("Speed Multiplyer Settings")]
    [SerializeField] private float slashMultiplyer = 1.8f;
    [SerializeField] private float slideMultiplyer = 1.5f;
    [SerializeField] private float slamMultiplyer = 2f;
    private Vector3 velocityVector;



    [Header("Physics Materials")]
    [SerializeField] PhysicMaterial grounded;
    [SerializeField] PhysicMaterial onWall;


    */

    [Header("Other")]
    [SerializeField] UIManager uiManager;
    [SerializeField] PlayerInput playerInput;

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
        grappleVelocity = Vector3.zero;
        normalForce = Vector3.zero;
        groundMask = LayerMask.GetMask("Ground");
        wallMask = LayerMask.GetMask("Wall");
        inAirJump = true;
        sliding = false;
        activeGrapple = false;
        pullTick = 0;

        isGrounded = false;
        isTouchingWall = false;

        //dashCooldownListener = GameObject.FindAnyObjectByType<DashCooldown>();
        remainingSlashes = slashes;
        defaultCapsuleHeight = playerCollider.height;
        defaultCapsuleRadius = playerCollider.radius;
}

    // Update is called once per frame
    void Update()
    {
        HandleLook();
        HandleSlashCooldown();
        if (grappleCooldownTimer > 0)
        {
            grappleCooldownTimer -= Time.deltaTime;
        }

        if (grapplingCalculation)
        {
            grapplingHook.SetPosition(0, hookEnd.position);
        }
    }

    void FixedUpdate()
    {
        HandleMovement();
        ApplyGravity();
        ApplyFrictionAndResistance();

        if (!activeGrapple)
        {
            pullTick = 0;
            totalVelocity = walkVelocity + jumpVelocity + wallJumpVelocity + slashVector + projectileVector + grappleVelocity;
        }
        else
        {
            pullTick += Time.deltaTime;
            if(pullTick > grapplePullTime)
            {
                GrappleEnd();
                activeGrapple = false;
                pullTick = 0;
                if (!isGrounded)
                {
                    inAirJump = true;
                }
            }
            Debug.Log(pullTick);
            Vector3 walkContribution = walkVelocity * Vector3.Dot(Vector3.Normalize(grappleVelocity), Vector3.Normalize(walkVelocity));
            Vector3 slashContribution = slashVector * Vector3.Dot(Vector3.Normalize(grappleVelocity), Vector3.Normalize(slashVector));
            totalVelocity = grappleVelocity + walkContribution + slashContribution;
        }

        if (isGrounded && Vector3.Dot(currentGroundNormal, Vector3.up) > 0.1f && !activeGrapple)
        {
            totalVelocity = Vector3.ProjectOnPlane(totalVelocity, currentGroundNormal);
        }

        playerBody.velocity = (totalVelocity);

        projectileVector -= (projectileVector * projectileDecreaseSpeed);
        if (projectileVector.magnitude < projectileMinimumForce)
        {
            projectileVector = Vector3.zero;
        }

    }

    private void HandleMovement()
    {
        #region Walking
        Vector3 acceleration = Vector3.zero;

        //Walking handler
        if (directionWASD.sqrMagnitude > 0.01f && !sliding && !activeGrapple)
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
        else if(!sliding && !activeGrapple)
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
        #endregion
        #region Sliding
        //Sliding handler
        if (sliding)
        {
            
            playerCamera.transform.localPosition = new Vector3(
                cameraHeightReset.x,
                slideCameraHeight,
                cameraHeightReset.z
            );
            
            playerCollider.height = slideColliderHeight;
            playerCollider.radius = slideColliderHeight;
            //Come to a stop with friction
            walkVelocity *= 1f - (slideFriction * Time.fixedDeltaTime);
            if (walkVelocity.sqrMagnitude < 0.1f)
            {
                walkVelocity = Vector3.zero;
            }
        }
        else
        {
            playerCamera.transform.localPosition = cameraHeightReset;
            playerCollider.height = defaultCapsuleHeight;
            playerCollider.radius = defaultCapsuleRadius;
        }
        #endregion
        #region Wall Collision


        totalVelocity.x = totalVelocity.x - (currentWallNormal.x * -totalVelocity.x);
        totalVelocity.y = totalVelocity.y - (currentWallNormal.y * -totalVelocity.y);
        totalVelocity.z = totalVelocity.z - (currentWallNormal.z * -totalVelocity.z);
        if (totalVelocity.x == 0 && currentWallNormal != Vector3.zero)
        {
            totalVelocity.x = 0;
        }
        #endregion
    }

    private void HandleSlashCooldown()
    {
        if (elapsedSlashCooldown > 0)
        {
            elapsedSlashCooldown -= Time.deltaTime;
            elapsedSlashCooldown = elapsedSlashCooldown <= 0 ? 0 : elapsedSlashCooldown;

            dashCooldownListener.ManualUpdate();
        }
        else
        {
            if (dashCooldownListener.Refill())
            {
                remainingSlashes++;
            }
            
            if (remainingSlashes < slashes)
            {
                elapsedSlashCooldown = slashCooldown;
            }
        }
    }

    private void ApplyGravity()
    {
        if (!isGrounded)
        {
            if(inAirJump)
            {
                jumpVelocity.y -= gravityStrength * Time.fixedDeltaTime;
                wallJumpVelocity.y -= gravityStrength * Time.fixedDeltaTime;
            }
        }
        else if (activeGrapple)
        {
            grappleVelocity.y -= gravityStrength * Time.fixedDeltaTime;
        }
    }

    private void ApplyFrictionAndResistance()
    {
        slashVector *= 1f - (generalAirResistance * Time.fixedDeltaTime);
        if (slashVector.sqrMagnitude < 0.1f)
        {
            slashVector = Vector3.zero;
        }
        if (!activeGrapple)
        {
            grappleVelocity *= 1f - (generalAirResistance * Time.fixedDeltaTime);
            if (grappleVelocity.sqrMagnitude < 0.1f)
            {
                grappleVelocity = Vector3.zero;
            }
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
        sliding = context.performed && isGrounded && !activeGrapple;
    }

    public void Slash(InputAction.CallbackContext context)
    {
        if (context.performed && remainingSlashes > 0)
        {
            slashVector = playerBody.transform.forward * slashSpeed;
            dashCooldownListener.Dash();

            if (elapsedSlashCooldown == 0)
                elapsedSlashCooldown = slashCooldown;
            
            remainingSlashes--;
        }

        
        if(slashVector.y > 0 || !isGrounded)
        {
            inAirJump = true;
        }
        if (isGrounded && slashVector.y < 0)
        {
            slashVector.y = 0;
        }
    }

    //All grapplingCalculation calculations and methods are here
    #region Grappling
    public void GrappleStart(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            if (grappleCooldownTimer > 0) return;
            grapplingCalculation = true;
            RaycastHit hitHook;
            if (Physics.Raycast(
                playerCamera.transform.position,
                playerCamera.transform.forward,
                out hitHook,
                grappleRange,
                wallMask))
            {
                currentGrapplePoint = hitHook.point;
                isGrounded = false;
                if (inAirJump)
                {
                    jumpVelocity = Vector3.zero;
                    wallJumpVelocity = Vector3.zero;
                    inAirJump = false;
                }
                Invoke(nameof(GrappleExecute), grappleDelayTime);
            }
            else
            {
                currentGrapplePoint = playerCamera.transform.position + playerCamera.transform.forward * grappleRange;

                Invoke(nameof(GrappleEnd), grappleDelayTime);
            }

            grapplingHook.enabled = true;
            grapplingHook.SetPosition(1, currentGrapplePoint);
        }
    }
    public void GrappleExecute()
    {


        Vector3 lowestPointPlayer = new Vector3(playerBody.transform.position.x, 
                                                playerBody.transform.position.y-.5f, 
                                                playerBody.transform.position.z);
        float relativeHeight = currentGrapplePoint.y - lowestPointPlayer.y;

        float highestPoint = relativeHeight + grappleOvershootYAxis;
        if (relativeHeight < 0)
        {
            highestPoint = grappleOvershootYAxis;
        }
        
        JumpToPosition(currentGrapplePoint, highestPoint);
        Invoke(nameof(GrappleEnd), grapplePullTime);
    }
    public void GrappleEnd()
    {
        grapplingCalculation = false;
        grappleCooldownTimer = grappleCooldown;
        grapplingHook.enabled = false;
    }

    private Vector3 velocityToSetAsJump;
    /// <summary>
    /// Sets a velocity to run as the calculated jump
    /// Only sets the new velocity to the grapple velocity after a small time delay
    /// </summary>
    /// <param name="targetPosition"></param>
    /// <param name="trajectoryHeight"></param>
    private void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        activeGrapple = true;
        velocityToSetAsJump = CalculateJumpVelocity(playerBody.transform.position,targetPosition, trajectoryHeight);

        Invoke(nameof(SetVelocity), 0.1f);
    }
    private void SetVelocity()
    {
        grappleVelocity = velocityToSetAsJump;

    }
    private Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        //Physical displacement (how far do I have to go at max)
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacememtXZ = new Vector3 (endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        //Calculating directional velocities
        //XZ velocity is determined by the time it takes to go all the way up then down to the target
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * -gravityStrength * trajectoryHeight);

        Vector3 velocityXZ = displacememtXZ / (Mathf.Sqrt(-2 * trajectoryHeight / -gravityStrength)
                                               + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / -gravityStrength));
        return (velocityXZ + velocityY) * grappleStrengthMultiplier;
    }
    #endregion
    public void Slam()
    {

    }


    public void Look(InputAction.CallbackContext context)
    {
        looking = context.ReadValue<Vector2>();
    }

    public void HandleLook()
    {
        lookPitch -= looking.y * lookSensitivity;
        lookPitch = Mathf.Clamp(lookPitch, -90f, 90f);

        lookYaw = Mathf.LerpAngle(lookYaw, lookYaw + looking.x * lookSensitivity, rotationSmoothTime);

        playerCamera.transform.localRotation = Quaternion.Euler(lookPitch, 0f, 0f);
        playerBody.transform.rotation = Quaternion.Euler(0f, lookYaw, 0f);
    }

    public void SwitchMap(string map)
    {
        playerInput.SwitchCurrentActionMap(map);
    }

    public void Pause(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            SwitchMap("UI");
            uiManager.Pause();
        }
    }

    public void Resume()
    {
        SwitchMap("Player");
    }

    void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {


            if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                if (Vector3.Dot(collision.contacts[0].normal, Vector3.up) >= onGroundLenience)
                {
                    isGrounded = true;
                    inAirJump = false;
                    currentGroundNormal = contact.normal;
                    wallJumpVelocity = Vector3.zero;
                    jumpVelocity = Vector3.zero;
                    activeGrapple = false;
                    GrappleEnd();
                    grappleVelocity = Vector3.zero;
                }
            }

            if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
            {
                isTouchingWall = true;
                if(activeGrapple)
                {
                    inAirJump = true;
                }
                activeGrapple = false;
                GrappleEnd();
                currentWallNormal = contact.normal;
                grappleVelocity = Vector3.zero;
            }
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (Vector3.Dot(collision.contacts[0].normal, Vector3.up) >= onGroundLenience)
            {
                isGrounded = true;
                currentGroundNormal = collision.contacts[0].normal;
            }
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            isTouchingWall = true;
            currentWallNormal = Vector3.zero;
        }
    }


    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = false;
            if (!activeGrapple)
            {
                inAirJump = true;
            }
            currentGroundNormal = Vector3.zero;
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            isTouchingWall = false;
            currentWallNormal = Vector3.zero;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Projectile")
        {

            projectileVector = other.GetComponent<Rigidbody>().velocity.normalized * projectileDamage;
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

    public void SetSensitivity(float sensitivity)
    {
        lookSensitivity = sensitivity;
    }

    public float GetSensitivity()
    {
        return lookSensitivity;
    }

    public float GetSlashCooldown()
    {
        return slashCooldown;
    }

    public float GetElapsedSlashCooldown()
    {
        return elapsedSlashCooldown;
    }

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

    

