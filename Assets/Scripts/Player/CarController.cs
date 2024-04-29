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
    private float maxBrakeTorque = 100000f;
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
        print(wheelRl.motorTorque);
        GetInput();
        Accelerate();
        Steer();
        updateWheelPoses();
        TractionForce();
        ApplyDrag();
        showDebbug();
    }

    public void ApplyDrag()
    {
        Vector3 velocity = rb.velocity;
        float fSpeed = velocity.magnitude;
        Vector3 fDrag = -cDrag * velocity.normalized * fSpeed; // Drag force opposes velocity

        // Apply the drag force to the rigidbody
        rb.AddForce(fDrag, ForceMode.Force);
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
        

        Flong = ((((hp / rpm) * 5252) + fDrag.x + ((cDrag * 30)) + 2.25f) / 0.3f);
    }

    private void Accelerate()
    {

        if (m_verticalInput > 0)
        {
            // Acceleration
            wheelRl.motorTorque = engineController.actualTorque;
            wheelRr.motorTorque = engineController.actualTorque;
        }
        if (m_verticalInput < 0)
        {
            // Braking: Apply braking torque to slow down the wheels
            wheelRl.brakeTorque = maxBrakeTorque * -m_verticalInput;
            wheelRr.brakeTorque = maxBrakeTorque * -m_verticalInput;
            wheelFl.brakeTorque = maxBrakeTorque * -m_verticalInput;
            wheelFr.brakeTorque = maxBrakeTorque * -m_verticalInput;
        }
        
        if (m_verticalInput == 0)
        {
            // No throttle or brake input: Set torque to 0
            wheelRl.motorTorque = 0;
            wheelRr.motorTorque = 0;
            wheelRl.brakeTorque = 0; // Apply slight brake torque to prevent rolling
            wheelRr.brakeTorque = 0; // Apply slight brake torque to prevent rolling
            wheelFl.brakeTorque = 0;
            wheelFr.brakeTorque = 0;
        }
        // wheelFl.motorTorque = engineController.actualTorque;
        // wheelFr.motorTorque = engineController.actualTorque;

        // wheelFl.motorTorque = engineController.actualTorque;
        // wheelFr.motorTorque = engineController.actualTorque;



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
