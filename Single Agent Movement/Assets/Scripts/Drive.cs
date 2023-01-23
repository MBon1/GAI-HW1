using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drive : MonoBehaviour
{
    Rigidbody2D rb;

    public float speed = 10.0f;
    public float rotationSpeed = 100.0f;
    public float currentSpeed = 0;

    private void Start()
    {
        rb = this.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Get the horizontal and vertical axis.
        // By default they are mapped to the arrow keys.
        // The value is in the range -1 to 1
        float translation = Input.GetAxis("Vertical") * speed;
        float rotation = Input.GetAxis("Horizontal") * rotationSpeed;

        /*// Make it move 10 meters per second instead of 10 meters per frame...
        translation *= Time.deltaTime;
        rotation *= Time.deltaTime;

        // Move translation along the object's z-axis
        transform.Translate(0, translation, 0);
        currentSpeed = translation;

        // Rotate around our y-axis
        transform.Rotate(0, 0, rotation);
        
        currentSpeed = translation;*/

        rb.velocity = new Vector2(Input.GetAxis("Horizontal") * speed * Time.deltaTime, Input.GetAxis("Vertical") * speed * Time.deltaTime);

        currentSpeed = rb.velocity.magnitude;
    }
}
