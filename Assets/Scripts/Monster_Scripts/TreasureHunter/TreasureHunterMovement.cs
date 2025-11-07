using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureHunterMovement : MonsterBaseMovement
{
    public MonsterAnimationSO monsterAnimationSO;
    public GameObject target;
    private float distance;
    //public float speed;

    private float facing;
    public Rigidbody2D rb;

    

    public LayerMask playerLayerMask;

    public Animator MonsterAnimator;

    public string currentAnim;

    public float MaxDistanceRadius, MinDistanceRadius;
    public override void setState(MonsterState.availableState newState)
    {
        if (newState == state)
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
        damagePlayer = false;
        target = GameObject.FindGameObjectWithTag("Player");
        MonsterAnimator = GetComponentInChildren<Animator>();
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    protected override void Update()
    {
        //This line prevent the monster movement crashing
        //when the player is dead and the game object got removed
        if (target != null)
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

        switch (state)
        {

            case MonsterState.availableState.isRunning:
                runAwayFromPlayer();

                break;

            case MonsterState.availableState.isIdle:
                PlayAnim(monsterAnimationSO.idle, null);
                rb.velocity = Vector2.zero;

                if (distance <= MaxDistanceRadius)
                {
                    state = MonsterState.availableState.isRunning;
                }

                break;

            case MonsterState.availableState.isHurt:
                rb.velocity = Vector2.zero;
                playDamageAnim();

                break;

            case MonsterState.availableState.isDie:
                rb.velocity = Vector2.zero;
                StartCoroutine(monsterDie());

                break;

        }
    }

    private void flipMonster(float facing)
    {
        if (facing < 0) this.transform.localScale = new Vector3(1, 1, 1);

        else this.transform.localScale = new Vector3(-1, 1, 1);
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
    private void runAwayFromPlayer()
    {
        void FixedUpdate()
        {
            if (target != null)
            {
                Vector3 dir = (target.transform.position - rb.transform.position).normalized;
                rb.MovePosition(rb.transform.position - dir * speed * Time.fixedDeltaTime);
            }

        }

        FixedUpdate();
        PlayAnim(monsterAnimationSO.run, null);

        if (distance > MaxDistanceRadius)
        {
            state = MonsterState.availableState.isIdle;
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(this.transform.position, MaxDistanceRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(this.transform.position, MinDistanceRadius);


    }

    IEnumerator monsterDie()
    {
        PlayAnim(monsterAnimationSO.idle, null);

        if (MonsterAnimator.GetCurrentAnimatorStateInfo(0).IsName(monsterAnimationSO.idle) &&
        MonsterAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
        {

            yield return new WaitForSecondsRealtime(0.5f);
            gameObject.SetActive(false);
            GameObject.Destroy(gameObject);
        }
    }
}
