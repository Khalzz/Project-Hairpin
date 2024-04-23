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
        if (throttle > 0)
        {
            // Calculate rpm when throttle is pressed
            rpm = maxRpm * throttle;

            // Evaluate torque curve only when throttle is pressed
            actualTorque = throttle * 1000;
        }
        else
        {
            // When throttle is released, stop increasing rpm and torque
            rpm = 0;
            actualTorque = 0;
        }

        // Clamp rpm to maxRpm
        rpm = Mathf.Clamp(rpm, 0, maxRpm);

        // Update UI text
        rpmText.text = rpm.ToString();

        // Reset throttle and torque if throttle is close to zero
        if (Mathf.Abs(throttle) <= 0.1f)
        {
            throttle = 0;
            actualTorque = 0;
        }

        // Reverse torque if in reverse gear
        if (actualGear == "R")
        {
            actualTorque *= -1;
        }

        print(actualTorque);

    }
}