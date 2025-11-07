using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "Scriptable Objects/EnemyAnim")]
public class MonsterAnimationSO : ScriptableObject
{
    public string idle;
    public string run;
    public string hurt;
    public string attack;
    public string die;

}
