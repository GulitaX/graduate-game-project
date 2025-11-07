using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class InteractablePoint : MonoBehaviour
{
    public bool isInRange;
    public KeyCode interactKey;
    public UnityEvent onInteract;

    private void Start()
    {
        isInRange = false;

    }
    private void Update()
    {
        if(isInRange)
        {
            if (Input.GetKeyDown(interactKey))
            {
                onInteract.Invoke();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isInRange = true;
            Debug.Log("Player now in range");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isInRange = false;
            Debug.Log("Player now outside range");
        }
    }

}

    