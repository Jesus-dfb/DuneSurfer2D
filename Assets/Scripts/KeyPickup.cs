using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        Rigidbody2D body = other.attachedRigidbody;

        if (body == null || !body.CompareTag("Player"))
        {
            return;
        }

        if (!body.TryGetComponent(out PlayerController player))
        {
            return;
        }

        if (!player.CanControlPlayer)
        {
            return;
        }

        player.CollectKey();
        gameObject.SetActive(false);
    }
}