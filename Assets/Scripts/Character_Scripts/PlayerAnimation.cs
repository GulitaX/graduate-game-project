using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    public Animator PlayerAnimator;

    public string currentState;
    public static string idle = "Player_Idle";
    public static string run = "Player_Run";
    public static string hurt = "Player_Hurt";
    public static string die = "Player_Die";
    public static string attack1 = "Player_Attack1";
    public static string attack2 = "Player_Attack2";
    public static string attack3 = "Player_Attack3";


    void Start()
    {
        //currentState = idle;
        PlayerAnimator = GetComponentInChildren<Animator>();
    }

    public void NextAttack()
    {
        for(int i = 0; i < PlayerVariables.isAttack.Length -1; i++)
        {
            if(!PlayerVariables.isHurting)
            {
                if (PlayerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Player_Attack" + (i + 1).ToString()) &&
                PlayerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
                {
                    PlayerVariables.isAttack[i] = false;

                    if (PlayerVariables.isAttack[i + 1])
                    {
                        PlayAnim("Player_Attack" + (i + 2).ToString());
                    }
                }
            }
           
        }
        
    }
    void FixedUpdate()
    {
       
        if (PlayerVariables.isDie) PlayAnim(die);

        else
        {
            NextAttack();

            if (PlayerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Player_Attack3") &&
                    PlayerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
            {
                PlayerVariables.isAttack[2] = false;
            }

            // stand still, do nothing
            if (
                !PlayerVariables.isRunning &&
                !PlayerVariables.isAttack[0] &&
                !PlayerVariables.isAttack[1] &&
                !PlayerVariables.isAttack[2] &&
                !PlayerVariables.isHurting
                )
            {
                PlayAnim(idle);
            }
            // running and not attack
            else if (PlayerVariables.isRunning &&
                !PlayerVariables.isAttack[0] &&
                !PlayerVariables.isAttack[1] &&
                !PlayerVariables.isAttack[2] &&
                !PlayerVariables.isHurting
                )
            {
                PlayAnim(run);
            }
            else if (PlayerVariables.isHurting && !PlayerVariables.isDie)
            {
                PlayAnim(hurt);

                //disable all attack so the animation won't crashed if player spamming attack button
                PlayerVariables.isAttack[0] = false;
                PlayerVariables.isAttack[1] = false;
                PlayerVariables.isAttack[2] = false;

                if (PlayerAnimator.GetCurrentAnimatorStateInfo(0).IsName(hurt) &&
                   PlayerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
                {
                    PlayerVariables.isHurting = false;

                }

            }
            //Start attacking with 1st combo
            else if (PlayerVariables.isAttack[0]
                && !PlayerVariables.isRunning
                && !PlayerVariables.isAttack[1]
                && !PlayerVariables.isAttack[2]
                && !PlayerVariables.isHurting)
            {
                PlayAnim(attack1);
            }
        }

    }

    public void PlayAnim(string newState)
    {

        // check if newState is the same, if true, ignore the newState
        if (currentState == newState) return;

        PlayerAnimator.Play(newState);
        currentState = newState;

    }

}
