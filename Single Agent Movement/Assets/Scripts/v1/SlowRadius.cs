using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowRadius : MonoBehaviour
{
    [SerializeField] public float slowRadius = 1.0f;

    private void Awake()
    {
        CircleCollider2D collider = this.gameObject.GetComponent<CircleCollider2D>();

        if (collider != null && collider.isTrigger)
        {
            slowRadius = collider.radius;
        }
    }
}
