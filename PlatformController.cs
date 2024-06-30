using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : MonoBehaviour
{
    private Rigidbody2D rb;
    private BoxCollider2D col;
    private Animator anim;
    private SpriteRenderer sp;
    private Vector2 inputVector, moveVector;
    private Vector3 groundCheckA, groundCheckB, ceilingCheckA, ceilingCheckB;
    private float yVel;
    public float gravity = 9.81f;
    public float jumpVel = 10f;
    public float climbVel = 9.81f;
    public float speed = 5f;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayers, enemyLayer, ceilingLayers;
    bool grounded, jumpPressed, jumping, squishEnemy, extraJump, ceilinged, climbing, crouching;
    public bool laddered, wasLaddered;

    public GameObject audioObject;

    public AudioManager am;
    float sinceLastFootstep;
    float timeBetweenFootsteps = 0.4f;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        col = GetComponent<BoxCollider2D>();
        am = FindObjectOfType<AudioManager>();
        sp = GetComponent<SpriteRenderer>();
        
        Manager.lastCheckPoint = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        CalculateScales();
        GetInputs();
        CalculateMovement();
        ControlAnimatiom();
        Crouch();
    }

    void GetInputs()
    {
        inputVector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        jumpPressed = Input.GetButtonDown("Jump");
        crouching = Input.GetKey(KeyCode.DownArrow);
        if (crouching == true)
        {
            Debug.Log(crouching);
        }
    }

    void Crouch()
    {
        if (crouching)
        {
            col.size = new Vector2 (col.size.x,0.56f);
            speed = 2f;
        }
        else
        {
            col.size = new Vector2(col.size.x, 1.28f);
            speed = 5f;
        }

    if (ceilinged && grounded)
    { 
        crouching = true;
    }
    else
    {
        crouching = false;
    }

    }
    void ControlAnimatiom()
    {
        if (!Manager.gamePaused) 
        {
            if (inputVector.x != 0f)
            {
                anim.SetBool("run", true);
                if (inputVector.x > 0f)
                {
                    sp.flipX = false;
                }
                else
                {
                    sp.flipX = true;
                }
            }
            else
            {
                anim.SetBool("run", false);
            }

            anim.SetBool("jump", jumping);
            anim.SetBool("climbing", climbing);
            anim.SetBool("crouching", crouching); 
        }
    }

    void CalculateMovement()
    {
        if (!Manager.gamePaused) 
        {
            grounded = CheckCollision(groundCheckA, groundCheckB, groundLayers);
            ceilinged = CheckCollision(ceilingCheckA, ceilingCheckB, ceilingLayers);

            if (jumpPressed)
            {
                jumpPressed = false;
                if (grounded)
                {
                    jumping = true;
                    yVel = jumpVel;
                    am.AudioTrigger(AudioManager.SoundFXCat.Jump, transform.position, 1f);
                    // Debug.Log("yVel" + yVel);
                }
                if (extraJump)
                {
                    extraJump = false;
                    jumping = true;
                    yVel = jumpVel;
                }
            }

            if (!grounded && yVel < 0f)
            {
                squishEnemy = CheckCollision(groundCheckA, groundCheckB, enemyLayer);
                if (squishEnemy)
                {
                    extraJump = true;
                    jumping = true;
                    yVel = jumpVel * 0.5f;
                }
            }
            if (grounded && yVel <= 0f || ceilinged && yVel > 0f)
            {
                if (grounded && jumping)
                    am.AudioTrigger(AudioManager.SoundFXCat.HitGround, transform.position, 1f);
                if (ceilinged && jumping)
                    am.AudioTrigger(AudioManager.SoundFXCat.HitCeiling, transform.position, 3f);

                yVel = 0f;
                jumping = false;
            }
            else
            {
                yVel -= gravity * Time.deltaTime;
            }

            if (laddered && !wasLaddered)
            {
                if (inputVector.y != 0f)
                {
                    climbing = true;
                    wasLaddered = true;
                }
            }

            if (wasLaddered && !laddered)
            {
                climbing = false;
                wasLaddered = false;
            }

            if (climbing)
            {
                yVel = climbVel * inputVector.y;
            }

            moveVector.y = yVel;
            moveVector.x = inputVector.x * speed;

            if (moveVector.y != 0f && laddered)
            {
                sinceLastFootstep = 0f;
                am.AudioTrigger(AudioManager.SoundFXCat.Ladder, transform.position, 0.4f);
            }

            sinceLastFootstep += Time.deltaTime;
            if (moveVector.x != 0f && grounded)
            {
                if(sinceLastFootstep > timeBetweenFootsteps)
                {
                    sinceLastFootstep = 0f;
                    am.AudioTrigger(AudioManager.SoundFXCat.FootSteps, transform.position, 0.5f);
                }
            }
        }
 
    }

    bool CheckCollision(Vector3 a, Vector3 b, LayerMask l)
    {
        Collider2D colA = Physics2D.OverlapCircle(transform.position - a, groundCheckRadius, l);
        Collider2D colB = Physics2D.OverlapCircle(transform.position - b, groundCheckRadius, l);
        if (colA)
        {
            if(l == enemyLayer && yVel < 0f)
            {
                colA.gameObject.GetComponent<EnemyHealthSystem>().RecieveHit(1);
            }
            return true;
        }else if (colB)
        {
            if(l == enemyLayer && yVel < 0f)
            {
                colB.gameObject.GetComponent<EnemyHealthSystem>().RecieveHit(1);
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    void CalculateScales()
    {
        groundCheckA = -col.offset - new Vector2(col.size.x / 2f - (groundCheckRadius * 2f), - col.size.y / 2f);
        groundCheckB = -col.offset - new Vector2(- col.size.x / 2f + (groundCheckRadius * 2f), - col.size.y / 2f);

        ceilingCheckA = -col.offset - new Vector2(col.size.x / 2f - (groundCheckRadius * 1.2f), col.size.y / 2.1f);
        ceilingCheckB = -col.offset - new Vector2(- col.size.x / 2f + (groundCheckRadius * 1.2f), col.size.y / 2.1f);
    }

    private void FixedUpdate()
    {
        rb.velocity = moveVector;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position - groundCheckA, groundCheckRadius);
        Gizmos.DrawWireSphere(transform.position - groundCheckB, groundCheckRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position - ceilingCheckA, groundCheckRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position - ceilingCheckB, groundCheckRadius);
    }

}
