using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotater : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;

    [SerializeField] float spinningSpeed;
    // Start is called before the first frame update

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.AddForce(spinningSpeed * Vector2.right);
    }
}
