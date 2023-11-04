using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideOptimizer : MonoBehaviour
{
    public Observer observer;
    private bool playerIsHere;
    public int index;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsHere = true;
            observer.UpdatePlayerState(index);
        }
    }

    public void SetActive(bool value)
    {
        gameObject.SetActive(value);
    }

    void OnDrawGizmos()
    {
        BoxCollider2D bc = GetComponent<BoxCollider2D>();
        if (bc != null)
        {
            Gizmos.color = Color.blue;
            Vector2 boxSize = bc.size;
            Vector3 boxPosition = transform.position + new Vector3(bc.offset.x, bc.offset.y, 0);
            Gizmos.DrawWireCube(boxPosition, boxSize);
        }
    }
}
