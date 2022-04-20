using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    [Header("Jumping")]

    public float jumpHeight;
    public float highTime;
    public float lowTime;
    public int overGroundJumpingFrames;
    public int afterGroundJumpingFrames;

    bool grounded;
    bool rememberGrounded;

    int overGroundJumpingTimer = 0;
    int afterGroundJumpingTimer = 0;

    [Header("Running")]
    [Space]

    public float runningSpeed;
    public float runningAccTime;
    public float runningDecTime;

    public float crouchingSpeed;

    float currentMaxSpeed;
    Vector3 runningDecVector;
    Vector3 speedVector;
    Vector3 runningAccVector;
    

    [Header("Events")]
    [Space]

    public UnityEvent OnLandEvent;
    [System.Serializable]
    public class BoolEvent : UnityEvent<bool>{}

    public BoolEvent OnCrouchEvent;
    private bool wasCrouching = false;
    private bool isCrouching = false;

    [Header("Collision")]
    [Space]

    [SerializeField] private LayerMask WhatIsGround;
    [SerializeField] private Transform GroundCheck;
    [SerializeField] private Transform CeilingCheck;
    [SerializeField] private Collider2D CrouchDisableCollider;
    Rigidbody2D rigb;

    const float GroundedRadius = 0.1f;
    const float CeilingRadius = .2f;



    /// ///


    private void Awake()
    {
        rigb = GetComponent<Rigidbody2D>();
        rigb.freezeRotation = true;

        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();

        if (OnCrouchEvent == null)
            OnCrouchEvent = new BoolEvent();
    }

    void Jump()
    {
        if ((!grounded) && (!rememberGrounded))
        {
            return;
        }
        rigb.velocity = new Vector2(0f, 2*jumpHeight/highTime);
    }

    void GravityController()
    {
        if (rigb.velocity.y <= 0)
        {
            rigb.gravityScale = (1 / 9.8f) * 2 * jumpHeight / (lowTime * lowTime);
            return;
        }
        rigb.gravityScale = (1 / 9.8f) * 2 * jumpHeight / (highTime * highTime);
    }

    void JumpingTimer()
    {
        if (overGroundJumpingTimer > 0)
        {
            overGroundJumpingTimer--;
        }

        if (afterGroundJumpingTimer > 0)
        {
            rememberGrounded = true;
            afterGroundJumpingTimer--;
        }
        else
        {
            rememberGrounded = false;
        }
    }

    void Movement()
    {
        rigb.velocity = new Vector3(0, rigb.velocity.y,0);
        if (Input.GetKey("a") == Input.GetKey("d"))
        {
            
            if (Mathf.Abs(speedVector.x) < runningDecVector.x * Time.fixedDeltaTime)
            {
                speedVector = Vector3.zero;
            }
            else if (speedVector.x < 0)
            {
                speedVector += runningDecVector * Time.fixedDeltaTime;
            }
            else if (speedVector.x > 0)
            {
                speedVector -= runningDecVector * Time.fixedDeltaTime;
            }
        }
        else if (Input.GetKey("a"))
        {
            if (speedVector.x > -currentMaxSpeed)
            {
                speedVector -= runningAccVector * Time.fixedDeltaTime;
            }
            else
            {
                speedVector = new Vector3(-currentMaxSpeed, 0, 0);
            }
        }
        else if (Input.GetKey("d"))
        {
            if (speedVector.x < currentMaxSpeed)
            {
                speedVector += runningAccVector * Time.fixedDeltaTime;
            }
            else
            {
                speedVector = new Vector3(currentMaxSpeed,0,0);
            }
        }

        transform.position += runningSpeed * speedVector * Time.fixedDeltaTime;
    }

    void GroundTouching()
    {
        bool wasGrounded = grounded;
        grounded = false;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(GroundCheck.position, GroundedRadius, WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                grounded = true;
                if (!wasGrounded)
                {
                    if (overGroundJumpingTimer > 0)
                    {
                        Jump();
                    }
                    OnLandEvent.Invoke();
                }
                    
            }
        }
        if (wasGrounded && (!grounded) && (rigb.velocity.y <= 0))
        {
            afterGroundJumpingTimer = afterGroundJumpingFrames;
        }
    }

    void Crouching(bool crouch)
    {
        if (!crouch)
            if (Physics2D.OverlapCircle(CeilingCheck.position, CeilingRadius, WhatIsGround))
            {
                crouch = true;
            }

        if (crouch)
        {
            if (!wasCrouching)
            {
                wasCrouching = true;
                OnCrouchEvent.Invoke(true);
                currentMaxSpeed = crouchingSpeed;
            }
            CrouchDisableCollider.enabled = false;
        }
        else
        {
            if (wasCrouching)
            {
                wasCrouching = false;
                OnCrouchEvent.Invoke(false);
                currentMaxSpeed = runningSpeed;
            }
            CrouchDisableCollider.enabled = true;
        }
        
    }

    void Start()
    {
        runningAccVector = new Vector3(2 / (runningAccTime * runningAccTime), 0, 0);
        runningDecVector = new Vector3(2 / (runningDecTime * runningDecTime), 0, 0);
        currentMaxSpeed = runningSpeed;
    }

    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            Jump();
            overGroundJumpingTimer = overGroundJumpingFrames;
        }
        if (Input.GetKey("s"))
        {
            isCrouching = true;
        }
        else
        {
            isCrouching = false;
        }
    }

    private void FixedUpdate()
    {
        GravityController();
        JumpingTimer();
        GroundTouching();
        Movement();
        Crouching(isCrouching);
    }

}
