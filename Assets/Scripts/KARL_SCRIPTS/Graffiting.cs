using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Graffiting : MonoBehaviour
{
    [SerializeField] private Renderer myModel;

    [Range(0,1)]
    [SerializeField] private float waitTime;
    //private Color color;

    public bool isCleaned;
    // Start is called before the first frame update
    void Start()
    {
        Color color = myModel.material.color;

        if (isCleaned)
            color.a = 0;
        else
            color.a = 1;

        myModel.material.color = color;
    }

    private void Update()
    {
        Color color = myModel.material.color;
        if (color.a < 1)
            isCleaned = true;
        else
            isCleaned = false;
    }

    public bool getIsCleaned { get { return isCleaned; } }

    public void startFadeIn(UnityEngine.InputSystem.InputValue c)
    {
        FadeInMaterial(waitTime, c);
    }
    private void FadeInMaterial(float waitTime, UnityEngine.InputSystem.InputValue c)
    {
        if (myModel.material.color.a <= 1 && c.isPressed)
        {
            Color color = myModel.material.color;
            float fadeAmount = color.a + waitTime;
            color = new Color(color.r, color.g, color.b, fadeAmount);
            myModel.material.color = color;
        }

        
    }
    
    public void startFadeOut(UnityEngine.InputSystem.InputValue c)
    {
        FadeOutMaterial(waitTime, c);
    }

    private void FadeOutMaterial(float waitTime, UnityEngine.InputSystem.InputValue c)
    {
        if (myModel.material.color.a >= 0 && c.isPressed)
        {
            Color color = myModel.material.color;
            float fadeAmount = color.a - waitTime;
            color = new Color(color.r, color.g, color.b, fadeAmount);
            myModel.material.color = color;
        }
    }
}
