using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] float speed = 3;
    [SerializeField] float jumpForce = 10;
 
    Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
            rb.velocity = new Vector2(GetInputAxis().x * speed, rb.velocity.y);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    Vector2 GetInputAxis()
    {
        return new Vector2(Input.GetAxis("Horizontal"), 0 * Input.GetAxis("Vertical"));
    }
}
