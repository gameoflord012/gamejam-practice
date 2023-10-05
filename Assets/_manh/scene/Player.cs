using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] float jumpForceGround = 10;
    [SerializeField] float jumpForceAir = 10;
    [SerializeField] float jumpForceOvertime = 1;
    [SerializeField] float wallPushOutForce = 100;

    [SerializeField] float horizontalSpeed = 10;
    [SerializeField] float airHorizontalAcceleration = 100;
    [SerializeField] float snapThreshold = 0.1f;

    [SerializeField] ColliderFilter groundDetector;
    [SerializeField] ColliderFilter leftWallDetector;
    [SerializeField] ColliderFilter rightWallDetector;

    [SerializeField] float jumpDuration = 0.3f;

    Rigidbody2D rb;

    bool groundJumped = false;
    bool isWallJumping = false;
    float jumpTimer = 100;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (!Mathf.Approximately(GetMoveAxis().sqrMagnitude, 0))
        {
            if (IsTouchGround())
            {
                rb.velocity = new Vector2(
                    GetMoveAxis().x * horizontalSpeed,
                    rb.velocity.y);
            }
            else
            {
                rb.AddForce(GetMoveAxis() * airHorizontalAcceleration);
            }
        }

        UpdateJumpLogic();

        // Debug.Log(jumpTimer);
    }

    private void UpdateJumpLogic()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (IsTouchGround() || IsTouchWall())
            {
                rb.AddForce(Vector2.up * jumpForceGround, ForceMode2D.Impulse);

                if(!IsTouchGround())
                    rb.AddForce(GetTouchedWallDirection() * wallPushOutForce, ForceMode2D.Impulse);

                Debug.Log("GetTouchedWallDirection" + GetTouchedWallDirection());

                groundJumped = true;
                jumpTimer = 0;
            }
            else if (groundJumped)
            {
                rb.AddForce(Vector2.up * jumpForceAir, ForceMode2D.Impulse);
                groundJumped = false;

                jumpTimer = 0;
            }
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            jumpTimer = 100;
        }

        bool isJumping = jumpTimer < jumpDuration;

        if (isJumping)
        {
            rb.AddForce(Vector2.up * jumpForceOvertime * Time.deltaTime, ForceMode2D.Impulse);
            jumpTimer += Time.deltaTime;
        }
    }

    bool IsTouchWall()
    {
        return leftWallDetector.GetTouchCols().Count > 0 || rightWallDetector.GetTouchCols().Count > 0;
    }

    bool IsTouchGround()
    {
        return groundDetector.GetTouchCols().Count > 0;
    }

    Vector2 GetTouchedWallDirection()
    {
        if (leftWallDetector.GetTouchCols().Count > 0) return Vector2.left;
        if (rightWallDetector.GetTouchCols().Count > 0) return Vector2.right;

        return Vector2.zero;
    }

    Vector2 GetInputAxis()
    {
        return new Vector2(Input.GetAxis("Horizontal"), 0 * Input.GetAxis("Vertical"));
    }

    Vector2 GetMoveAxis()
    {
        return new Vector2(GetInputAxis().x, 0);
    }
}
