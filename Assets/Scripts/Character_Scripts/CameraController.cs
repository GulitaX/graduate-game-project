using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    private Vector3 offset;

    void Start()
    {

        StartCoroutine(waitForPlayerToSpawn());
        transform.position = new Vector3(player.transform.localPosition.x, player.transform.localPosition.y, -10);

        //Private variable to store the offset distance between the player and camera
        //Calculate offset value by getting the distance between the player's position and camera's position.
        offset = transform.position - player.transform.position;
    }

    // LateUpdate() is called after Update each frame
    void LateUpdate()
    {
        if(player == null)
        {
            return;
        }
        // Set the position of the camera's transform to be the same as the player's, but offset by the calculated offset distance.
        //The Lerp() function from Vector class allow for simple follow animation
        //with curve line interpolation rather than normal linear line
        transform.position = Vector3.Lerp(transform.position, player.transform.position + offset, Time.deltaTime * 3f);
    }
    // Use this to check whether the game object with tagged "Player" is in the scene, if not then wait,
    // so script won't crash in the event that the player is dead and the game object got remove.
    public bool isPlayerNull()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    IEnumerator waitForPlayerToSpawn()
    {
        yield return new WaitWhile(isPlayerNull);
    }
}
