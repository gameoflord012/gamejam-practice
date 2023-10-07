using System;
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


    [Header("Detector")]
    [SerializeField] ColliderFilter groundDetector;
    [SerializeField] ColliderFilter leftWallDetector;
    [SerializeField] ColliderFilter rightWallDetector;

    [Header("Wall")]
    [SerializeField] float hugWallForce;
    [SerializeField] float wallPushOutForce = 10;

    [Header("Strength Scales")]
    [Range(0f, 6)]
    [SerializeField] float tireness = 1;
    [Range(0f, 5f)]
    [SerializeField] float groundJumpForceModifier = 1;
    [Range(0f, 5f)]
    [SerializeField] float airJumpForceModifier = 1;
    [Range(0f, 5f)]
    [SerializeField] float jumpForceOvertimeModifier = 1;
    [Range(0f, 5f)]
    [SerializeField] float horizontalSpeedModifier = 1;
    [Range(0f, 5f)]
    [SerializeField] float airHorizontalAccelerationModifier = 1;
    [Range(0f, 5f)]
    [SerializeField] float hugWallForceModifier = 1;
    [Range(0f, 5f)]
    [SerializeField] float wallPushOutForceModifier = 1;

    Rigidbody2D rb;

    bool groundJumped = false;
    float jumpTimer = 100;

    List<CharacterEvent> playerEvents = new();

    float GetHorizontalSpeed()
    {
        return horizontalSpeed - Mathf.Min(horizontalSpeed, tireness * horizontalSpeedModifier);
    }

    float GetGroundJumpForce()
    {
        return groundJumpForce - Mathf.Min(groundJumpForce, tireness * groundJumpForceModifier);
    }

    float GetHugWallForce()
    {
        return hugWallForce - Mathf.Min(hugWallForce, tireness * hugWallForceModifier);
    }

    float GetAirHorizontalAcceleration()
    {
        return airHorizontalAcceleration - Mathf.Min(airHorizontalAcceleration, tireness * airHorizontalAccelerationModifier);
    }

    float GetAirJumpForce()
    {
        return airJumpForce - Mathf.Min(airJumpForce, tireness * airJumpForceModifier);
    }

    float GetWallPushOutForce()
    {
        return wallPushOutForce - Mathf.Min(wallPushOutForce, tireness * wallPushOutForceModifier);
    }

    float GetJumpForceOvertime()
    {
        return jumpForceOvertime - Mathf.Min(jumpForceOvertime, tireness * jumpForceOvertimeModifier);
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        InputHandler();
    } 
    private void FixedUpdate()
    {
        UpdateHorizontalMovement();
        UpdateJumpLogic();
        UpdateHugWallLogic();
        playerEvents.Clear();
    }

    private void UpdateHugWallLogic()
    {
        if(IsTouchWall() && rb.velocity.y < 0)
        {
            rb.AddForce(Vector2.up * GetHugWallForce());
        }
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
