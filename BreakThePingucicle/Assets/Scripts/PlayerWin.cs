using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWin : MonoBehaviour
{
    public GameObject player;
    public Vector3 teleportPosition = new Vector3(0, 10, 0);
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object that entered is the player
        if (other.gameObject == player)
        {
            // Teleport the player to the specified position
            player.transform.position = teleportPosition;
        }
    }
}
