using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed;

    public float maxHealth;
    public float currHealth;
    public float losedHealth;
    public GameObject healthBar;

    public float maxDamage;
    private float currDamage;
    public float attackRange;
    public GameObject attackPoint;
    public LayerMask enemyLayerMask;

    public Rigidbody2D rb;
    public Vector2 moveDirection;

    float moveX = 0f;
    float moveY = 0f;
    
    public bool canAttack;

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        currHealth = maxHealth;
        losedHealth = 0f;

        PlayerVariables.isHurting = false;
        PlayerVariables.isDie = false;
        canAttack = true;

    }

    void Update()
    {
        if(!MenuScripts.isPaused)
        {
            ProcessInput();
        }
    }

    private void FixedUpdate()
    {
        updateHealthUI();
        Move();
    }
    void ProcessInput()
    {
        isLosingHealth();

        if (!PlayerVariables.isHurting || !PlayerVariables.isDie)
        {
            if (Input.GetKeyDown(KeyCode.F) && canAttack && (
                !PlayerVariables.isAttack[0] ||
                !PlayerVariables.isAttack[1] ||
                !PlayerVariables.isAttack[2]))
            {
                PlayerVariables.isRunning = false;
                moveDirection = Vector2.zero;
                AttackState();
            }
            else if (!PlayerVariables.isAttack[0] &&
                !PlayerVariables.isDie &&
                (Input.GetKey(KeyCode.A) ||
                Input.GetKey(KeyCode.S) ||
                Input.GetKey(KeyCode.D) ||
                Input.GetKey(KeyCode.W)) )
            {
                PlayerVariables.isRunning = true;
                moveX = Input.GetAxisRaw("Horizontal");
                moveY = Input.GetAxisRaw("Vertical");

                if (moveX < 0 && PlayerVariables.isRunning)
                {
                    transform.localScale = new Vector3(-1, 1, 1);
                }
                else if (moveX > 0 && PlayerVariables.isRunning)
                {
                    transform.localScale = new Vector3(1, 1, 1);
                }

                moveDirection = new Vector2(moveX, moveY).normalized;
            }
            else
            {
                PlayerVariables.isRunning = false;
                moveDirection = Vector2.zero;

            }
        }

    }

    void isLosingHealth(){

        if (PlayerVariables.isHurting) canAttack = false;
        else canAttack = true;

        if(losedHealth > 0)
        {
            PlayerVariables.isHurting = true;
            currHealth -= losedHealth;
            losedHealth = 0f;
            
        }

        else if (currHealth <= 0)
        {
            moveSpeed = 0f;
            PlayerVariables.isDie = true;
            StartCoroutine(deletePlayer());
        }
    }

    private void updateHealthUI()
    {
        if(currHealth >= 0)
        {
            healthBar.transform.Find("Health Fill").GetComponent<Image>().fillAmount = currHealth / maxHealth;
            healthBar.transform.Find("Health Text").GetComponent<TMP_Text>().text = currHealth + " / " + maxHealth;
        }
        else if(currHealth < 0)
        {
            healthBar.transform.Find("Health Fill").GetComponent<Image>().fillAmount = 0f;
            healthBar.transform.Find("Health Text").GetComponent<TMP_Text>().text = 0 + " / " + maxHealth;
        }
    }

    private void Move()
    {
        rb.velocity = new Vector2(moveDirection.x * moveSpeed , moveDirection.y * moveSpeed);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(attackPoint.transform.position, attackRange);
    }

    public void AttackState()
    {
        if (!PlayerVariables.isHurting &&
            !PlayerVariables.isAttack[0] &&
            !PlayerVariables.isAttack[1] &&
            !PlayerVariables.isAttack[2])
        {
            PlayerVariables.isAttack[0] = true;
            currDamage = maxDamage * 0.3f;
            moveDirection = Vector2.zero;
        }
        else if (PlayerVariables.isAttack[0] && 
            !PlayerVariables.isRunning && 
            !PlayerVariables.isHurting && 
            Input.GetKeyDown(KeyCode.F))
        {
            PlayerVariables.isAttack[1] = true;
            currDamage = maxDamage * 0.5f;
            moveDirection = Vector2.zero;

        }
        else if (PlayerVariables.isAttack[1] && 
            !PlayerVariables.isRunning && 
            !PlayerVariables.isHurting && 
            Input.GetKeyDown(KeyCode.F))
        {
            PlayerVariables.isAttack[2] = true;
            currDamage = maxDamage;
            moveDirection = Vector2.zero;
        }
    }

    public void Attacking()
    {
        Collider2D[] enemiesCollider = Physics2D.OverlapCircleAll(attackPoint.transform.position, attackRange, enemyLayerMask);

        foreach(Collider2D enemy in enemiesCollider)
        {
            if(enemy.gameObject.CompareTag("Enemy Hitbox"))
            {
                
                enemy.GetComponent<MonsterHealth>().losedHealth = (int)currDamage;
                Debug.Log("health of " + enemy.gameObject.name + 
                    " is " + enemy.GetComponent<MonsterHealth>().currHealth);
            }


        }
    }
    IEnumerator deletePlayer()
    {
        yield return new WaitForSecondsRealtime(2f);
        gameObject.SetActive(false);
        GameObject.Destroy(gameObject);
        MenuScripts Menu = GameObject.FindFirstObjectByType<MenuScripts>();

        Menu.GameOver(false);
    }
    
}
