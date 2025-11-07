using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterMovement : MonsterBaseMovement
{
    public MonsterAnimationSO monsterAnimationSO;
    public GameObject target;
    private float distance;
    
    private float facing;
    public Rigidbody2D rb;

    public LayerMask playerLayerMask;
    public GameObject attackPoint;
    public Animator MonsterAnimator;

    public string currentAnim;

    public float MaxDistanceRadius, MinDistanceRadius;

    public override void setState(MonsterState.availableState newState)
    {
        if(newState == state)
        {
            return;
        }
        else
        {
            state = newState;
        }

    }

    private void Awake()
    {
        
        state = MonsterState.availableState.isIdle;
    }

    protected override void Start()
    {
        scaleStatsOnDifficulty();
        damagePlayer = true;
        target = GameObject.FindGameObjectWithTag("Player");
        MonsterAnimator = GetComponentInChildren<Animator>();
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    protected override void Update()
    {

        //This line prevent the monster movement crashing
        //when the player is dead and the game object got removed
        if(target != null)
        {
            distance = Vector3.Distance(target.transform.position, this.transform.position);
            facing = target.transform.position.x - this.transform.position.x;
        }
        else
        {
            distance = MaxDistanceRadius + 2f;
            facing = 0;
        }
        

        flipMonster(facing);

        switch(state)
        {
            
            case MonsterState.availableState.isRunning:
                chasePlayer();

                break;

            case MonsterState.availableState.isAttack:
                rb.velocity = Vector2.zero;
                PlayAnim(monsterAnimationSO.attack, null);
                if (MonsterAnimator.GetCurrentAnimatorStateInfo(0).IsName(monsterAnimationSO.attack) &&
                    MonsterAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
                {
                    //print(Time.time);
                    nextAttackTime = Time.time + attackDelay;
                    //print(nextAttackTime);

                    state = MonsterState.availableState.onCoolDown;
                }

                break;

            case MonsterState.availableState.isIdle:
                PlayAnim(monsterAnimationSO.idle, null);
                rb.velocity = Vector2.zero;

                if (distance <= MaxDistanceRadius && distance > MinDistanceRadius)
                {
                    state = MonsterState.availableState.isRunning;
                }
                else if (distance < MinDistanceRadius)
                {
                    state = MonsterState.availableState.isAttack;
                }
                
                break;

            case MonsterState.availableState.isHurt:
                rb.velocity = Vector2.zero;
                playDamageAnim();

                break;

            case MonsterState.availableState.isDie:
                rb.velocity = Vector2.zero;
                StartCoroutine( monsterDie());

                break;

            case MonsterState.availableState.onCoolDown:
                //print("jump to cooldown");

                rb.velocity = Vector2.zero;
                PlayAnim(monsterAnimationSO.idle, null);
                if (Time.time > nextAttackTime)
                {
                    //print("back to idling");
                    state = MonsterState.availableState.isIdle;
                }

                break;

        }    

    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(this.transform.position, MaxDistanceRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(this.transform.position, MinDistanceRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(attackPoint.transform.position, attackRange);

    }

    private void flipMonster(float facing)
    {
        if(facing < 0) this.transform.localScale = new Vector3(-1, 1, 1);

        else this.transform.localScale = new Vector3(1, 1, 1);
    }


    public void PlayAnim(string newAnim, System.Action onCompleteAnim)
    {
        if (currentAnim == newAnim) return;
        
        MonsterAnimator.Play(newAnim);
        currentAnim = newAnim;

        if (onCompleteAnim != null) 
        {
            onCompleteAnim();
        }


    }
    private void chasePlayer()
    {
        void FixedUpdate()
        {
            if(target != null)
            {
                Vector3 dir = (target.transform.position - rb.transform.position).normalized;
                rb.MovePosition(rb.transform.position + dir * speed * Time.fixedDeltaTime);
            }
           
        }

        FixedUpdate();
        PlayAnim(monsterAnimationSO.run, null);

        if(distance <= MinDistanceRadius)
        {
            state = MonsterState.availableState.isAttack;
        }
        else if(distance > MaxDistanceRadius)
        {
            state = MonsterState.availableState.isIdle;
        }
    }

    public void attackPlayer()
    {

        Collider2D[] playerCollider = Physics2D.OverlapCircleAll(attackPoint.transform.position, attackRange, playerLayerMask);

        foreach(Collider2D collider in playerCollider)
        {
            if (collider.gameObject.CompareTag("Player"))
            {
                GameObject player = collider.gameObject;
                player.GetComponent<PlayerMovement>().losedHealth = (int)damage;

                Debug.Log("health of " + collider.gameObject.name + " is "
                    + collider.GetComponent<PlayerMovement>().currHealth);

                break;
            }
        }


    }

    public void playDamageAnim()
    {
        PlayAnim(monsterAnimationSO.hurt, null);

        if (MonsterAnimator.GetCurrentAnimatorStateInfo(0).IsName(monsterAnimationSO.hurt) &&
        MonsterAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
        {
            state = MonsterState.availableState.isIdle;
        }

    }

    IEnumerator monsterDie()
    {
        PlayAnim(monsterAnimationSO.die, null);

        if (MonsterAnimator.GetCurrentAnimatorStateInfo(0).IsName(monsterAnimationSO.die) &&
        MonsterAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
        {
           
            yield return new WaitForSecondsRealtime(0.5f);
            gameObject.SetActive(false);
            GameObject.Destroy(gameObject);
        }
    }

    void scaleStatsOnDifficulty()
    {
        switch (GameState.difficulty)
        {
            case (GameState.difficulties.Easy):
                damage -= (int) (damage * 0.3f);
                speed -= (int) (speed * 0.4f);
                break;

            case (GameState.difficulties.Medium):
                damage += (int) (damage * 0.2f);
                break;

            case (GameState.difficulties.Hard):
                damage += (int) (damage * 0.1f);
                speed += (int) (speed * 0.3f);
                break;
        }
    }

}
