using System;
using System.Collections;
using TMPro;
using UnityEngine;

[Serializable]
public class Gear
{
    public string gearName;
    public float gearRatio;
}

public class EngineControl : MonoBehaviour
{ 
    float rpm = 0;
    float maxRpm = 6500; // max rpm of porsche 930
    public float throttle;

    string actualGear;
    int actualGearIndex;
    [SerializeField] Gear[] gearBox;

    [SerializeField] AnimationCurve torqueCurve;

    Rigidbody rb;
    public float wheelRotationRate;
    
    // text values
    [SerializeField] TextMeshProUGUI rpmText;
    [SerializeField] TextMeshProUGUI actualGearText;

    public float actualTorque = 0;

    void Start()
    {
        actualGearIndex = 1;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        gearShift();
        calculateRpm();
        //print(rpm);
    }

    private void gearShift()
    {
        if (Input.GetButtonDown("UpShift") && actualGearIndex < gearBox.Length - 1)
        {
            actualGearIndex += 1;
        }
        if (Input.GetButtonDown("DownShift") && actualGearIndex > 0)
        {
            actualGearIndex -= 1;
        }

        for (int i = 0; i <= gearBox.Length; i++)
        {
            if (i == actualGearIndex)
            {
                actualGear = gearBox[actualGearIndex].gearName.ToString();
            }
        }
        actualGearText.text = actualGear;
    }

    private void calculateRpm()
    {

        if (rb.velocity.magnitude <= 0.01f)
        {
            rpm = maxRpm * throttle;
        }
        else
        {
            rpm = (((wheelRotationRate * gearBox[actualGearIndex].gearRatio * (4.22f)) * 60) / 6.28318530718f);
        }

        if (rpm > maxRpm) 
        { 
            rpm = maxRpm;
            actualTorque = 0;
        } else
        {
            actualTorque = (float)(torqueCurve.Evaluate(rpm / 1000) * 100 * gearBox[actualGearIndex].gearRatio * 4.22f * 0.7f / 0.26);
        }

        if (actualGear != "N")
        {
            if (rpm < 1000) { rpm = 1000; }
        }
        else
        {
            rpm = maxRpm * throttle;
        }

        

        rpmText.text = rpm.ToString();
        if (throttle >= -0.1 && throttle <= 0.1)
        {
            throttle = 0;
            actualTorque = 0;
        }

        if (actualGear == "R")
        {
            actualTorque *= -1;
        }

    }
}
