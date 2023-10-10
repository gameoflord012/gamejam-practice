using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;

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

    enum PlayerStat
    {
        GroundJumpVelocity,
        AirJumpVelocity,
        JumpForceOverTime,

        GroundAcceleration,
        AirAcceleration,

        WallSlideDrag,
        WallPushOutVelocity,
        WallPushUpVelocity
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

    [Header("Detector")]
    [SerializeField] ColliderFilter groundDetector;
    [SerializeField] ColliderFilter leftWallDetector;
    [SerializeField] ColliderFilter rightWallDetector;

    [Header("Jump")]
    [SerializeField] float groundJumpVelocity = 10;
    [SerializeField] float airJumpVelocity = 10;
    [SerializeField] float jumpForceOvertime = 1;
    [SerializeField] float jumpDuration = 0.3f;

    [Header("Horizontal")]
    [SerializeField] float groundAcceleration = 10;
    [SerializeField] float airAcceleration = 100;
    
    [Header("Wall")]
    [SerializeField] float hugWallReleaseDuration = 0.3f;
    [SerializeField] float wallPushOutVelocity = 10;
    [SerializeField] float wallPushUpVelocity = 10;

    [Header("Friction")]
    [SerializeField][Range(0, 1)] float groundDrag = 10;
    [SerializeField][Range(0, 1)] float horizontalAirDrag = 10;
    [SerializeField][Range(0f, 1f)] float wallSlideDrag = 0.3f;

    [Header("Tireness Scales")]
    [Range(0f, 10   )] [SerializeField] float tireness = 1;
    [Header("Modifier")]
    [Range(0f, 10f  )] [SerializeField] float groundAccelerationModifier = 1;
    [Range(0f, 10f  )] [SerializeField] float airAccelerationModifier = 1;
    [Space]
    [Range(0f, 5f   )] [SerializeField] float groundJumpVelocityModifier = 1;
    [Range(0f, 5f   )] [SerializeField] float airJumpVelocityModifier = 1;
    [Range(0f, 10f  )] [SerializeField] float jumpForceOvertimeModifier = 1;
    [Space]
    [Range(0f, 10f  )] [SerializeField] float wallSlideDragModifier;
    [Range(0f, 5f   )] [SerializeField] float wallPushOutVelocityModifier = 1;
    [Range(0f, 10f  )] [SerializeField] float wallPushUpVelocityModifier;


    Rigidbody2D rb;

    bool canAirJump = false;

    float jumpTimer = 100;
    float hugWallReleaseTimer = 100;
    float initialGravityScale;

    List<PlayerInputEvent> playerEvents = new();
    PlayerState currentPlayerState;
    PlayerMovementData movementData;

    public void AddToTireness(float addAmount)
    {
        tireness = Mathf.Min(tireness + addAmount, 10);
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
        UpdateCheckPlayerState();
        UpdateHorizontalMovement();
        UpdateJumpLogic();
        UpdateHugWallLogic();

        ApplyResistance();

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

        //rb.gravityScale = GetSlideDownGravity();

        if(rb.velocity.y < 0)
        {
            rb.AddForce(Vector2.up * (-rb.velocity.y * GetStat(PlayerStat.WallSlideDrag)), ForceMode2D.Impulse);
        }

        if (movementData.IsMoving() && SignOf(GetHugWallDirection().x) != SignOf(movementData.GetMoveAxis()))
        {
            if( movementData.startMoving )
            {
                hugWallReleaseTimer = 0;
                //Debug.Log("============================");
            }

            //Debug.Log(hugWallReleaseTimer);

            if (hugWallReleaseTimer > hugWallReleaseDuration)
            {
                rb.AddForce(movementData.GetMoveDirection() * GetStat(PlayerStat.AirAcceleration));
            }
        }

        hugWallReleaseTimer += Time.deltaTime;
    }

    private void UpdateHorizontalMovement()
    {
        if(currentPlayerState == PlayerState.OnAir || currentPlayerState == PlayerState.OnGround)
        {
            float acceleration = IsOnGround() ? GetStat(PlayerStat.GroundAcceleration) : GetStat(PlayerStat.AirAcceleration);
            rb.AddForce(movementData.GetMoveDirection() * acceleration);
        }
    }

    private void ApplyResistance()
    {
        if (currentPlayerState == PlayerState.OnGround)
            rb.velocity = new Vector2(rb.velocity.x * (1 - groundDrag), rb.velocity.y);
        else
        {
            rb.velocity = new Vector2(rb.velocity.x * (1 - horizontalAirDrag), rb.velocity.y);
        }
    }

    private void UpdateJumpLogic()
    {
        if (playerEvents.Contains(PlayerInputEvent.StartJump))
        {
            if(currentPlayerState == PlayerState.OnGround)
            {
                canAirJump = true;
                jumpTimer = 0;
            }
            else if (currentPlayerState == PlayerState.HugWall)
            {
                ApplyWallJumpVelocity();
                canAirJump = true;
            }
            else if (canAirJump)
            {
                canAirJump = false;
                jumpTimer = 0;
            }
        }

        if (playerEvents.Contains(PlayerInputEvent.StopJump))
        {
            jumpTimer = 100;
        }

        bool isJumping = jumpTimer < jumpDuration;

        if (isJumping)
        {
            rb.velocity = new Vector2(
                rb.velocity.x, 
                GetStat(canAirJump ? PlayerStat.GroundJumpVelocity : PlayerStat.AirJumpVelocity));

            jumpTimer += Time.deltaTime;
        }
    }

    private void ApplyWallJumpVelocity()
    {
        rb.velocity = new Vector2(
            GetStat(PlayerStat.WallPushOutVelocity) * -GetHugWallDirection().x,
            GetStat(PlayerStat.WallPushUpVelocity));
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


    float GetStat(PlayerStat stat)
    {
        float baseStat = 0, modifier = 0;
        switch (stat)
        {
            case PlayerStat.GroundJumpVelocity:
                baseStat =  groundJumpVelocity;
                modifier =  groundJumpVelocityModifier;

                break;
            case PlayerStat.AirJumpVelocity:
                baseStat =  airJumpVelocity;
                modifier =  airJumpVelocityModifier;

                break;
            case PlayerStat.JumpForceOverTime:
                baseStat =  jumpForceOvertime;
                modifier =  jumpForceOvertimeModifier;

                break;
            case PlayerStat.GroundAcceleration:
                baseStat =  groundAcceleration;
                modifier =  groundAccelerationModifier;

                break;
            case PlayerStat.AirAcceleration:
                baseStat =  airAcceleration;
                modifier =  airAccelerationModifier;

                break;
            case PlayerStat.WallSlideDrag:
                baseStat =  wallSlideDrag;
                modifier =  wallSlideDragModifier;

                break;
            case PlayerStat.WallPushOutVelocity:
                baseStat =  wallPushOutVelocity;
                modifier =  wallPushOutVelocityModifier;

                break;
            case PlayerStat.WallPushUpVelocity:
                baseStat =  wallPushUpVelocity;
                modifier =  wallPushUpVelocityModifier;

                break;
            default:
                break;
        }

        return baseStat - Mathf.Min(baseStat, tireness * modifier);
    }

    int SignOf(float value)
    {
        return value > 0 ? 1 : value < 0 ? -1 : 0;
    }
}
