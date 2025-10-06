using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Compass : MonoBehaviour
{
    private float compassWidth;
    [Header("Transforms/References")]
    [SerializeField] RectTransform infoContainerTransform;
    [SerializeField] RectTransform targetTransform;
    [SerializeField] RectTransform compassContainer;
    [SerializeField] Transform targetPos;
    [SerializeField] Rigidbody playerBody;

    [Header("Compass Customization")]
    // This is the number where while playing in Full HD view, the compass loops seamlessly
    [SerializeField] float initAngleRatio;
    // This is the width of the compass info container when in Full HD view
    [SerializeField] float initContainerWidth = 720.8f;

    // Class variables to avoid repetitive initializtion
    Vector3 toTarget;
    float compassAngle;
    float playerAngle;
    float angleRatio;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        compassAngle = GetCompassAngle();
        compassWidth = infoContainerTransform.rect.width;
        Debug.LogWarning(compassWidth);
        angleRatio = initAngleRatio / (compassWidth / initContainerWidth);
        infoContainerTransform.localPosition = Vector2.left * compassAngle * (angleRatio * compassWidth);

        toTarget = targetPos.position - playerBody.position;
        toTarget -= Vector3.up * toTarget.y;

        playerAngle = GetPlayerAngle();
        if (playerAngle > 180)
        {
            playerAngle -= 360;
        }

        targetTransform.localPosition = Vector3.ClampMagnitude(Vector2.right * playerAngle * (angleRatio * compassWidth), compassContainer.rect.width / 2);
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
        return playerBody.transform.eulerAngles.y % 360;
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
        return Vector3.SignedAngle(playerBody.transform.forward, toTarget, Vector3.up) % 360;
    }
}
