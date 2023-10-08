using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

public class Player : MonoBehaviour
{
    enum PlayerInputEvent
    {
        StartMoveLeft,
        StopMoveLeft,

        StartMoveRight,
        StopMoveRight,

        StartJump,
        StopJump
    }

    enum PlayerState
    {
        OnGround,
        HugWall,
        OnAir
    }

    struct PlayerMovementData
    {
        public Vector2 horizontalMovingDirection;
        public bool startMoving;

        public Vector2 GetMoveDirection()
        {
            return horizontalMovingDirection.normalized;
        }

        public float GetMoveAxis()
        {
            return horizontalMovingDirection.x;
        }

        public bool IsMoving()
        {
            return horizontalMovingDirection.magnitude > 0.1;
        }
    }

    [Header("Jump")]
    [SerializeField] float groundJumpForce = 10;
    [SerializeField] float airJumpForce = 10;
    [SerializeField] float jumpForceOvertime = 1;
    [SerializeField] float jumpDuration = 0.3f;

    [Header("Moving")]
    [SerializeField] float groundAcceleration = 10;
    [SerializeField] float horizontalMaxSpeed = 10;
    [SerializeField] float airAcceleration = 100;


    [Header("Detector")]
    [SerializeField] ColliderFilter groundDetector;
    [SerializeField] ColliderFilter leftWallDetector;
    [SerializeField] ColliderFilter rightWallDetector;

    [Header("Wall")]
    [SerializeField] float hugWallReleaseDuration = 0.3f;
    [SerializeField] float slideDownGravity = 0.3f;
    [SerializeField] float wallPushOutForce = 10;

    [Header("Tireness Scales")]
    [Range(0f, 10   )] [SerializeField] float tireness = 1;
    [Space]
    [Range(0f, 10f  )] [SerializeField] float groundAccelerationModifier = 1;
    [Range(0f, 5f   )] [SerializeField] float horizontalMaxSpeedModifier = 1;
    [Range(0f, 10f  )] [SerializeField] float airAccelerationModifier = 1;
    [Space]
    [Range(0f, 5f   )] [SerializeField] float airJumpForceModifier = 1;
    [Range(0f, 5f   )] [SerializeField] float groundJumpForceModifier = 1;
    [Range(0f, 10f  )] [SerializeField] float jumpForceOvertimeModifier = 1;
    [Space]
    [Range(0f, 10f  )] [SerializeField] float slideDownSpeedModifier = 1;
    [Range(0f, 5f   )] [SerializeField] float wallPushOutForceModifier = 1;
  

    Rigidbody2D rb;

    bool groundJumped = false;

    float jumpTimer = 100;
    float hugWallReleaseTimer = 100;
    float initialGravityScale;

    List<PlayerInputEvent> playerEvents = new();
    PlayerState currentPlayerState;
    PlayerMovementData movementData;


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
        UpdateCheckPlayerState();
        UpdateHorizontalMovement();
        UpdateJumpLogic();
        UpdateHugWallLogic();

        //Debug.Log(currentPlayerState.ToString());

        ProcessPlayerEvents();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("obstacle"))
        {
            gameObject.SetActive(false);
        }
    }

    private void ProcessPlayerEvents()
    {
        movementData.startMoving = false;

        while (playerEvents.Count > 0)
        {
            PlayerInputEvent e = playerEvents[0];

            switch (e)
            {
                case PlayerInputEvent.StartMoveLeft:
                    movementData.horizontalMovingDirection += Vector2.left;

                    break;
                case PlayerInputEvent.StopMoveLeft:
                    movementData.horizontalMovingDirection -= Vector2.left;

                    break;
                case PlayerInputEvent.StartMoveRight:
                    movementData.horizontalMovingDirection += Vector2.right;

                    break;
                case PlayerInputEvent.StopMoveRight:
                    movementData.horizontalMovingDirection -= Vector2.right;

                    break;
                case PlayerInputEvent.StartJump:
                    break;
                case PlayerInputEvent.StopJump:
                    break;
                default:
                    break;
            }

            if(movementData.IsMoving())
            {
                movementData.startMoving = true;
            }

            playerEvents.RemoveAt(0);
        }
    }

    private void UpdateCheckPlayerState()
    {
        if (IsOnGround()) currentPlayerState = PlayerState.OnGround;
        else if (IsHugWall()) currentPlayerState = PlayerState.HugWall;
        else currentPlayerState = PlayerState.OnAir;
    }

    private void UpdateHugWallLogic()
    {
        rb.gravityScale = initialGravityScale;

        if (currentPlayerState != PlayerState.HugWall) return;

        rb.gravityScale = GetSlideDownGravity();

        if (movementData.IsMoving() && SignOf(GetHugWallDirection().x) != SignOf(movementData.GetMoveAxis()))
        {
            if( movementData.startMoving )
            {
                hugWallReleaseTimer = 0;
                Debug.Log("============================");
            }

            Debug.Log(hugWallReleaseTimer);

            if (hugWallReleaseTimer > hugWallReleaseDuration)
            {
                 rb.AddForce(movementData.GetMoveDirection() * GetAirAcceleration());
            }
        }

        hugWallReleaseTimer += Time.deltaTime;
    }

    private void UpdateHorizontalMovement()
    {
        if(currentPlayerState == PlayerState.OnAir || currentPlayerState == PlayerState.OnGround)
        {
            if (movementData.IsMoving())
            {
                float acceleration = IsOnGround() ? GetGroundAcceleration() : GetAirAcceleration();

                if (SignOf(rb.velocity.x) != SignOf(movementData.GetMoveAxis()) ||
                       Mathf.Abs(rb.velocity.x) < GetHorizontalSpeed())
                {
                    rb.AddForce(movementData.GetMoveDirection() * acceleration);
                }
            }
        }
    }
    private void UpdateJumpLogic()
    {
        if (playerEvents.Contains(PlayerInputEvent.StartJump))
        {
            if (currentPlayerState == PlayerState.HugWall || currentPlayerState == PlayerState.OnGround)
            {
                rb.AddForce(Vector2.up * GetGroundJumpForce(), ForceMode2D.Impulse);

                if(currentPlayerState == PlayerState.HugWall)
                    rb.AddForce(-GetHugWallDirection() * GetWallPushOutForce(), ForceMode2D.Impulse);

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

        if (playerEvents.Contains(PlayerInputEvent.StopJump))
        {
            jumpTimer = 100;
        }

        bool isJumping = jumpTimer < jumpDuration;

        if (isJumping && currentPlayerState == PlayerState.OnAir)
        {
            rb.AddForce(Vector2.up * GetJumpForceOvertime());
            jumpTimer += Time.deltaTime;
        }
    }
    
    bool IsHugWall()
    {
        return leftWallDetector.GetTouchCols().Count > 0 || rightWallDetector.GetTouchCols().Count > 0;
    }

    bool IsOnGround()
    {
        return groundDetector.GetTouchCols().Count > 0;
    }

    Vector2 GetHugWallDirection()
    {
        if (leftWallDetector.GetTouchCols().Count > 0) return Vector2.left;
        if (rightWallDetector.GetTouchCols().Count > 0) return Vector2.right;

        return Vector2.zero;
    }

    private void InputHandler()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            playerEvents.Add(PlayerInputEvent.StartJump);

        if (Input.GetKeyUp(KeyCode.Space))
            playerEvents.Add(PlayerInputEvent.StopJump);

        if (Input.GetKeyDown(KeyCode.A))
            playerEvents.Add(PlayerInputEvent.StartMoveLeft);

        if (Input.GetKeyUp(KeyCode.A))
            playerEvents.Add(PlayerInputEvent.StopMoveLeft);

        if (Input.GetKeyDown(KeyCode.D))
            playerEvents.Add(PlayerInputEvent.StartMoveRight);

        if (Input.GetKeyUp(KeyCode.D))
            playerEvents.Add(PlayerInputEvent.StopMoveRight);
    }

    float GetHorizontalSpeed()
    {
        return horizontalMaxSpeed - Mathf.Min(horizontalMaxSpeed, tireness * horizontalMaxSpeedModifier);
    }

    float GetGroundJumpForce()
    {
        return groundJumpForce - Mathf.Min(groundJumpForce, tireness * groundJumpForceModifier);
    }

    float GetSlideDownGravity()
    {
        return slideDownGravity - Mathf.Min(slideDownGravity, tireness * slideDownSpeedModifier);
    }

    float GetAirAcceleration()
    {
        return airAcceleration - Mathf.Min(airAcceleration, tireness * airAccelerationModifier);
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

    float GetGroundAcceleration()
    {
        return groundAcceleration - Mathf.Min(groundAcceleration, tireness * groundAccelerationModifier);
    }

    int SignOf(float value)
    {
        return value > 0 ? 1 : value < 0 ? -1 : 0;
    }
}
