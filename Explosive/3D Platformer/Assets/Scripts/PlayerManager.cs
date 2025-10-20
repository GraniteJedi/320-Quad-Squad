using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using Unity.VisualScripting;
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
    [SerializeField] private CapsuleCollider playerCollider;
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
    [SerializeField] private float slideColliderHeight = 1f;
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

    [Header("Other")]
    [SerializeField] UIManager uiManager;

    private Quaternion worldToLocal;
    private Vector3 spawnPoint;

    // Start is called before the first frame update
    void Start()
    {
        cameraHeight = playerCamera.transform.localPosition;
        spawnPoint = playerBody.transform.position;
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

            if (inputManager.IsOnWall() && Vector3.Dot(-inputManager.GetWallNormal(), worldToLocal * velocityVector) >= 0)
            {
                Debug.LogWarning("Projecting");
                SetVelocity(Vector3.ProjectOnPlane(worldToLocal * (velocityVector * moveSpeed * speedMultiplyer), inputManager.GetWallNormal()) + maintainVelocity + GetVelocity().y * Vector3.up);
            }
            else
            {
                SetVelocity(Vector3.ProjectOnPlane(worldToLocal * (velocityVector * moveSpeed * speedMultiplyer), inputManager.GetGroundNormal()) + maintainVelocity + GetVelocity().y * Vector3.up);
            }
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

            //playerBody.velocity -= Vector3.Project(playerBody.velocity, -collision.contacts[0].normal);

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
            //playerBody.velocity -= Vector3.Project(playerBody.velocity, -collision.contacts[0].normal);

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

    public void ResetPlayer()
    {
        uiManager.AddDialogue(new UIManager.Dialogue("You ran out of time. Try again.", true));
        playerBody.transform.position = spawnPoint;
    }
}


//sasha code
/*

public class PlayerManager : MonoBehaviour
{
    //[SerializeField] private InputManager inputManager;

    //Physics Settings - to add to physics class defaults
    [Header("Physics Settings")]
    private PhysicsUnity physicsManager = new PhysicsUnity();
    [SerializeField] float mass;
    [SerializeField] float gravity;
    [SerializeField] private float airResistance;
    private float worldUpdateTime;
    private bool grounded;


    [Header("Move Settings")]
    [SerializeField] private Rigidbody playerBody;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float maxSpeed;
    private bool moving;
    private Vector2 regMoveVector;

    [Header("Jump Settings")]
    [SerializeField] float jumpStrength;
    private bool goingUp;

    [Header("Slide Settings")]
    [SerializeField] float slideSpeed;
    private bool crouched;

    [Header("Slash Settings")]
    [SerializeField] float slashStrength;

    [Header("Camera Settings")]
    private float lookPitch = 0f;
    private float lookYaw = 0f;
    private Vector3 cameraHeight;
    [SerializeField] float lookSensitivity = 0f;
    [SerializeField] float FOV = 90f;
    [SerializeField] Camera playerCamera;

    [Header("Colision Detection")]
    [SerializeField] private LayerMask ground;
    [SerializeField] private LayerMask wall;

    [Header("Other")]
    [SerializeField] private Countdown countdown;
    //  [SerializeField] private float wallJumpSpeed;
    //  [SerializeField] private float wallClingStrength;

    [Header("Jump Settings")]
    [SerializeField] private float jumpSpeed = 10f;
    [SerializeField] private float wallJumpSpeed;


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
        cameraHeight = playerCamera.transform.position.y;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        physicsManager.Physics(playerBody.position.x, playerBody.position.y, playerBody.position.z);
        physicsManager.Mass = mass;
        physicsManager.Gravity = gravity;
        grounded = true;
        crouched = false;


        countdown.SetPhysicsManager(physicsManager);
    }

    // Update is called once per frame
    void Update()
    {
        //Wall Detection 
        Vector3 horizontalVelocity = new Vector3(physicsManager.Velocity.x, 0, physicsManager.Velocity.z);
        Vector3 direction = horizontalVelocity.normalized;

        if (horizontalVelocity.magnitude > 0.01f)
        {
            float radius = 0.5f;
            float castDistance = 0.3f;

            if (Physics.SphereCast(playerBody.position, radius, direction, out RaycastHit hitWall, castDistance, wall))
            {
                // Stop horizontal movement when colliding with wall
                Vector3 newVelocity = physicsManager.Velocity;
                newVelocity = Vector3.ProjectOnPlane(newVelocity, hitWall.normal); // slide along the wall
                physicsManager.Velocity = new Vector3(newVelocity.x, physicsManager.Velocity.y, newVelocity.z);
            }
        }
        Vector3 rayDirectionGround = new Vector3(0, -1, 0);
        if (Physics.Raycast(playerBody.transform.position, rayDirectionGround, out RaycastHit hitinfoGround, .5f, ground))
        {
            Debug.Log("hit");
            physicsManager.ApplyNormal(1);
        }
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
            playerCamera.transform.localPosition = new Vector3(0,
                                              cameraHeight,
                                              0);
        }
        if (!grounded)
        {
            physicsManager.Gravity = gravity;
        }
        else
        {
            physicsManager.Gravity = 0;
        }
        if(physicsManager.Velocity.y < 0)
        {
            goingUp = false; 
        }
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
            moving = false;
        }
            regMoveVector = context.ReadValue<Vector2>();
        
    }
    public void Move(Vector2 direction2D)
    {
        Vector3 direction3D = playerBody.transform.forward * direction2D.y + playerBody.transform.right * direction2D.x;

    
            if (!grounded)
            {
                physicsManager.ApplyForce(direction3D * moveSpeed * .01f, 1f);

            }
            else
            {
                physicsManager.ApplyForce(direction3D * moveSpeed, 1f);
            }
        if ((float)Math.Sqrt(physicsManager.Velocity.sqrMagnitude) > maxSpeed / 30)
        {
            if (!grounded)
            {
                physicsManager.ApplyForce(-direction3D * moveSpeed * .01f, 1f);

            }
            else
            {
                physicsManager.ApplyForce(-direction3D * moveSpeed, 1f);
            }
        }
    }

    public void Jump()
    {
        //Debug.Log("hit");
        goingUp = true;
        if (physicsManager.Gravity == 0)
        {
            grounded = false;
            Vector3 jumpForce = new Vector3(0, jumpStrength, 0);
            physicsManager.ApplyForce(jumpForce, 1f);
        }

    }

    public void OnSlide(InputAction.CallbackContext context)
    {

        
        if (context.action.inProgress)
        {
            crouched = true;
            if (grounded)
            {
                if (moving)
                {
                    Vector3 direction3D = playerBody.transform.forward * regMoveVector.y + playerBody.transform.right * regMoveVector.x;
                    physicsManager.ApplyForce(direction3D * slideSpeed, 1f);

                }
            }

        }
        else
        {
            crouched = false;
        }

    }


    
    public void OnSlash()
    {

        Vector3 slashForce;
        slashForce = playerBody.transform.forward + playerBody.transform.right + playerBody.transform.up;
        Debug.Log(slashForce);
        physicsManager.ApplyForce(slashForce*slashStrength,1);
    }
   

    public void Look(InputAction.CallbackContext context)
    {
        Vector2 cameraVector = context.ReadValue<Vector2>();
        lookPitch -= cameraVector.y * lookSensitivity;
        lookPitch = Mathf.Clamp(lookPitch, -90f, 90f);

        lookYaw = cameraVector.x * lookSensitivity;

        playerCamera.transform.localRotation = Quaternion.Euler(lookPitch, 0f, 0f);
        playerBody.transform.rotation *= Quaternion.Euler(0f, lookYaw, 0f);
    }

  //void OnCollisionEnter(Collision collision)
  //{
  //  //if (collision.gameObject.CompareTag("Wall"))
  //  //{
  //  //      physicsManager.ApplyForce(-physicsManager.Velocity*mass, 1f);
  //  //}
  //  if (collision.gameObject.CompareTag("Ground"))
  //  {
  //      physicsManager.Gravity = 0;
  //      grounded = true;
  //      physicsManager.ZeroYVelocity();
  //  }
  //}

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            //Debug.Log("hit");
            physicsManager.ApplyForce(-physicsManager.Velocity * mass, 1f);
        }
    }
  
    public void WallJump(Vector3 wallNormal)
      {
          wallNormal = Vector3.ProjectOnPlane(wallNormal, playerBody.transform.up).normalized;
    
      }
    //
    //
    //  public void Slam()
    //  {
    //      StartCoroutine(SlamMove());
    //  }
    //
    //  public void SlideCancelled()
    //  {
    //      inputManager.SetSlidingOff();
    //  }
    //
    //  public void GrappleCancel()
    //  {
    //      inputManager.SetGrapplingOff();
    //  }
    //
    //  public void Grapple()
    //  {
    //      StartCoroutine(GrappleMove());
    //  }
    //
    //  public void Swing()
    //  {
    //      ;
    //  }
    //

    //
    //  public void Slash()
    //  {
    //      StartCoroutine(SlashMove());
    //  }
    //
    //  public void QuickMine()
    //  {
    //      ;
    //  }
    //
    //  public void ToggleAltMode()
    //  {
    //      ;
    //  }
    //
    //  public void Kamikaze()
    //  {
    //      ;
    //  }
    //
    //  public IEnumerator SlashMove()
    //  {
    //      inputManager.SetSlammingOff();
    //      inputManager.SetSlidingOff();
    //      inputManager.SetSlashingOn();
    //
    //      bool forcedNonzero = false;
    //
    //      Vector3 initDirection;
    //
    //      if (velocityVector == Vector3.zero)
    //      {
    //          forcedNonzero = true;
    //          initDirection = playerBody.transform.forward;
    //          velocityVector = playerBody.transform.forward;
    //      }
    //      else
    //      {
    //          initDirection = Quaternion.FromToRotation(Vector3.forward, playerBody.transform.forward) * velocityVector.normalized;
    //      }
    //
    //      float elapsedTime = 0;
    //      speedMultiplyer = slashMultiplyer;
    //      delayFrames = 0;
    //
    //      while (elapsedTime < slashDuration && inputManager.IsSlashing() && !inputManager.IsSliding())
    //      {
    //          SetVelocity((-2f * Mathf.Lerp(0, 0.5f, elapsedTime / slashDuration) + 2) * slashSpeed * initDirection);
    //
    //          elapsedTime += Time.fixedDeltaTime;
    //
    //          yield return new WaitForFixedUpdate();
    //      }
    //
    //      if (forcedNonzero)
    //      {
    //          velocityVector = Vector3.zero;
    //      }
    //
    //      inputManager.SetSlashingOff();
    //  }
    //
    //  public IEnumerator SlideMove()
    //  {
    //      Vector3 slideDirection = playerBody.transform.forward;
    //      Vector3 groundNormal;
    //      Vector3 initCameraPos = playerCamera.transform.localPosition;
    //      RaycastHit hit;
    //      if(speedMultiplyer < slideMultiplyer)
    //          speedMultiplyer = slideMultiplyer;
    //          
    //      int frameCount = 0;
    //
    //      float elapsedTime = 0;
    //      inputManager.SetSlidingOn();
    //
    //      while (inputManager.IsSliding())
    //      {
    //          LowerCamera(initCameraPos, elapsedTime);
    //
    //          if (!Physics.Raycast(playerCamera.transform.position, -playerBody.transform.up, out hit, 3.5f, LayerMask.GetMask("Ground")))
    //          {
    //              break;
    //          }
    //
    //          groundNormal = hit.normal;
    //
    //          Debug.Log(frameCount);
    //          if (frameCount >= frictionFrameDelay)
    //          {
    //              speedMultiplyer = Mathf.Clamp(speedMultiplyer - slideFriction * Time.fixedDeltaTime, 0, int.MaxValue);
    //              if (speedMultiplyer <= 0)
    //                  break;
    //          }
    //
    //          SetVelocity(Vector3.ProjectOnPlane(slideDirection * slideSpeed * speedMultiplyer, groundNormal));
    //
    //          elapsedTime += Time.fixedDeltaTime;
    //          frameCount += 1;
    //          yield return new WaitForFixedUpdate();
    //      }
    //
    //      inputManager.SetSlidingOff();
    //
    //      elapsedTime = 0;
    //      initCameraPos = playerCamera.transform.localPosition;
    //
    //      while (elapsedTime < slideCameraTime)
    //      {
    //          RaiseCamera(initCameraPos, elapsedTime);
    //
    //          elapsedTime += Time.fixedDeltaTime;
    //          yield return new WaitForFixedUpdate();
    //      }
    //
    //      playerCamera.transform.localPosition = cameraHeight;
    //  }
    //
    //  public IEnumerator SlamMove()
    //  {
    //      inputManager.SetSlammingOn();
    //
    //      speedMultiplyer = slamMultiplyer;
    //      delayFrames = 0;
    //
    //      while (inputManager.IsSlamming())
    //      {
    //          SetVelocity(-playerBody.transform.up * slamSpeed);
    //          yield return new WaitForFixedUpdate();
    //      }
    //  }
    //
    //  public IEnumerator GrappleMove()
    //  {
    //      float currentLength = 0;
    //      Vector3 initPosition;
    //
    //      while (currentLength < grappleRange && inputManager.IsGrappling())
    //      {
    //          yield return new WaitForFixedUpdate();
    //      }
    //  }
    //
    //
    //  void OnCollisionStay(Collision collision)
    //  {
    //      if (collision.gameObject.CompareTag("Wall"))
    //      {
    //          //SetVelocity(GetVelocity() + collision.contacts[0].normal * Mathf.Clamp(Vector3.Dot(GetVelocity(), -collision.contacts[0].normal), 0f, 100f));
    //          if (Vector3.Dot(-collision.contacts[0].normal, worldToLocal * velocityVector) >= 0)
    //              speedMultiplyer = 0.1f;
    //          else
    //              speedMultiplyer = 1.5f;
    //
    //          delayFrames = 0;
    //      }
    //      if (collision.gameObject.CompareTag("Ground"))
    //      {
    //          inputManager.SetOnGround();
    //          playerCollider.material = grounded;
    //      }
    //  }
    //
    //  void OnCollisionExit(Collision collision)
    //  {
    //      if (collision.gameObject.CompareTag("Wall"))
    //      {
    //          inputManager.SetOffWall();
    //          Debug.LogError("Off Wall");
    //      }
    //      else if (collision.gameObject.CompareTag("Ground"))
    //      {
    //          Debug.LogWarning("Off Ground");
    //          inputManager.SetOffGround();
    //      }
    //  }
    //
    //  void SetVelocity(Vector3 newVelocity)
    //  {
    //      playerBody.velocity = newVelocity;
    //  }
    //
    //  Vector3 GetVelocity()
    //  {
    //      return playerBody.velocity;
    //  }
    //
    //  float GetSpeed()
    //  {
    //      return playerBody.velocity.magnitude;
    //  }
    //
    //  void LowerCamera(Vector3 initPosition, float elapsedTime)
    //  {
    //      float clampedT = Mathf.Clamp01(elapsedTime / slideCameraTime);
    //
    //      float t = Mathf.Lerp(0, 1, clampedT);
    //
    //      playerCamera.transform.localPosition = Vector3.Lerp(initPosition, cameraHeight + Vector3.down * slideCameraHeight, t);
    //  }
    //
    //  void RaiseCamera(Vector3 initPosition, float elapsedTime)
    //  {
    //      float clampedT = Mathf.Clamp01(elapsedTime / slideCameraTime);
    //
    //      float t = Mathf.Lerp(0, 1, clampedT);
    //
    //      playerCamera.transform.localPosition = Vector3.Lerp(initPosition, cameraHeight, t);
    //  }
    //
    //  public void SetAirResistance(float newResistance)
    //  {
    //      airResistance = newResistance;
    //  }
} */
