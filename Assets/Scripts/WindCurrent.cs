using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class WindCurrent : MonoBehaviour
{
    [SerializeField] private Vector2 windDirection = Vector2.up;
    [SerializeField, Min(0f)] private float windForce = 30f;

    readonly HashSet<Collider2D> playerColliders = new HashSet<Collider2D>();
    Rigidbody2D playerRigidbody;
    PlayerController playerController;

    void Reset()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        Rigidbody2D body = other.attachedRigidbody;

        if (body == null || !body.CompareTag("Player"))
        {
            return;
        }

        if (!body.TryGetComponent(out PlayerController controller))
        {
            return;
        }

        playerRigidbody = body;
        playerController = controller;
        playerColliders.Add(other);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        playerColliders.Remove(other);
    }

    void FixedUpdate()
    {
        playerColliders.RemoveWhere(collider =>
            collider == null || !collider.enabled || !collider.gameObject.activeInHierarchy);

        if (playerColliders.Count == 0 || playerRigidbody == null ||
            playerController == null || !playerController.CanControlPlayer ||
            !GetComponent<BoxCollider2D>().enabled)
        {
            return;
        }

        // Apply once per physics step, even when both head and body overlap.
        playerRigidbody.AddForce(windDirection.normalized * windForce, ForceMode2D.Force);
    }

    void OnDisable()
    {
        playerColliders.Clear();
        playerRigidbody = null;
        playerController = null;
    }
}
