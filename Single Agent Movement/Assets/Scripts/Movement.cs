using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Movement : MonoBehaviour
{
    [SerializeField] public float maxSpeed = 10.0f;
    [SerializeField] public float maxAcceleration = 2.0f;

    public SteeringOutput steering= new SteeringOutput { linearAcceleration = Vector3.zero, angularRoation = 0.0f };

    Rigidbody2D characterRB;
    [SerializeField] private Vector3 characterVelocity = Vector3.zero;

    // Target
    [SerializeField] private GameObject target = null;
    [SerializeField] public float targetSlowRadius = 0.0f;
    [SerializeField] private float timeToTarget = 0.5f;


    private void Awake()
    {
        characterRB = this.gameObject.GetComponent<Rigidbody2D>();

        SlowRadius slowRadius = target.GetComponent<SlowRadius>();
        if (slowRadius != null)
        {
            targetSlowRadius = slowRadius.slowRadius;
        }
    }

    private void FixedUpdate()
    {
        DynamicArrive();
    }

    private void UpdateCharacterVelocity()
    {
        characterRB.transform.position += characterVelocity * Time.fixedDeltaTime;

        characterVelocity += steering.linearAcceleration * Time.fixedDeltaTime;

        if (characterVelocity.magnitude > maxSpeed)
        {
            characterVelocity = characterVelocity.normalized * maxSpeed;
        }
    }

    private void DynamicArrive()
    {
        // Get Target's Position
        Vector3 targetPos = target.transform.position;

        // Align to Target  (currently snaps to direction)
        Vector3 direction = targetPos - this.gameObject.transform.position;
        float distanceToTarget = direction.magnitude;

        // Check if the player is ontop of the target
        if (distanceToTarget < 0.05f)
        {
            steering.linearAcceleration = Vector3.zero;
            characterVelocity = Vector3.zero;
        }
        else
        {
            float targetSpeed = 0.0f;
            if (distanceToTarget > targetSlowRadius)
            {
                targetSpeed = maxSpeed;
            }
            else
            {
                targetSpeed = maxSpeed * (distanceToTarget / targetSlowRadius);
            }

            Vector3 targetVelocity = direction.normalized * targetSpeed;

            steering.linearAcceleration = (targetVelocity - characterVelocity) / timeToTarget;
            if (steering.linearAcceleration.magnitude > maxAcceleration)
            {
                steering.linearAcceleration = steering.linearAcceleration.normalized * maxAcceleration;
            }
        }

        UpdateCharacterVelocity();
    }


    [System.Serializable]
    public struct SteeringOutput
    {
        public Vector3 linearAcceleration;     // using Vector3 so this can easily be moved to 3D if need be
        public float angularRoation;
    }
}
