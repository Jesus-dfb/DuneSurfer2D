using UnityEngine;

public class GravityPortal : MonoBehaviour
{
    [SerializeField] private bool rotatePlayer = true;

    bool hasActivated;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasActivated)
        {
            return;
        }

        Rigidbody2D playerRigidbody = other.attachedRigidbody;

        if (playerRigidbody == null || !playerRigidbody.CompareTag("Player"))
        {
            return;
        }

        playerRigidbody.gravityScale *= -1f;

        if (rotatePlayer)
        {
            Vector3 playerScale = playerRigidbody.transform.localScale;
            playerScale.y *= -1f;
            playerRigidbody.transform.localScale = playerScale;
        }

        hasActivated = true;
    }
}
