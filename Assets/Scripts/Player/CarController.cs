using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private float motorForce = 1000f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = com;

    }

    void FixedUpdate()
    {
        GetInput();
        Accelerate();
        Steer();
        updateWheelPoses();
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

        m_horizontalInput = Input.GetAxis("Horizontal");
    }

    private void Accelerate()
    {
        wheelRl.motorTorque = m_verticalInput * motorForce;
        wheelRr.motorTorque = m_verticalInput * motorForce;
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
