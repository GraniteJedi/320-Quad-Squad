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
    [Header("Major References")]
    [SerializeField] Rigidbody playerbody;
    PhysicsUnity physicsManager = null;

    [Header("Countdown Customization")]
    [SerializeField] float time = 10f;
    [SerializeField] float speedThreshold = 5f;
    [SerializeField] float initLenience;
    float lenience;
    [SerializeField] bool isActive = true;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (IsStopped() && isActive)
        {
            lenience -= Time.deltaTime;

            if (lenience <= 0)
            {
                if (time - Time.deltaTime > 0)
                {
                    time -= Time.deltaTime;
                }
                else
                {
                    time = 0;
                    OutOfTime();
                }
            }
        }
        else
        {
            lenience = initLenience;
        }
    }

    /// <summary>
    /// Manually set the remaining time of the countdown
    /// </summary>
    /// 
    /// <param name="time">
    /// The time the countdown is being set to
    /// </param>
    void SetTime(float time)
    {
        this.time = time;
    }

    /// <summary>
    /// Add a specific amount of time to the countdown
    /// </summary>
    /// 
    /// <param name="time">
    /// The time being added to the countdown
    /// </param>
    void AddTime(float time)
    {
        this.time += time;
    }

    /// <summary>
    /// Subtract a specific amount of time from the countdown
    /// </summary>
    /// 
    /// <param name="time">
    /// The time being removed from the countdown
    /// </param>
    void RemoveTime(float time)
    {
        this.time -= time;
    }

    /// <summary>
    /// Fetches the current speed of the player (REPLACE WITH FUNCTION FROM CUSTOM PHYSICS CLASS IF NECESSARY)
    /// </summary>
    /// 
    /// <returns>
    /// The current speed of the player, or a zero vector if N/A
    /// </returns>
    public float GetSpeed()
    {
        if (physicsManager == null)
        {
            return playerbody.velocity.magnitude;
        }
        else
        {
            return (float)Mathf.Sqrt(physicsManager.Velocity.sqrMagnitude);
        }
    }

    /// <summary>
    /// Fetches the current time remaining on the countdown
    /// </summary>
    /// 
    /// <returns>
    /// The current time left on the countdown
    /// </returns>
    public float GetTime()
    {
        return time;
    }

    /// <summary>
    /// Fetches the current remaining lenience time (while lenience <= 0, the timer ticks down)
    /// </summary>
    /// 
    /// <returns>
    /// The lenience represented as time
    /// </returns>
    public float GetLenience()
    {
        return lenience;
    }

    /// <summary>
    /// Fetches the current threshold of speed the player must surpass to keep the countdown inactive
    /// </summary>
    /// 
    /// <returns>
    /// The countdown's speed threshold
    /// </returns>
    public float GetThreshold()
    {
        return speedThreshold;
    }

    /// <summary>
    /// Determines if the player is or is nearly stopped based on if their speed is below a threshold
    /// </summary>
    /// 
    /// <returns>
    /// Returns TRUE when the player's speed is too low and FALSE when the player's speed is past the threshold
    /// </returns>
    public bool IsStopped()
    {
        return GetSpeed() < speedThreshold;
    }

    /// <summary>
    /// Sets this countdown to active if true, and inactive if false, changing whether or not it will count down
    /// </summary>
    /// 
    /// <param name="isActive">
    /// The new active state for the countdown
    /// </param>
    public void SetActive(bool isActive)
    {
        this.isActive = isActive;
    }

    /// <summary>
    /// Checks whether or not the timer on this countdown is active or not
    /// </summary>
    /// 
    /// <returns>
    /// True if the countdown is active and false otherwise
    /// </returns>
    public bool IsActive()
    {
        return isActive;
    }

    /// <summary>
    /// Triggers a manager for the player when their countdown hits 0 to kill/respawn them
    /// </summary>
    void OutOfTime()
    {
        Debug.LogError("Countdown.OutOfTime() not implemented yet, ending Play Mode.");
        #if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        #endif
    }

    public void SetPhysicsManager(PhysicsUnity physicsManager)
    {
        this.physicsManager = physicsManager;
    }
}