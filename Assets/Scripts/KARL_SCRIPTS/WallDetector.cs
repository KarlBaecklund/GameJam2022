using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class WallDetector
{
    public void interactingWithWall(Transform tran, ref InputValue c, CharacterInformation charInfo, bool blockTag = false)
    {
        int contactDistance = 2;
        int contactDistanceDown = 3;
        int layerMask = 1 << 6;
        Ray ray = new Ray(tran.position, tran.forward);
        Ray rayDown = new Ray(tran.position, - tran.up);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, contactDistance, layerMask))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.red);
            if (charInfo._character == CharacterENUM.MORT)
            {
                if (blockTag)
                {
                    charInfo.GetComponent<MovementScript>().PowerUp2 = false;
                    UIManager.UI.SetPowerUpImage(UIManager.UI.mortImage2, false);
                    TagBlocker(hit, charInfo);
                }
                else
                {
                    CallCleaning(hit, c, charInfo);
                }
            }
            else
            {
                CallGrafitti(hit, c, charInfo);
            }
        }
        else if (Physics.Raycast(rayDown, out hit, contactDistanceDown, layerMask))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.red);
            if (charInfo._character == CharacterENUM.MORT)
            {
                CallCleaning(hit, c, charInfo);
            }
            else
            {
                CallGrafitti(hit, c, charInfo);
            }
        }
        else
        {
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * 100, Color.green);
        }
    }
    private void TagBlocker(RaycastHit hit, CharacterInformation charInfo)
    {
        Graffiting graffiting = hit.collider.gameObject.GetComponent(typeof(Graffiting)) as Graffiting;
        if (graffiting != null)
        {
            graffiting.ActivateNoTag(charInfo.GetComponent<MovementScript>().tagBlockDuration);
        }
    }
    private void CallGrafitti(RaycastHit hit, InputValue c, CharacterInformation charInfo)
    {
        Graffiting graffiting = hit.collider.gameObject.GetComponent(typeof(Graffiting)) as Graffiting;
        if (graffiting != null)
        {
            graffiting.startFadeIn(c, charInfo);
            //Debug.Log("TR?FF");
        }
        else
        {
            Debug.Log("Hittar inte graffiting");
        }
    }
    private void CallCleaning(RaycastHit hit, InputValue c, CharacterInformation charInfo)
    {
        Graffiting graffiting = hit.collider.gameObject.GetComponent(typeof(Graffiting)) as Graffiting;
        if (graffiting != null)
        {
            graffiting.startFadeOut(c, charInfo);
            //Debug.Log("TR?FF");
        }
        else
        {
            Debug.Log("Hittar inte cleaning");
        }
    }
}
