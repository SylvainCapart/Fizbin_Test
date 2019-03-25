using System;
using UnityEngine;

/// <summary>
/// Character controller class. Required components are a rigidbody, animator and collider. Allows to move the player through the mean of the move() method.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(Collider2D))]
public class Character2DController : MonoBehaviour
{
    /// <summary>
    /// PlayerState : Player state state machine enumeration. The enumeration is used to trigger the right animation through the setter of the player state.
    /// TODO : examine if the JUMP and FALL states are really relevant in this case, as it should be possible to move or not while jumping.
    /// </summary>
    enum PlayerState { IDLE, WALK, RUN, JUMP, FALL };

    /// <summary>
    /// m_PlayerRigidbody : Rigidboy of the player, here, dynamic because we want to apply forces on it and use gravity.
    /// </summary>
    private Rigidbody2D m_PlayerRigidbody;

    /// <summary>
    /// m_PlayerAnimator : Player animator.
    /// </summary>
    private Animator m_PlayerAnimator;

    /// <summary>
    /// Player Collider, in shape of a simple capsule here.
    /// </summary>
    private Collider2D m_PlayerCollider;

    /// <summary>
    /// m_RefPlayerVelocity : reference to the player velocity, used to smooth the speed of the player.
    /// </summary>
    private Vector2 m_RefPlayerVelocity;

    /// <summary>
    /// m_PlayerMovemementSmoothing : The player movemement smoothing.
    /// </summary>
    [SerializeField] [Range(0f, 1f)] private float m_PlayerMovemementSmoothing = 0.1f;

    /// <summary>
    /// m_PlayerWalkSpeed :  player walk speed.
    /// </summary>
    [SerializeField] private float m_PlayerWalkSpeed;

    /// <summary>
    /// m_PlayerRunSpeed : The player run speed.
    /// </summary>
    [SerializeField] private float m_PlayerRunSpeed;

    /// <summary>
    /// m_PlayerJumpForce : player jump force.
    /// </summary>
    [SerializeField] private float m_PlayerJumpForce;

    /// <summary>
    /// m_Grounded : if the player touches the ground or not
    /// </summary>
    [SerializeField] private bool m_Grounded = false;

    /// <summary>
    /// m_InAir : if the player is in air or not
    /// </summary>
    [SerializeField] private bool m_InAir = false;


    /// <summary>
    /// m_WhatIsGround : layer mask used to detect the ground
    /// </summary>
    [SerializeField] private LayerMask m_WhatIsGround;

    /// <summary>
    /// m_PlayerGroundDetectionPoint : point at the feet of the player, used to detect the ground
    /// </summary>
    [SerializeField] private Transform m_PlayerGroundDetectionPoint;

    /// <summary>
    /// m_GroundDetectionRadius : radius used for detecting the ground around the point of detection
    /// </summary>
    [SerializeField] [Range(0f, 0.2f)] private float m_GroundDetectionRadius = 0.2f;

    /// <summary>
    /// m_PlayerState : state of the player regarding its movement. See enum above.
    /// </summary>
    [SerializeField] private PlayerState m_PlayerState;

    /// <summary>
    /// m_FacingRight : Is the player facing right or not.
    /// </summary>
    private bool m_FacingRight = true;

    private const float EPSILON = 0.01f;

    /// <summary>
    /// Gets or sets the player state. For the moment, the setter is just triggering the states of the animator.
    /// </summary>
    /// <value>The player state property.</value>
    private PlayerState PlayerStateProperty
    {
        get { return m_PlayerState; }
        set
        {
            // if the state is already set, return
            if (m_PlayerState == value) return;
            switch (value)
            {
                case PlayerState.IDLE:
                    m_PlayerAnimator.SetBool("Walk", false);
                    m_PlayerAnimator.SetBool("Run", false);
                    m_PlayerAnimator.SetBool("Jump", false);
                    m_PlayerAnimator.SetBool("Fall", false);
                    break;
                case PlayerState.WALK:
                    m_PlayerAnimator.SetBool("Walk", true);
                    m_PlayerAnimator.SetBool("Jump", false);
                    m_PlayerAnimator.SetBool("Fall", false);
                    break;
                case PlayerState.RUN:
                    m_PlayerAnimator.SetBool("Run", true);
                    m_PlayerAnimator.SetBool("Jump", false);
                    m_PlayerAnimator.SetBool("Fall", false);
                    break;
                case PlayerState.JUMP:
                    m_PlayerAnimator.SetBool("Jump", true);
                    m_PlayerAnimator.SetBool("Fall", false);
                    break;
                    // The fall state has for the moment no apex calculation, and is not rendering correctly.
                    // TODO : Render correctly the state FALL
                    // TODO : include apex calculation
                case PlayerState.FALL:
                    m_PlayerAnimator.SetBool("Fall", true);
                    m_PlayerAnimator.SetBool("Jump", false);
                    break;
                default:
                    // Only here if the case a new state is created and was forgotten in the setter.
                    throw new NotImplementedException();
            }
            m_PlayerState = value;
        }
    }

    private void Start()
    {
        // GetComponent in Start method as they are costy
        m_PlayerRigidbody = GetComponent<Rigidbody2D>();
        m_PlayerAnimator = GetComponent<Animator>();
        m_PlayerCollider = GetComponent<CapsuleCollider2D>();
    }

    /// <summary>
    /// Ground detection is done in Fixed Update as we want the ground detection to be framerate independant
    /// </summary>
    private void FixedUpdate()
    {

        m_Grounded = false;


        Collider2D[] groundColliders;
        Vector2 groundDetectionPoint = new Vector2(m_PlayerGroundDetectionPoint.position.x, m_PlayerGroundDetectionPoint.position.y);

        // detecting all the colliders from the mask around the detecting point
        groundColliders = Physics2D.OverlapCircleAll(groundDetectionPoint, m_GroundDetectionRadius, m_WhatIsGround);

        for (int i = 0; i < groundColliders.Length; i++)
        {

            if (groundColliders[i] != null)
            {
                // if the detetced colliders are not null and not the player
                if (groundColliders[i].gameObject != gameObject)
                {
                    m_Grounded = true;
                }
            }
        }

        if(!m_Grounded)
        {
            // if the player is not grounded and not in air, it is considered in air
            if (!m_InAir)
                m_InAir = true;
            PlayerStateProperty = PlayerState.FALL;
        }

        m_PlayerAnimator.SetBool("Grounded", m_Grounded);

        // if the player is grounded, it cannot be in air
        if (m_Grounded && m_InAir)
        {
            m_InAir = false;
        }
    }

    /// <summary>
    /// Move the specified _PlayerHorizontalMove, _PlayerVerticalMove, _PlayerJump and _PlayerRun.
    /// </summary>
    /// <param name="_PlayerHorizontalMove">Player horizontal move.</param>
    /// <param name="_PlayerVerticalMove">Player vertical move.</param>
    /// <param name="_PlayerJump">If set to true if player jumps.</param>
    /// <param name="_PlayerRun">If set to true if player runs.</param>
    public void Move(float _PlayerHorizontalMove, float _PlayerVerticalMove, bool _PlayerJump, bool _PlayerRun)
    {
        Vector2 targetSpeed;

        // player grounded ...
        if (m_Grounded)
        {
            // ... and moving
            if (Math.Abs(_PlayerHorizontalMove) > EPSILON)
            {
                // ... and walking
                if (!_PlayerRun)
                {
                    targetSpeed = new Vector2(_PlayerHorizontalMove * m_PlayerWalkSpeed, _PlayerVerticalMove * m_PlayerWalkSpeed);
                    PlayerStateProperty = PlayerState.WALK;

                }
                /// ... or runninng
                else
                {
                    targetSpeed = new Vector2(_PlayerHorizontalMove * m_PlayerRunSpeed, _PlayerVerticalMove * m_PlayerRunSpeed);
                    PlayerStateProperty = PlayerState.RUN;
                }
                // change the rb velocity
                m_PlayerRigidbody.velocity = Vector2.SmoothDamp(m_PlayerRigidbody.velocity, targetSpeed * Time.fixedDeltaTime, ref m_RefPlayerVelocity, m_PlayerMovemementSmoothing);
            }
            else // no movement detected
            {
                PlayerStateProperty = PlayerState.IDLE;
            }

            // jump only triggered if he player is grounded
            if (_PlayerJump)
            {
                PlayerStateProperty = PlayerState.JUMP;
                m_PlayerRigidbody.AddForce(new Vector2(0f, m_PlayerJumpForce));
            }
        }
        else // player not grounded should fall
        {
            PlayerStateProperty = PlayerState.FALL;
            // TODO : debug the FALL state and include apex calculation
        }

        // if the player is not facing the right direction, flip it
        if (m_FacingRight && _PlayerHorizontalMove < 0f)
        {
            FlipScale();
        }
        else if (!m_FacingRight && _PlayerHorizontalMove > 0f)
        {
            FlipScale();
        }

    }

    private void FlipScale()
    {
        m_FacingRight = !m_FacingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

}
