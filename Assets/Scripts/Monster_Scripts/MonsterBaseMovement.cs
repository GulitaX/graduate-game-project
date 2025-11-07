using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MonsterBaseMovement : MonoBehaviour
{
    public bool damagePlayer;

    public float speed;
    public float damage;
    public float attackRange;
    
    protected float nextAttackTime;
    public float attackDelay;

    [SerializeField]
    protected MonsterState monsterState;
    protected MonsterState.availableState state;

    public virtual void setState(MonsterState.availableState newState)
    {

    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {

    }
}
