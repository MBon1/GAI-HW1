using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SteeringOutput = Movement.SteeringOutput;
public class NewMovement : MonoBehaviour
{
    // Character
    [Header("Character")]
    Rigidbody2D characterRB;

    [SerializeField]
    MovementOperation movementType = MovementOperation.None;

    [SerializeField] public float maxSpeed = 10.0f;
    [SerializeField] public float maxAcceleration = 2.0f;
    [SerializeField] public float maxRotationSpeed = 2.0f;
    [SerializeField] public float maxRotationAcceleration = 1.0f;
    [SerializeField] private Vector3 characterVelocity = Vector3.zero;
    [SerializeField] private float characterArrivalRadius = 1.0f;
    [SerializeField] public float fleeSafeDistance = 5.0f;
    [SerializeField] public float fleeSafeRadius = 1.0f;
    public SteeringOutput steering = new SteeringOutput { linearAcceleration = Vector3.zero, angularRoation = 0.0f };

    // Wander-Specific
    [SerializeField] float wanderRadius = 10;
    [SerializeField] float wanderDistance = 10;
    [SerializeField] float wanderJitter = 1;
    Vector3 wanderTarget = Vector3.zero;

    // Path-Following Specific
    [SerializeField] Transform[] pointsOnPath = { };
    [SerializeField] PathFollowingTypes pathFollowingType = PathFollowingTypes.OneTime;
    int nextPointIndex = 0;
    [SerializeField] private float pathPointArrivalRadius = 0.1f;


    // Target
    [Header("Target")]
    [SerializeField] private GameObject target = null;
    [SerializeField] public float targetSlowRadius = 0.0f;
    [SerializeField] private float targetArrivalRadius;
    [SerializeField] private float timeToTarget = 0.5f;
    Vector3 targetLastPosition;

    // Start is called before the first frame update
    void Start()
    {
        characterRB = this.gameObject.GetComponent<Rigidbody2D>();
        ArriveRadius arriveRadius = this.gameObject.GetComponent<ArriveRadius>();
        if (arriveRadius == null)
        {
            characterArrivalRadius = 0.0f;
        }
        else
        {
            characterArrivalRadius = arriveRadius.arrivalRadius;
        }

        arriveRadius = target.GetComponent<ArriveRadius>();
        if (arriveRadius == null)
        {
            targetArrivalRadius = 0.0f;
        }
        else
        {
            targetArrivalRadius = arriveRadius.arrivalRadius;
        }

        SlowRadius slowRadius = target.GetComponent<SlowRadius>();
        if (slowRadius == null)
        {
            targetSlowRadius = 0.0f;
        }
        else
        {
            targetSlowRadius = slowRadius.slowRadius;
        }

        targetLastPosition = target.transform.position;
    }


    void FixedUpdate()
    {
        //Seek(target.transform.position, characterArrivalRadius, targetArrivalRadius, targetSlowRadius);
        //Flee(target.transform.position, fleeSafeRadius);
        //Pursue();
        //Evade();
        //Wander();
        //PathFollowing();

        if (movementType == MovementOperation.Pursue)
        {
            Pursue();
        }else if (movementType == MovementOperation.Evade)
        {
            Evade();
        }else if (movementType == MovementOperation.Wander)
        {
            Wander();
        }
        else if (movementType == MovementOperation.FollowPath)
        {
            PathFollowing();
        }
        else
        {
            return;
        }
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

    void Seek(Vector3 location, float charactertArriveRadius, float targetArriveRadius, float slowRadius)
    {
        if (targetArriveRadius >= slowRadius)
        {
            Debug.LogError("Invalid target arrival radius and/or slow radius");
        }


        Vector3 direction = location - this.gameObject.transform.position;
        float distanceToTarget = direction.magnitude;

        // Check if the player is ontop of the target
        if (distanceToTarget < (charactertArriveRadius / 2 + targetArriveRadius / 2))
        {
            steering.linearAcceleration = Vector3.zero;
            characterVelocity = Vector3.zero;
        }
        else
        {
            float targetSpeed;
            if (distanceToTarget > slowRadius)
            {
                targetSpeed = maxSpeed;
            }
            else
            {
                targetSpeed = maxSpeed * (distanceToTarget / (slowRadius - targetArriveRadius));
            }

            Vector3 targetVelocity = direction.normalized * targetSpeed;

            steering.linearAcceleration = (targetVelocity - characterVelocity) / timeToTarget;
            if (steering.linearAcceleration.magnitude > maxAcceleration)
            {
                steering.linearAcceleration = steering.linearAcceleration.normalized * maxAcceleration;
            }
        }

        UpdateCharacterVelocity();

        OrientTo(location);
    }

    void Flee(Vector3 location, float safeRadius)
    {
        Vector3 fleeVector = location - this.gameObject.transform.position;
        Seek(this.gameObject.transform.position - fleeVector, 0, 0, safeRadius);
    }

    void Flee(Vector3 location, float maxDistFromLocation, float safeRadius)
    {
        Vector3 direction = location - this.gameObject.transform.position;
        float distanceToTarget = direction.magnitude;

        // Check if the player is ontop of the target
        if (distanceToTarget >= maxDistFromLocation)
        {
            steering.linearAcceleration = Vector3.zero;
            characterVelocity = Vector3.zero;
        }
        else
        {
            float targetSpeed;
            if (distanceToTarget > safeRadius)
            {
                targetSpeed = maxSpeed;
            }
            else
            {
                targetSpeed = maxSpeed * (distanceToTarget / safeRadius);
            }

            Vector3 targetVelocity = direction.normalized * targetSpeed;

            steering.linearAcceleration = (characterVelocity - targetVelocity) / timeToTarget;
            if (steering.linearAcceleration.magnitude > maxAcceleration)
            {
                steering.linearAcceleration = steering.linearAcceleration.normalized * maxAcceleration;
            }
        }

        UpdateCharacterVelocity();

        OrientTo(location);
    }

    void Pursue()
    {
        Vector3 targetDir = target.transform.position - this.transform.position;
        //float targetSpeed = (target.transform.position - targetLastPosition).magnitude;      // How should I get target speed?
        float targetSpeed = target.GetComponent<Drive>().currentSpeed;
        //targetLastPosition = target.transform.position;

        float relativeHeading = Vector3.Angle(this.transform.up, this.transform.TransformVector(target.transform.up));
        float toTarget = Vector3.Angle(this.transform.up, this.transform.TransformVector(targetDir));

        if ((toTarget > 90 && relativeHeading < 20) || targetSpeed < 0.01f)
        {
            Seek(target.transform.position, characterArrivalRadius, targetArrivalRadius, targetSlowRadius);
            Debug.Log("SEEKING");
            return;
        }

        float lookAhead = targetDir.magnitude / (targetSpeed);
        Seek(target.transform.position + target.transform.up * lookAhead, characterArrivalRadius, targetArrivalRadius, targetSlowRadius);
        Debug.Log("PURSUEING");
    }

    void Evade()
    {
        Vector3 targetDir = target.transform.position - this.transform.position;
        //float targetSpeed = (target.transform.position - targetLastPosition).magnitude;      // How should I get target speed?
        float targetSpeed = target.GetComponent<Drive>().currentSpeed;
        targetLastPosition = target.transform.position;

        float lookAhead;
        if (targetSpeed == 0)
        {
            lookAhead = targetDir.magnitude / (targetSpeed);
        }

        lookAhead = targetDir.magnitude;

        Vector3 n = target.transform.position + target.transform.up * lookAhead;
        n.z = 0.0f;
        Flee(n, fleeSafeRadius);
    }

    void Wander()
    {
        wanderTarget += new Vector3(Random.Range(-1.0f, 1.0f) * wanderJitter,
                                    0,
                                    Random.Range(-1.0f, 1.0f) * wanderJitter);

        wanderTarget.Normalize();
        wanderTarget *= wanderRadius;

        Vector3 targetLocal = wanderTarget + new Vector3(0, 0, wanderDistance);
        Vector3 targetWorld = this.gameObject.transform.InverseTransformVector(targetLocal);

        Seek(targetWorld, 0, 0, wanderRadius / 4);

    }

    void PathFollowing()
    {
        if (nextPointIndex >= pointsOnPath.Length)
        {
            if (pathFollowingType == PathFollowingTypes.OneTime)
            {
                return;
            }
            else
            {
                nextPointIndex %= pointsOnPath.Length;
            }
        }

        /*for(int i = 0; i < pointsOnPath.Length; i++)
        {

        }*/

        Vector3 nextPoint = pointsOnPath[nextPointIndex].position;
        nextPoint = new Vector3(nextPoint.x, nextPoint.y, this.transform.position.z);
        Seek(nextPoint, characterArrivalRadius, pathPointArrivalRadius, characterArrivalRadius / 2);

        if (Vector3.Distance(this.transform.position, nextPoint) <= characterArrivalRadius + pathPointArrivalRadius)
        {
            nextPointIndex++;
        }
    }


    void OrientTo(Vector3 target)
    {
        // Determine which direction to rotate towards
        Vector3 targetDirection = target - this.transform.position;

        // The step size is equal to speed times frame time.
        float singleStep = maxRotationSpeed * Time.fixedDeltaTime;

        this.gameObject.transform.up = Vector3.RotateTowards(this.gameObject.transform.up, targetDirection, singleStep, Mathf.PI);
    }


    /*void Align(Vector3 location, float charactertArriveRadius, float targetArriveRadius, float slowRadius)
    {
        if (targetArriveRadius >= slowRadius)
        {
            Debug.LogError("Invalid target arrival radius and/or slow radius");
        }

        Vector3 direction = location - this.gameObject.transform.position;
        float distanceToTarget = direction.magnitude;

        // Check if the player is ontop of the target
        if (distanceToTarget < (charactertArriveRadius / 2 + targetArriveRadius / 2))
        {
            steering.linearAcceleration = Vector3.zero;
            characterVelocity = Vector3.zero;
        }
        else
        {
            float targetSpeed;
            if (distanceToTarget > slowRadius)
            {
                targetSpeed = maxSpeed;
            }
            else
            {
                targetSpeed = maxSpeed * (distanceToTarget / (slowRadius - targetArriveRadius));
            }

            Vector3 targetVelocity = direction.normalized * targetSpeed;

            steering.linearAcceleration = (targetVelocity - characterVelocity) / timeToTarget;
            if (steering.linearAcceleration.magnitude > maxAcceleration)
            {
                steering.linearAcceleration = steering.linearAcceleration.normalized * maxAcceleration;
            }
        }
    }*/



    private enum PathFollowingTypes
    {
        OneTime,
        Cyclical
    }

    private enum MovementOperation
    {
        None, 
        Pursue,
        Evade,
        Wander, 
        FollowPath
    }

    /*void Flee(Vector3 location, float maxDistFromLocation, float safeRadius)
    {
        // Align to Target
        Vector3 direction = this.gameObject.transform.position - location;
        float distanceFromTarget = direction.magnitude;

        // Check if the player is ontop of the target
        if (maxDistFromLocation >= 0 && distanceFromTarget >= maxDistFromLocation)
        {
            steering.linearAcceleration = Vector3.zero;
            characterVelocity = Vector3.zero;
        }
        else
        {
            float targetSpeed;
            if (distanceFromTarget > safeRadius)
            {
                targetSpeed = maxSpeed;
            }
            else
            {
                targetSpeed = maxSpeed * (distanceFromTarget / safeRadius);
            }

            Vector3 targetVelocity = direction.normalized * targetSpeed;

            steering.linearAcceleration = (characterVelocity - targetVelocity) / timeToTarget;
            if (steering.linearAcceleration.magnitude > maxAcceleration)
            {
                steering.linearAcceleration = steering.linearAcceleration.normalized * maxAcceleration;
            }
        }

        UpdateCharacterVelocity();
    }*/

    /*void Flee(Vector3 location)
    {
        // Align to Target
        Vector3 direction = this.gameObject.transform.position - (location - this.gameObject.transform.position);
        float distanceFromTarget = direction.magnitude;


        float targetSpeed = maxSpeed / distanceFromTarget;

        if (targetSpeed < 0.01f)
        {
            steering.linearAcceleration = Vector3.zero;
            characterVelocity = Vector3.zero;
        }
        else
        {
            Vector3 targetVelocity = direction.normalized * targetSpeed;

            steering.linearAcceleration = (characterVelocity - targetVelocity) / timeToTarget;
            if (steering.linearAcceleration.magnitude > maxAcceleration)
            {
                steering.linearAcceleration = steering.linearAcceleration.normalized * maxAcceleration;
            }
        }

        UpdateCharacterVelocity();
    }*/
}
