using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SteeringOutput = Movement.SteeringOutput;

[RequireComponent(typeof(Movement))]
public class Alignment : MonoBehaviour
{
    // Character
    [SerializeField] public float maxRotation = 1.0f;
    [SerializeField] public float maxAngularAcceleration = 2.0f;

    protected SteeringOutput steering;

    Rigidbody2D characterRB;
    [SerializeField] protected float rotationSpeed = 0.0f;

    // Target
    [SerializeField] protected GameObject target = null;
    [SerializeField] protected float targetSlowRadius = 0.0f;
    [SerializeField] protected float timeToTarget = 0.5f;

    private void Start()
    {
        Movement movement = this.GetComponent<Movement>();

        steering = movement.steering;

        targetSlowRadius = movement.targetSlowRadius;
    }

    private void FixedUpdate()
    {
        Align();
    }


    protected void Align()
    {
        // Rotation
        float rotationAmt = Mathf.DeltaAngle(this.transform.rotation.eulerAngles.z, target.transform.rotation.eulerAngles.z);

        // Map rotation to the (-pi, pi) interval
        float rotationDir = rotationAmt / Mathf.Abs(rotationAmt);


        if (Mathf.Abs(rotationAmt) < 1.5f)
        {
            steering.angularRoation = 0;
            rotationSpeed = 0;
        }
        else
        {
            //Initially character turn at maximum angular speed
            // Slow down after reaching the slow radius
            float targetRotation = 0.0f;
            if (Mathf.Abs(rotationAmt) > targetSlowRadius)
            {
                targetRotation = maxRotation;
            }
            else
            {
                targetRotation = maxRotation * (Mathf.Abs(rotationAmt) / targetSlowRadius);
            }

            targetRotation *= rotationDir;

            steering.angularRoation = (targetRotation - rotationSpeed) / timeToTarget;

            if (Mathf.Abs(steering.angularRoation) > maxAngularAcceleration)
            {
                steering.angularRoation = steering.angularRoation / Mathf.Abs(steering.angularRoation) * maxAngularAcceleration;
            }
        }

        // In fixed amount of time, bring character's rotation speed to the same as target's
        if (rotationSpeed != 0 && steering.angularRoation != 0)
        {
            this.gameObject.transform.rotation *= Quaternion.Euler(0, 0, rotationSpeed);
        }

        rotationSpeed += steering.angularRoation * Time.fixedDeltaTime;

        if (rotationSpeed > maxRotation)
        {
            rotationSpeed /= Mathf.Abs(rotationSpeed) * maxRotation;
        }
    }
}
