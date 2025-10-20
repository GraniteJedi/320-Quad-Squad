using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Compass : MonoBehaviour
{
    [Header("Transforms/References")]
    [SerializeField] RectTransform infoContainerTransform;
    [SerializeField] RectTransform targetTransform;
    [SerializeField] RectTransform compassContainer;
    [SerializeField] Transform targetPos;
    [SerializeField] Rigidbody playerBody;
    [SerializeField] Image targetIcon;

    [Header("Compass Customization")]
    // This is the number where while playing in Full HD view, the compass loops seamlessly
    [SerializeField] float initAngleRatio;
    // This is the width of the compass info container when in Full HD view
    [SerializeField] float initContainerWidth = 720.8f;
    [SerializeField] float compassSensitivity;

    // Class variables to avoid repetitive initializtion
    Vector3 toTarget;
    float prevCompassAngle;
    float compassAngle;
    float playerAngle;
    float angleRatio;
    float compassWidth;
    float initCompassSensitivity;

    // The color of the target icon used to shift it's opacity for a pulsing effect
    Color targetColor;

    // Start is called before the first frame update
    void Start()
    {
        targetColor = targetIcon.color;

        if (targetPos == null)
        {
            targetTransform.gameObject.SetActive(false);
        }

        initCompassSensitivity = compassSensitivity;

        prevCompassAngle = compassAngle = GetCompassAngle();
    }

    // Update is called once per frame
    void Update()
    {
        compassAngle = GetCompassAngle();

        if (Mathf.Abs(prevCompassAngle - compassAngle) > 300)
        {
            compassSensitivity = 1;
        }
        else
        {
            compassSensitivity = initCompassSensitivity;
        }

        prevCompassAngle = compassAngle;
        
        compassWidth = infoContainerTransform.rect.width;
        angleRatio = initAngleRatio / (compassWidth / initContainerWidth);
        infoContainerTransform.localPosition = Vector2.Lerp(infoContainerTransform.localPosition, Vector2.left * compassAngle * (angleRatio * compassWidth), compassSensitivity);

        if (targetPos == null)
        {
            return;
        }

        if (playerBody == null)
        {
            toTarget = Vector3.zero;
        }
        else
        {
            toTarget = targetPos.position - playerBody.position;
            toTarget -= Vector3.up * toTarget.y;
        }

        playerAngle = GetPlayerAngle();
        if (playerAngle > 180)
        {
            playerAngle -= 360;
        }

        targetTransform.localPosition = Vector3.Lerp(targetTransform.localPosition, Vector3.ClampMagnitude(Vector2.right * playerAngle * (angleRatio * compassWidth), compassContainer.rect.width / 2), compassSensitivity);

        targetColor.a = Mathf.Lerp(1, 0, Mathf.Pow(0.5f * Mathf.Sin(7 * Time.time) + 0.5f, 2));

        targetIcon.color = targetColor;
    }

    /// <summary>
    /// Finds the horizontal angle between the player's forward vector and "North"
    /// </summary>
    /// 
    /// <returns>
    /// The player's y euler angle representative of their horizontal angle
    /// </returns>
    float GetCompassAngle()
    {
        if (playerBody == null)
        {
            return 0;
        }
        else
        {
            return playerBody.transform.eulerAngles.y % 360;
        }
    }

    /// <summary>
    /// Finds the horizontal angle between the player's forward vector and the line from the player to the target 
    /// </summary>
    /// 
    /// <returns>
    /// The horizontal angle change from the player's forward vector to the line from the player to their target
    /// </returns>
    float GetPlayerAngle()
    {
        if (playerBody == null)
        {
            return 0;
        }
        else
        {
            return Vector3.SignedAngle(playerBody.transform.forward, toTarget, Vector3.up) % 360;
        }
    }

    /// <summary>
    ///     Set the current position of the target using its transform component
    ///     </summary>
    /// 
    /// <param name="transform">
    ///     The transform component of the object being targetted by the compass
    ///     </param>
    public void SetTargetPos(Transform transform)
    {
        targetPos = transform;
    }
}
