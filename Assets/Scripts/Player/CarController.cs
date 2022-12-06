using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using TMPro;

public class CarController : MonoBehaviour
{
    public Vector3 com;
    public bool withController;
    private Rigidbody rb;

    [SerializeField] WheelCollider wheelRl, wheelRr;
    [SerializeField] WheelCollider wheelFl, wheelFr;

    [SerializeField] Transform wheelRlT, wheelRrT, wheelFlT, wheelFrT;

    private float m_verticalInput;
    private float m_horizontalInput;
    private float m_steeringAngle;
    public float maxSteeringAngle;

    public Vector3 velocity;
    public Vector3 fDrag;
    public float cDrag = 0.37f;
    public float fSpeed;

    [Header("Power Trail")]

    public TextMeshProUGUI wheelTorqueText;
    public TextMeshProUGUI carSpeedText;
    public TextMeshProUGUI fLongText;
    public TextMeshProUGUI LWTText;
    public TextMeshProUGUI RWTText;

    EngineControl engineController;

    public float wheelRadius;
    public float hp;

    private float wheelRotationRate;
    private float rpm;
    private float wheelTorque;
    private float motorForce = 1000f;

    private float Flong;

    [SerializeField] GameObject[] cameras;
    private int actualCamera = 0;

    private void Start()
    {  
        engineController = GetComponent<EngineControl>();
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = com;
    }

    private void Update()
    {
        if (m_verticalInput >= -0.1 && m_verticalInput <= 0.1)
        {
            m_verticalInput = 0;
            wheelRl.motorTorque = 0;
            wheelRr.motorTorque = 0;
        }
        engineController.throttle = m_verticalInput;

        changeCamera();
    }

    void FixedUpdate()
    {
        GetInput();
        Accelerate();
        Steer();
        updateWheelPoses();
        TractionForce();
        showDebbug();

        velocity = GetComponent<Rigidbody>().velocity;
        fSpeed = Mathf.Sqrt(velocity.x * velocity.x + velocity.y * velocity.y);
        fDrag.x = -cDrag * velocity.x * fSpeed;
        fDrag.z = -cDrag * velocity.z * fSpeed;
    }

    public void GetInput()
    {
        if (Input.GetAxis("Throttle") > 0)
        {
            m_verticalInput = Input.GetAxis("Throttle");
        }
        else if (Input.GetAxis("Brake") > 0)
        {
            m_verticalInput = Input.GetAxis("Brake") * -1;
        }
        else
        {
            m_verticalInput = 0;
        }
        m_horizontalInput = Input.GetAxis("Horizontal");
    }

    private void changeCamera()
    {
        if (Input.GetButtonDown("Camera"))
        {
            if (actualCamera != cameras.Length - 1)
            {
                actualCamera++;
            }
            else
            {
                actualCamera = 0;
            }
            print(actualCamera);   
        }

        for (int i = 0; i < cameras.Length; i++)
        {
            if (i == actualCamera)
            {
                cameras[i].gameObject.SetActive(true);
            }
            else
            {
                cameras[i].gameObject.SetActive(false);
            }
        }
    }

    private void showDebbug()
    {
        // debug data
        wheelTorqueText.text = Flong.ToString();
        carSpeedText.text = rb.velocity.magnitude.ToString();
        fLongText.text = m_verticalInput.ToString();

        LWTText.text = wheelRl.motorTorque.ToString();
        RWTText.text = wheelRr.motorTorque.ToString();
    }

    private void TractionForce()
    {
        wheelRotationRate = rb.velocity.magnitude/wheelRadius;
        engineController.wheelRotationRate = wheelRotationRate;    

        // 2.25f + 0.5f is the differential ratio (first gear + 0.5f)
        rpm = (((wheelRotationRate * 2.25f * (4.22f)) * 60) / 6.28318530718f) * m_verticalInput;

        if (rpm < 1000) { rpm = 1000; }

        wheelTorque = ((hp / rpm) * 5252);

        Flong = ((((hp / rpm) * 5252) + fDrag.x + ((cDrag * 30)) + 2.25f) / 0.3f);
    }

    private void Accelerate()
    {

        // in newton meters
        if (m_verticalInput < 0)
        {
            wheelRl.motorTorque = -engineController.actualTorque;
            wheelRr.motorTorque = -engineController.actualTorque;
        } else
        {
            wheelRl.motorTorque = engineController.actualTorque;
            wheelRr.motorTorque = engineController.actualTorque;
        }
        // wheelFl.motorTorque = engineController.actualTorque;
        // wheelFr.motorTorque = engineController.actualTorque;

        velocity = GetComponent<Rigidbody>().velocity;
        fSpeed = Mathf.Sqrt(velocity.x * velocity.x + velocity.y * velocity.y);
        fDrag.x = -cDrag * velocity.x * fSpeed;
        fDrag.z = -cDrag * velocity.z * fSpeed;

        // print(fDrag);
    }

    private void Steer()
    {
        m_steeringAngle = m_horizontalInput * maxSteeringAngle;
        wheelFl.steerAngle = m_steeringAngle;
        wheelFr.steerAngle = m_steeringAngle;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + transform.rotation * com, 0.2f);
    }

    // make the update to every wheel
    private void updateWheelPoses()
    {
        updateWheelPose(wheelRl, wheelRlT);
        updateWheelPose(wheelRr, wheelRrT);
        updateWheelPose(wheelFl, wheelFlT);
        updateWheelPose(wheelFr, wheelFrT);
    }

    // function that updates a single wheel
    private void updateWheelPose(WheelCollider _collider, Transform _transform)
    {
        Vector3 _pos = _transform.position;
        Quaternion _quat = _transform.rotation;

        _collider.GetWorldPose(out _pos, out _quat);

        _transform.position = _pos;
        _transform.rotation = _quat;
    }
}
