using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DashCooldown : MonoBehaviour
{
    [SerializeField] Image outlineFill;
    [SerializeField] Image dashHeadFill1;
    [SerializeField] Image dashHeadFill2;
    [SerializeField] Image dashHeadFill3;
    [SerializeField] Image outline;
    [SerializeField] Image dashHead1;
    [SerializeField] Image dashHead2;
    [SerializeField] Image dashHead3;
    [SerializeField] float cooldown;
    [SerializeField] float depleteFillStrength;
    private PlayerManager playerManager;
    private float elapsedTime = 0;
    private bool coolingDown = false;

    // Start is called before the first frame update
    void Start()
    {
        outlineFill.fillMethod = dashHeadFill1.fillMethod = dashHeadFill2.fillMethod = dashHeadFill3.fillMethod = Image.FillMethod.Horizontal;
        outlineFill.fillOrigin = dashHeadFill1.fillOrigin = dashHeadFill2.fillOrigin = dashHeadFill3.fillOrigin = (int)Image.OriginHorizontal.Left;
        outlineFill.fillAmount = dashHeadFill1.fillAmount = dashHeadFill2.fillAmount = dashHeadFill3.fillAmount = 0;

        outlineFill.enabled = dashHeadFill1.enabled = dashHeadFill2.enabled = dashHeadFill3.enabled = false;

        playerManager = GameObject.FindAnyObjectByType<PlayerManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (dashHeadFill2.enabled)
        {
            dashHeadFill1.fillAmount = Mathf.Lerp(dashHeadFill1.fillAmount, 0, Time.deltaTime * depleteFillStrength);
        }
        
        if (dashHeadFill3.enabled)
        {
            dashHeadFill2.fillAmount = Mathf.Lerp(dashHeadFill2.fillAmount, 0, Time.deltaTime * depleteFillStrength);
        }
    }

    public void ManualUpdate()
    {
        if (!dashHead3.enabled)
        {
            SetFill(dashHeadFill3);
            //SetFill(outlineFill);
        }
        else if (!dashHead2.enabled)
        {
            SetFill(dashHeadFill2);
        }
        else if (!dashHead1.enabled)
        {
            SetFill(dashHeadFill1);
        }
    }
    
    public bool Refill()
    {
        return RefillDash();
    }

    public void Dash()
    {
        if (dashHead1.enabled)
        {
            coolingDown = true;
            dashHead1.enabled = false;
            dashHeadFill1.enabled = true;
        }
        else if (dashHead2.enabled)
        {
            //dashHeadFill1.fillAmount = 0;

            coolingDown = true;
            dashHead2.enabled = false;
            dashHeadFill2.enabled = true;
        }
        else if (dashHead3.enabled)
        {
            //dashHeadFill2.fillAmount = 0;

            coolingDown = true;
            dashHead3.enabled = false;
            dashHeadFill3.enabled = true;
            //outlineFill.enabled = true;
            //outline.enabled = false;
        }
    }

    private void SetFill(Image imageFill)
    {
        imageFill.fillAmount = Mathf.Lerp(imageFill.fillAmount, Mathf.Lerp(0, 1, (playerManager.GetSlashCooldown() - playerManager.GetElapsedSlashCooldown()) / playerManager.GetSlashCooldown()), Time.deltaTime * depleteFillStrength);
    }
    
    private bool RefillDash()
    {
        if (!dashHead3.enabled)
        {
            dashHeadFill3.enabled = false;
            //outlineFill.enabled = false;

            dashHead3.enabled = true;
            //outline.enabled = true;

            return true;
        }
        else if (!dashHead2.enabled)
        {
            dashHeadFill2.enabled = false;

            dashHead2.enabled = true;

            return true;
        }
        else if (!dashHead1.enabled)
        {
            dashHeadFill1.enabled = false;

            dashHead1.enabled = true;
            coolingDown = false;

            return true;
        }

        return false;
    }
}
