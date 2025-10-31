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
    [SerializeField] private Vector3 totalVelocity;
    private Vector3 normalForce;

    [Header("Projetile Settings")]
    [SerializeField] private float projectileDecreaseSpeed;
    [SerializeField] private float projectileMinimumForce;
    [SerializeField] private float projectileDamage;

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
    [SerializeField] private float grappleStrength;
    [SerializeField] private LineRenderer grapplingHook;
    [SerializeField] private Transform hookEnd;
    private bool grappling;
    private Vector3 currentGrapplePoint;

    [Header("Colission Settings")]
    [SerializeField]private bool isGrounded = false;
    [SerializeField] private bool isTouchingWall = false;
    [SerializeField] private DashCooldown dashCooldownListener;
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
        normalForce = Vector3.zero;
        groundMask = LayerMask.GetMask("Ground");
        wallMask = LayerMask.GetMask("Wall");
        inAirJump = true;
        sliding = false;


        isGrounded = false;
        isTouchingWall = false;

        dashCooldownListener = GameObject.FindAnyObjectByType<DashCooldown>();
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

        if (grappling)
        {
            grapplingHook.SetPosition(0, hookEnd.position);
        }
    }

    void FixedUpdate()
    {
        HandleMovement();
        ApplyGravity();
        ApplyFrictionAndResistance();

        totalVelocity = walkVelocity + jumpVelocity + wallJumpVelocity + slashVector + normalForce + projectileVector;

        playerBody.transform.position = playerBody.transform.position + totalVelocity * Time.deltaTime;

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
        if (directionWASD.sqrMagnitude > 0.01f && !sliding)
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
        else if(!sliding)
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

           // Debug.Log(theta);
        }
    }

    private void ApplyGravity()
    {
        if (!isGrounded)
        {
            jumpVelocity.y -= gravityStrength * Time.fixedDeltaTime;
            wallJumpVelocity.y -= gravityStrength * Time.fixedDeltaTime;
        }
        else
        {
            
           if(currentGroundNormal.y < 0)
           {
               wallJumpVelocity.y = -hitByAboveNormal;
               jumpVelocity.y = -hitByAboveNormal;
               jumpVelocity.y -= gravityStrength * Time.fixedDeltaTime;
               wallJumpVelocity.y -= gravityStrength * Time.fixedDeltaTime;
           }
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

    public void GrappleStart(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            if (grappleCooldownTimer > 0) return;
            grappling = true;

            RaycastHit hitHook;
            if (Physics.Raycast(
                playerCamera.transform.position,
                playerCamera.transform.forward,
                out hitHook,
                grappleRange,
                wallMask))
            {
                currentGrapplePoint = hitHook.point;

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

    }
    public void GrappleEnd()
    {
        grappling = false;
        grappleCooldownTimer = grappleCooldown;
        grapplingHook.enabled = false;
    }

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
                isGrounded = true;
                inAirJump = false;
                currentGroundNormal = contact.normal;
                wallJumpVelocity = Vector3.zero;
                jumpVelocity = Vector3.zero;
            }

            if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
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
            currentWallNormal = Vector3.zero;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Projectile")
        {
            //SASHA HELP
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

    

