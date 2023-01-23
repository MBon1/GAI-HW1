using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArriveRadius : MonoBehaviour
{
    [SerializeField] public float arrivalRadius = 1.0f;

    private void Awake()
    {
        CapsuleCollider2D collider = this.gameObject.GetComponent<CapsuleCollider2D>();

        if (collider != null && !collider.isTrigger)
        {
            arrivalRadius = collider.size.x;
        }
    }
}
