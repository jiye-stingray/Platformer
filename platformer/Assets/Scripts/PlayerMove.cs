using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public GameManager gameManager;
    public float maxSpeed;
    public float jumpPower;
    Rigidbody2D rigid;
    SpriteRenderer spriteRender;

    [SerializeField]
    CapsuleCollider2D capsuleCollider;

    Animator anim;

    [Header("Audio")]
    public AudioClip audioJump;
    public AudioClip audioAttack;
    public AudioClip audioDamage;
    public AudioClip audioItem;
    public AudioClip audioDie;
    public AudioClip audioFinish;
    AudioSource audioSource;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRender = GetComponent<SpriteRenderer>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
   
    }

    public void PlaySound(string action)
    {
        switch (action)
        {
            case "JUMP":
                audioSource.clip = audioJump; break;
            case "ATTACK":
                audioSource.clip = audioAttack; break;
            case "DAMAGED":
                audioSource.clip = audioDamage; break;
            case "ITEM":
                audioSource.clip = audioItem; break;
            case "DIE":
                audioSource.clip = audioDie; break;
            case "FINISH":
                audioSource.clip = audioFinish; break;


            default:
                break;
        }
        audioSource.Play();
    }

    void Update()
    {
        //Jump
        if (Input.GetButtonDown("Jump") && !anim.GetBool("isJumping"))
        {
            rigid.AddForce(transform.up * jumpPower, ForceMode2D.Impulse);
            anim.SetBool("isJumping", true);
            PlaySound("JUMP");
        }

        //Stop Speed
        if (Input.GetButtonUp("Horizontal"))
        {
            rigid.velocity = new Vector2(rigid.velocity.normalized.x * 0.5f , rigid.velocity.y);
        }

        //Direction Sprite
        if (Input.GetButton("Horizontal"))
        {
            spriteRender.flipX = Input.GetAxisRaw("Horizontal") == -1;
        }

        //Animation
        if (Mathf.Abs(rigid.velocity.x) < 0.3)
        {
            anim.SetBool("isWalking", false);
        }
        else
        {
            anim.SetBool("isWalking", true);
        }
    }

    void FixedUpdate()
    {
        //Move Speed
        float h = Input.GetAxisRaw("Horizontal");

        rigid.AddForce(Vector2.right * h * rigid.gravityScale, ForceMode2D.Impulse);
    

        //Max Speed
        if (rigid.velocity.x > maxSpeed) //Rigit maxSpeed
        {
            rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y);
        }
        else if (rigid.velocity.x < maxSpeed * -1)//Left maxSpeed
        {
            rigid.velocity = new Vector2(maxSpeed * -1, rigid.velocity.y);
        }

        //Landing Platform
        if (rigid.velocity.y < 0)
        {
            Debug.DrawRay(rigid.position, Vector3.down, new Color(0, 1, 0));
            RaycastHit2D rayhit = Physics2D.Raycast(rigid.position, Vector3.down,1, LayerMask.GetMask("Platform"));
            if (rayhit.collider != null)
            {
                if (rayhit.distance < 0.5f)
                {
                    anim.SetBool("isJumping", false);
                }

            }
        }
       
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy" || collision.gameObject.tag == "Spike")
        {
            Debug.Log("충돌");
            bool isAttack = rigid.velocity.y < 0 && transform.position.y > collision.transform.position.y;
            Debug.Log(isAttack);
            if ( isAttack && collision.gameObject.tag == "Enemy")
            {
                Debug.Log("공격 인식");
                OnAttack(collision.transform);
                Debug.Log("공격 후");
            }
            else if(!isAttack || collision.gameObject.tag == "Spike")
            {
                Debug.Log(collision.collider);
                Debug.Log("피격");
                OnDamaged(collision.transform.position);
                Debug.Log("피격 후");
            }
                
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Item")
        {
            PlaySound("ITEM");
            //Point
            bool isBronze = collision.gameObject.name.Contains("Bronze");
            bool isSliver = collision.gameObject.name.Contains("Silver");
            bool isGold = collision.gameObject.name.Contains("Gold");

            if (isBronze)
                gameManager.stagePoint += 50;
            else if (isSliver)
                gameManager.stagePoint += 100;
            else if (isGold)
                gameManager.stagePoint += 300;

            //Deactive Item
            collision.gameObject.SetActive(false);
        }
        else if (collision.gameObject.tag == "Finish")
        {
            PlaySound("FINISH");
            //Next Stage
            gameManager.NextStage();
        }
    }
    void OnAttack(Transform enemy)
    {
        PlaySound("ATTACK");
        //point
        gameManager.stagePoint += 100;

        //Reaction Force
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);

        //EnemyDie
        EnemyMove enemyMove = enemy.GetComponent<EnemyMove>();
        enemyMove.OnDamaged();
    }

    void OnDamaged(Vector2 targetPos)
    {
    
        //Health Down
        gameManager.HealthDown();

        //Change Layer
        gameObject.layer = 9;

        //View Alpha
        spriteRender.color = new Color(1, 1, 1, 0.4f);

        //Rection Force
        int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1;
        rigid.AddForce(new Vector2(dirc, 1) * 10, ForceMode2D.Impulse);

        //Animation
        anim.SetTrigger("doDamaged");
        Invoke("OffDamaged", 3);

    }    

    void OffDamaged()
    {
        gameObject.layer = 8;

        spriteRender.color = new Color(1, 1, 1,1);
    }

   
    public void OnDie()
    {
        PlaySound("DIE");
        //Sprite Alpha
        spriteRender.color = new Color(1, 1, 1, 0.4f);

        //Sprite Flip Y
        spriteRender.flipY = true;

        //Collider Disable
        capsuleCollider.enabled = false;

        //Die Effect Jump
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
    }

    public void VelocityZero()
    {
        rigid.velocity = Vector2.zero;
    }
}
