using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class InputManager : MonoBehaviour
{
    private InputAsset moveControls;
    [SerializeField] PlayerManager playerManager;
    [SerializeField] UIManager uIManager;
    [SerializeField] Rigidbody playerbody;
    [SerializeField] float groundCheckDistance;

    private bool altModeOn = false;
    private bool isOnWall = false;
    private Vector3 wallNormal;
    [SerializeField] int initWallJumps = 3;
    int wallJumps = 0;

    private bool isGrounded = false;
    private bool isSlashing = false;
    private bool isSliding = false;
    private bool isSlamming = false;
    private bool isGrappling = false;

    private RaycastHit hit;

    // Start is called before the first frame update
    void Start()
    {
        moveControls = new InputAsset();

        moveControls.Player.Move.performed += OnMove;
        moveControls.Player.Move.canceled += OnMove;

        moveControls.Player.Jump.performed += OnJump;

        moveControls.Player.Slide.performed += OnSlide;
        moveControls.Player.Slide.canceled += OnSlideCancelled;

        moveControls.Player.Grapple.performed += OnGrapple;

        moveControls.Player.Swing.performed += OnSwing;

        moveControls.Player.Look.performed += OnLook;

        moveControls.Player.Slash.performed += OnSlash;

        moveControls.Player.QuickMine.performed += OnQuickMine;

        moveControls.Player.ALTMODE.performed += OnAltMode;
        //moveControls.Player.ALTMODE.canceled += OnAltMode;    UNCOMMENT THIS TO MAKE ALT ACTIVE WHILE HOLDING RMB

        moveControls.Player.Kamikaze.performed += OnKamikaze;

        moveControls.Player.Pause.performed += OnPause;

        moveControls.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        if (Physics.Raycast(playerbody.transform.position, Vector3.down, out hit, groundCheckDistance, LayerMask.GetMask("Ground")))
        {
            SetOnGround();
            //playerManager.SetOnGround();
        }
        else
        {
            SetOffGround();
            //playerManager.SetOffGround();
        }
    }

    /// =====================================================================
    /// ========================== INPUT FUNCTIONS ==========================
    /// =====================================================================
    /// 
    /// All INPUT FUNCTIONS check if an input is valid then pass the appropriate calls to
    ///     the necessary classes

    void OnMove(InputAction.CallbackContext context)
    {
        if (IsMoveValid())
        {
            //playerManager.Move(context.ReadValue<Vector2>());
        }
    }

    void OnJump(InputAction.CallbackContext context)
    {
        if (IsJumpValid())
        {
            Debug.Log($"OnJump | {context.action.name}");
            playerManager.Jump();
        }
        else if (IsWallJumpValid())
        {
            Debug.Log($"OnWallJump | {context.action.name}");
            //playerManager.WallJump(wallNormal);
        }
    }

    void OnSlide(InputAction.CallbackContext context)
    {
        if (IsSlideValid())
        {
            Debug.Log($"OnSlide | {context.action.name}");
            //playerManager.Slide();
        }
        else if (IsSlamValid())
        {
            Debug.Log($"OnSlam | {context.action.name}");
            playerManager.Slam();
        }
    }

    void OnSlideCancelled(InputAction.CallbackContext context)
    {
        Debug.Log($"OnSlideCancelled | {context.action.name}");
        if (IsSliding())
        {
            //playerManager.SlideCancelled();
        }
    }

    void OnGrapple(InputAction.CallbackContext context)
    {
        Debug.Log($"OnMove | {context.action.name}");
        if (IsGrappling())
        {
            playerManager.GrappleCancel();
        }
        else if (IsGrappleValid())
        {
            playerManager.Grapple();
        }
    }

    void OnSwing(InputAction.CallbackContext context)
    {
        Debug.Log($"OnSwing | {context.action.name}");
        if (IsSwingValid())
        {
            //playerManager.Swing();
        }
    }

    void OnLook(InputAction.CallbackContext context)
    {
        //Debug.Log($"OnLook | { context.action.name }");
        if (IsLookValid())
        {
            //playerManager.Look(context.ReadValue<Vector2>());
        }
    }

    void OnSlash(InputAction.CallbackContext context)
    {
        Debug.Log($"OnSlash {context.action.name}");
        if (IsSlashValid())
        {
            playerManager.Slash(context);
        }
    }

    void OnQuickMine(InputAction.CallbackContext context)
    {
        Debug.Log($"OnQuickMine | {context.action.name}");
        if (IsQuickMineValid())
        {
            playerManager.QuickMine();
        }
    }

    void OnAltMode(InputAction.CallbackContext context)
    {
        Debug.Log($"OnAltMode | {context.action.name}");
        if (IsAltModeValid())
        {
            playerManager.ToggleAltMode();
        }
    }

    void OnKamikaze(InputAction.CallbackContext context)
    {
        Debug.Log($"OnKamikaze | {context.action.name}");
        if (IsKamikazeValid())
        {
            playerManager.Kamikaze();
        }
    }

    void OnPause(InputAction.CallbackContext context)
    {
        //uIManager.Pause();
        moveControls.Disable();
    }

    public void Resume()
    {
        moveControls.Enable();
    }

    /// =====================================================================
    /// ========================== VALIDITY CHECKS ==========================
    /// =====================================================================
    /// 
    /// All VALIDITY CHECK functions check if a certain input action is valid in
    ///     the current context of the game

    /// <summary>
    /// Checks if the player is currently in a state to move
    /// </summary>
    /// 
    /// <returns>
    /// TRUE if the player is allowed to move and FALSE otherwise
    /// </returns>
    private bool IsMoveValid()
    {
        return true;
    }


    /// <summary>
    /// Checks if the player is currently in a state to jump based on if they're on the ground
    /// </summary>
    /// 
    /// <returns>
    /// TRUE if the player is allowed to jump and FALSE otherwise
    /// </returns>
    private bool IsJumpValid()
    {
        Debug.Log("Jump is valid should be " + isGrounded);
        return isGrounded;
    }

    /// <summary>
    /// Checks if the player is currently in a state to wall jump based on if they're colliding with
    ///     a wall while off of the ground and they still have wall jumps remaining
    /// </summary>
    /// 
    /// <returns>
    /// TRUE if the player is allowed to wall jump and FALSE otherwise
    /// </returns>
    private bool IsWallJumpValid()
    {
        if (IsOnWall() && !IsSlashing() && !IsGrounded() && wallJumps > 0)
        {
            Debug.Log("IsOnWall apparently");
            wallJumps--;
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if the player is currently in a state to slide based on if they're on the ground
    /// </summary>
    /// 
    /// <returns>
    /// TRUE if the player is allowed to slide and FALSE otherwise
    /// </returns>
    private bool IsSlideValid()
    {
        return isGrounded;
    }

    /// <summary>
    /// Checks if the player is currently in a state to slam based on whether they're airborne
    /// </summary>
    /// 
    /// <returns>
    /// TRUE if the player is allowed to slam and FALSE otherwise
    /// </returns>
    private bool IsSlamValid()
    {
        return !isGrounded;
    }

    /// <summary>
    /// Checks if the player is currently in a state to grapple (mostly based on cooldown period)
    /// </summary>
    /// 
    /// <returns>
    /// TRUE if the player is allowed to grapple and FALSE otherwise
    /// </returns>
    private bool IsGrappleValid()
    {
        return true;
    }

    /// <summary>
    /// Checks if the player is currently in a state to swing their weapon (mostly based on cooldown period)
    /// </summary>
    /// 
    /// <returns>
    /// TRUE if the player is allowed to swing and FALSE otherwise
    /// </returns>
    private bool IsSwingValid()
    {
        return true;
    }

    /// <summary>
    /// Checks if the player is currently in a state to look around 
    /// </summary>
    /// 
    /// <returns>
    /// TRUE if the player is allowed to look around and FALSE otherwise
    /// </returns>
    private bool IsLookValid()
    {
        return true;
    }

    /// <summary>
    /// Checks if the player is currently in a state to slash (mostly based on cooldown period)
    /// </summary>
    /// 
    /// <returns>
    /// TRUE if the player is allowed to slash and FALSE otherwise
    /// </returns>
    private bool IsSlashValid()
    {
        return true;
    }

    /// <summary>
    /// Checks if the player is currently in a state to throw a quick mine based on cooldowns
    ///     and if there are any more mines remaining
    /// </summary>
    /// 
    /// <returns>
    /// TRUE if the player is allowed to toss a quick mine and FALSE otherwise
    /// </returns>
    private bool IsQuickMineValid()
    {
        return true;
    }

    /// <summary>
    /// Checks if the player is currently in a state to toggle ALT MODE
    /// </summary>
    /// 
    /// <returns>
    /// TRUE if the player is allowed to toggle ALT MODE and FALSE otherwise
    /// </returns>
    private bool IsAltModeValid()
    {
        return true;
    }

    /// <summary>
    /// Checks if the player is currently in a state to sacrifice countdown time for a
    ///     boost in velocity based on remaining time and cooldown
    /// </summary>
    /// 
    /// <returns>
    /// TRUE if the player is allowed to kamikaze and FALSE otherwise
    /// </returns>
    private bool IsKamikazeValid()
    {
        return true;
    }

    /// ==================================================================
    /// ========================== STATE CHECKS ==========================
    /// ==================================================================
    /// 
    /// All STATE CHECK functions verify whether or not the player is currently in
    ///     the middle of a certain input action

    /// <summary>
    /// Checks if the player is currently sliding or not
    /// </summary>
    /// 
    /// <returns>
    /// TRUE if the player is sliding and FALSE otherwise
    /// </returns>
    public bool IsSliding()
    {
        return isSliding;
    }

    public void SetSlidingOn()
    {
        isSliding = true;
    }

    public void SetSlidingOff()
    {
        isSliding = false;
    }

    public bool IsOnWall()
    {
        return isOnWall;
    }

    public void SetOnWall(Vector3 wallNormal)
    {
        isOnWall = true;
        this.wallNormal = wallNormal;
    }

    public void SetOffWall()
    {
        isOnWall = false;
    }

    public Vector3 GetWallNormal()
    {
        return wallNormal;
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    public void SetOnGround()
    {
        wallJumps = initWallJumps;
        isGrounded = true;
        //playerManager.SetAirResistance(8);
        if (IsSlamming())
        {
            isSlamming = false;
        }
    }

    public void SetOffGround()
    {
        isGrounded = false;
        //playerManager.SetAirResistance(7);
        if (IsSliding())
        {
            isSliding = false;
        }
    }

    public bool IsSlashing()
    {
        return isSlashing;
    }

    public void SetSlashingOn()
    {
        isSlashing = true;
    }

    public void SetSlashingOff()
    {
        isSlashing = false;
    }

    public bool IsSlamming()
    {
        return isSlamming;
    }

    public void SetSlammingOn()
    {
        isSlamming = true;
    }

    public void SetSlammingOff()
    {
        isSlamming = false;
    }

    /// <summary>
    /// Checks if the player is currently grappling or not
    /// </summary>
    /// 
    /// <returns>
    /// TRUE if the player is grappling and FALSE otherwise
    /// </returns>
    public bool IsGrappling()
    {
        return isGrappling;
    }

    public void SetGrapplingOn()
    {
        isGrappling = true;
    }

    public void SetGrapplingOff()
    {
        isGrappling = false;
    }

    public Vector3 GetGroundNormal()
    {
        if (isGrounded)
            return hit.normal.normalized;
        else
            return Vector3.up;
    }
}
