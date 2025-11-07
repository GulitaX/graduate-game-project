using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterState
{
    public enum availableState
    {
        isAttack,
        isRunning,
        isHurt,
        isDie,
        isIdle,
        onCoolDown,
        runAway
    }

}
