using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Countdown : MonoBehaviour
{
    /// <summary>
    ///     References to major components/objects that this script relies on
    ///     </summary>
    [Header("Major References")]
    [SerializeField] Rigidbody playerbody;
    [SerializeField] PlayerManager playerManager;
    PhysicsUnity physicsManager = null;

    /// <summary>
    ///     Variables to help customize how the countdown functions
    ///     </summary>
    [Header("Countdown Customization")]
    [SerializeField] float time = 10f;
    [SerializeField] float speedThreshold = 5f;
    [SerializeField] float initLenience;
    float lenience;
    [SerializeField] bool isActive = true;

    // Start is called before the first frame update
    void Start()
    {
        if (playerbody == null)
        {
            Debug.LogError("PLAYERBODY hasn't been set on the COUNTDOWN script");
        }
        if(playerManager == null)
        {
            Debug.LogError("PLAYER MANAGER hasn't been set on the COUNTDOWN script");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // If the player is 'stopped' and the timer is active, lower the lenience before the countdown starts
        if (IsStopped() && isActive)
        {
            lenience -= Time.deltaTime;

            // If the lenience has reached 0...
            if (lenience <= 0)
            {
                // Lower the countdown time if the timer is above 0
                if (time - Time.deltaTime > 0)
                {
                    time -= Time.deltaTime;
                }
                // Otherwise, carry out a function once the player's timer is at 0
                else
                {
                    time = 0;
                    OutOfTime();
                }
            }
        }
        // Otherwise, the player isn't 'stopped' and their lenience is reset
        else
        {
            lenience = initLenience;
        }
    }

    /// <summary>
    ///     Manually set the remaining time of the countdown
    ///     </summary>
    /// 
    /// <param name="time">
    ///     The time the countdown is being set to
    ///     </param>
    void SetTime(float time)
    {
        this.time = time;
    }

    /// <summary>
    ///     Add a specific amount of time to the countdown
    ///     </summary>
    /// 
    /// <param name="time">
    ///     The time being added to the countdown
    ///     </param>
    void AddTime(float time)
    {
        this.time += time;
    }

    /// <summary>
    ///     Subtract a specific amount of time from the countdown
    ///     </summary>
    /// 
    /// <param name="time">
    ///     The time being removed from the countdown
    ///     </param>
    void RemoveTime(float time)
    {
        this.time -= time;
    }

    /// <summary>
    ///     Fetches the current speed of the player (REPLACE WITH FUNCTION FROM CUSTOM PHYSICS CLASS IF NECESSARY)
    ///     </summary>
    /// 
    /// <returns>
    ///     The current speed of the player, or a zero vector if N/A
    ///     </returns>
    public float GetSpeed()
    {
        if (physicsManager == null)
        {
            if (playerbody == null)
            {
                return 0;
            }
            else
            {
                return (float)Mathf.Sqrt(playerManager.TotalVelocity.sqrMagnitude) / 2;
            }
        }
        else
        {
            return (float)Mathf.Sqrt(playerManager.TotalVelocity.sqrMagnitude);
        }
    }

    /// <summary>
    ///     Fetches the current time remaining on the countdown
    ///     </summary>
    /// 
    /// <returns>
    ///     The current time left on the countdown
    ///     </returns>
    public float GetTime()
    {
        return time;
    }

    /// <summary>
    ///     Fetches the current remaining lenience time (while lenience <= 0, the timer ticks down)
    ///     </summary>
    /// 
    /// <returns>
    ///     The lenience represented as time
    ///     </returns>
    public float GetLenience()
    {
        return lenience;
    }

    /// <summary>
    ///     Fetches the current threshold of speed the player must surpass to keep the countdown inactive
    ///     </summary>
    /// 
    /// <returns>
    ///     The countdown's speed threshold
    ///     </returns>
    public float GetThreshold()
    {
        return speedThreshold;
    }

    /// <summary>
    ///     Determines if the player is or is nearly stopped based on if their speed is below a threshold
    ///     </summary>
    /// 
    /// <returns>
    ///     Returns TRUE when the player's speed is too low and FALSE when the player's speed is past the threshold
    ///     </returns>
    public bool IsStopped()
    {
        return GetSpeed() < speedThreshold;
    }

    /// <summary>
    ///     Sets this countdown to active if true, and inactive if false, changing whether or not it will count down
    ///     </summary>
    /// 
    /// <param name="isActive">
    ///     The new active state for the countdown
    ///     </param>
    public void SetActive(bool isActive)
    {
        this.isActive = isActive;
    }

    /// <summary>
    ///     Checks whether or not the timer on this countdown is active or not
    ///     </summary>
    /// 
    /// <returns>
    ///     True if the countdown is active and false otherwise
    ///     </returns>
    public bool IsActive()
    {
        return isActive;
    }

    /// <summary>
    ///     Triggers a manager for the player when their countdown hits 0 to kill/respawn them
    ///     </summary>
    void OutOfTime()
    {
        // Debug.LogError("Countdown.OutOfTime() not implemented yet, ending Play Mode.");
        // #if UNITY_EDITOR
        // EditorApplication.isPlaying = false;
        // #endif
        if(playerManager != null)
        {
            playerManager.ResetPlayer();
            time = 10;
        }
    }

    public void SetPhysicsManager(PhysicsUnity physicsManager)
    {
        this.physicsManager = physicsManager;
    }
}