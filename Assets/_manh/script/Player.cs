using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

public class Player : MonoBehaviour
{
    enum CharacterEvent
    {
        StartMoveLeft,
        StopMoveLeft,

        StartMoveRight,
        StopMoveRight,

        StartJump,
        StopJump
    }

    [Header("Jump")]
    [SerializeField] float groundJumpForce = 10;
    [SerializeField] float airJumpForce = 10;
    [SerializeField] float jumpForceOvertime = 1;
    [SerializeField] float jumpDuration = 0.3f;

    [Header("Moving")]
    [SerializeField] float horizontalSpeed = 10;
    [SerializeField] float airHorizontalAcceleration = 100;


    [Header("Collider")]
    [SerializeField] ColliderFilter groundDetector;
    [SerializeField] ColliderFilter leftWallDetector;
    [SerializeField] ColliderFilter rightWallDetector;

    [Header("Others")]
    [SerializeField] float globalStrengthScale = 1;
    [SerializeField] float wallPushOutForce = 100;
    [SerializeField] float wallFriction = 0.5f;

    Rigidbody2D rb;

    bool groundJumped = false;
    float initialGravityScale;
    float jumpTimer = 100;

    List<CharacterEvent> playerEvents = new();

    float GetHorizontalSpeed()
    {
        return horizontalSpeed * globalStrengthScale;
    }

    float GetGroundJumpForce()
    {
        return groundJumpForce * globalStrengthScale;
    }

    float GetWallFriction()
    {
        return wallFriction / globalStrengthScale;
    }

    float GetAirHorizontalAcceleration()
    {
        return airHorizontalAcceleration * globalStrengthScale;
    }

    float GetAirJumpForce()
    {
        return airJumpForce * globalStrengthScale;
    }

    float GetWallPushOutForce()
    {
        return wallPushOutForce * globalStrengthScale;
    }

    float GetJumpForceOvertime()
    {
        return jumpForceOvertime * globalStrengthScale;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        initialGravityScale = rb.gravityScale;
    }

    private void Update()
    {
        InputHandler();
    } 
    private void FixedUpdate()
    {
        UpdateHorizontalMovement();
        UpdateJumpLogic();
        rb.gravityScale = IsTouchWall() ? initialGravityScale * GetWallFriction() : initialGravityScale;

        playerEvents.Clear();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("obstacle"))
        {
            gameObject.SetActive(false);
        }
    }

    private void UpdateHorizontalMovement()
    {
        if (IsReceiveMovingInput())
        {
            if (IsOnGround())
            {
                rb.velocity = new Vector2(
                    GetMoveAxis().x * GetHorizontalSpeed(),
                    rb.velocity.y);
            }
            else
            {
                rb.AddForce(GetMoveAxis() * GetAirHorizontalAcceleration());
            }
        }
    }
    private void UpdateJumpLogic()
    {
        if (playerEvents.Contains(CharacterEvent.StartJump))
        {
            if (IsOnGround() || IsTouchWall())
            {
                rb.AddForce(Vector2.up * GetGroundJumpForce(), ForceMode2D.Impulse);

                if(!IsOnGround())
                    rb.AddForce(GetTouchedWallDirection() * GetWallPushOutForce(), ForceMode2D.Impulse);

                // Debug.Log("GetTouchedWallDirection" + GetTouchedWallDirection());

                groundJumped = true;
                jumpTimer = 0;
            }
            else if (groundJumped)
            {
                rb.AddForce(Vector2.up * GetAirJumpForce(), ForceMode2D.Impulse);
                groundJumped = false;

                jumpTimer = 0;
            }
        }

        if (playerEvents.Contains(CharacterEvent.StopJump))
        {
            jumpTimer = 100;
        }

        bool isJumping = jumpTimer < jumpDuration;

        if (isJumping)
        {
            rb.AddForce(Vector2.up * GetJumpForceOvertime());
            jumpTimer += Time.deltaTime;
        }
    }
    private bool IsReceiveMovingInput()
    {
        return GetMoveAxis().magnitude > 0.1;
    }

    bool IsTouchWall()
    {
        return leftWallDetector.GetTouchCols().Count > 0 || rightWallDetector.GetTouchCols().Count > 0;
    }

    bool IsOnGround()
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
        return new Vector2(
            (Input.GetKey(KeyCode.A) ? -1 : 0) +
            (Input.GetKey(KeyCode.D) ? 1 : 0),
            0);
    }

    Vector2 GetMoveAxis()
    {
        return new Vector2(GetInputAxis().x, 0);
    }

    private void InputHandler()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            playerEvents.Add(CharacterEvent.StartJump);

        if (Input.GetKeyUp(KeyCode.Space))
            playerEvents.Add(CharacterEvent.StopJump);

        if (Input.GetKeyDown(KeyCode.A))
            playerEvents.Add(CharacterEvent.StartMoveLeft);

        if (Input.GetKeyUp(KeyCode.A))
            playerEvents.Add(CharacterEvent.StopMoveLeft);

        if (Input.GetKeyDown(KeyCode.D))
            playerEvents.Add(CharacterEvent.StartMoveRight);

        if (Input.GetKeyUp(KeyCode.D))
            playerEvents.Add(CharacterEvent.StopMoveRight);
    }
}
