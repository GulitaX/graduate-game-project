using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonsterHealth : MonoBehaviour
{
    public float maxHealth;
    public float currHealth;
    public float losedHealth;
    public MonsterBaseMovement monsterMovement;

    [SerializeField]
    private bool canDamage;
    public GameObject healthBar;
    public float pointOnKill;
    private bool hasDead;    

    private void Start()
    {
        scaleHealthOnDifficulty();
        hasDead = false;
        losedHealth = 0f;
        canDamage = true;
        currHealth = maxHealth;
    }

    private void Update()
    {
        if(currHealth <= 0)
        {
            if(!hasDead)
            {
                PlayScore score = GameObject.FindFirstObjectByType<PlayScore>();
                score.UpdateGameScore(pointOnKill);
                hasDead = true;
                Debug.Log("Enemy Dead");
            }
           
            canDamage = false;
            monsterMovement.setState(MonsterState.availableState.isDie);

        }
        if (losedHealth > 0 && canDamage)
        {
            canDamage = false;
            
            StartCoroutine(takeDamage());
            currHealth -= losedHealth;
            losedHealth = 0f;
        }
    }

    private void FixedUpdate()
    {
        if (currHealth >= 0)
        {
            healthBar.transform.Find("Health Fill").GetComponent<Image>().fillAmount = currHealth / maxHealth;
        }
        else if (currHealth < 0)
        {
            healthBar.transform.Find("Health Fill").GetComponent<Image>().fillAmount = 0f;
        }
       
    }

    IEnumerator takeDamage()
    {
        monsterMovement.setState(MonsterState.availableState.isHurt);

        yield return new WaitForSecondsRealtime(0.5f);
        canDamage = true;
    }

    public void scaleHealthOnDifficulty()
    {
        switch (GameState.difficulty)
        {
            case (GameState.difficulties.Easy):
                maxHealth -= maxHealth * 0.4f;
                break;
            case (GameState.difficulties.Medium):
                maxHealth -= maxHealth * 0.2f;
                break;
            case (GameState.difficulties.Hard):
                maxHealth += maxHealth * 0.2f;
                break;
        }
    }
}
