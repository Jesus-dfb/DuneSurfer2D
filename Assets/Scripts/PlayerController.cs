using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float torqueAmount = 1f;
    [SerializeField] private float bootsSpeed = 35f;
    [SerializeField] private ParticleSystem snowEffect;
    [SerializeField] private ScoreManager scoreManager;

    SurfaceEffector2D[] surfaceEffectors;
    float[] surfaceDirections;
    Rigidbody2D rb;

    float baseSpeed;
    InputAction moveAction;
    Vector2 moveInput;
    float previousRotation; // Store the previous rotation of the player
    float totalRotation; // Store the total rotation of the player
    int flipCount; // Store the number of flips performed by the player

    int activePowerUpsCount; // Track the number of active power-ups

    bool isReady;

    private bool canControlPlayer = true; // Flag to control player input

    public bool CanControlPlayer { get => canControlPlayer; set => canControlPlayer = value; }

    public bool HasKey { get; private set; }

    public void CollectKey()
    {
        HasKey = true;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ActivateSelectedCharacter();

        rb = GetComponent<Rigidbody2D>();
        surfaceEffectors = FindObjectsByType<SurfaceEffector2D>();

        if (scoreManager == null)
        {
            scoreManager = FindAnyObjectByType<ScoreManager>();
        }

        if (InputSystem.actions != null)
        {
            moveAction = InputSystem.actions.FindAction("Player/Move") ?? InputSystem.actions.FindAction("Move");
        }

        if (rb == null)
        {
            Debug.LogError("PlayerController needs a Rigidbody2D on the same GameObject.");
            enabled = false;
            return;
        }

        if (surfaceEffectors.Length == 0)
        {
            Debug.LogError("SurfaceEffector2D not found in this scene. Check that the level floor has a SurfaceEffector2D component.");
            enabled = false;
            return;
        }

        if (moveAction == null)
        {
            Debug.LogError("Move action not found. Check Assets/Settings/InputSystem_Actions and the project's default input actions.");
            enabled = false;
            return;
        }

        baseSpeed = Mathf.Abs(surfaceEffectors[0].speed);
        surfaceDirections = new float[surfaceEffectors.Length];

        for (int i = 0; i < surfaceEffectors.Length; i++)
        {
            float direction = Mathf.Sign(surfaceEffectors[i].speed);
            surfaceDirections[i] = direction == 0 ? 1f : direction;
        }

        moveAction.Enable();
        isReady = true;
    }

    void ActivateSelectedCharacter()
    {
        Transform characters = transform.Find("Characters");

        if (characters == null || characters.childCount == 0)
        {
            Debug.LogError("Player needs a Characters child containing the character visuals.", this);
            return;
        }

        int selectedCharacter = PlayerPrefs.GetInt("SelectedCharacter", 0);

        selectedCharacter = Mathf.Clamp(selectedCharacter, 0, characters.childCount - 1);

        for (int i = 0; i < characters.childCount; i++)
        {
            characters.GetChild(i).gameObject.SetActive(i == selectedCharacter);
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (!canControlPlayer || !isReady) return; // If player input is disabled, exit the method

        PlayerTorque();
        BoostPlayer();
        CalculateFlips();

    }

    /// <summary>
    /// Calculates the number of flips the player has performed based on their rotation 
    /// </summary>
    private void CalculateFlips()
    {
        // Get the current rotation of the player in degrees
        float currentRotation = transform.rotation.eulerAngles.z;
        // Calculate the change in rotation since the last frame
        totalRotation += Mathf.DeltaAngle(previousRotation, currentRotation);

        if (Math.Abs(totalRotation) > 340)
        {
            flipCount++; // Increment the flip count if the total rotation exceeds 360 degrees

            if (scoreManager != null)
            {
                scoreManager.AddScore(flipCount * 100);
            }

            totalRotation = 0; // Reset the total rotation for the next flip            
        }

        previousRotation = currentRotation; // Update the previous rotation for the next frame

    }

    /// <summary>
    /// Applies torque to the player based on the horizontal input from the Move action.
    /// </summary>
    void PlayerTorque()
    {
        if (moveAction == null || rb == null) return;

        moveInput = moveAction.ReadValue<Vector2>();
        if (moveInput.x < 0)
        {
            rb.AddTorque(torqueAmount);
        }
        else if (moveInput.x > 0)
        {
            rb.AddTorque(-torqueAmount);
        }

    }

    void BoostPlayer()
    {
        if (surfaceEffectors == null || surfaceEffectors.Length == 0) return;

        float targetSpeed = moveInput.y > 0 ? bootsSpeed : baseSpeed;

        for (int i = 0; i < surfaceEffectors.Length; i++)
        {
            surfaceEffectors[i].speed = surfaceDirections[i] * targetSpeed;
        }
    }

    // Detects when the player collides with the floor and plays the snow effect
    void OnCollisionEnter2D(Collision2D collision)
    {
        int layerIndex = LayerMask.NameToLayer("Floor");
        if (collision.gameObject.layer == layerIndex)
        {
            snowEffect.Play();
        }
    }

    // Detects when the player exits the collision with the floor and stops the snow effect
    void OnCollisionExit2D(Collision2D collision)
    {
        int layerIndex = LayerMask.NameToLayer("Floor");
        if (collision.gameObject.layer == layerIndex)
        {
            snowEffect.Stop();
        }
    }

    public void ApplyPowerUp(PowerUpScriptableObject powerUpData)
    {
        activePowerUpsCount++; // Increment the count of active power-ups
        if (powerUpData.PowerUpType == "Speed")
        {
            baseSpeed += powerUpData.PowerUpValue; // Increase the base speed by the power-up value
            bootsSpeed += powerUpData.PowerUpValue; // Increase the boots speed by the power-up value
        }
    }

    public void DeactivatePowerUp(PowerUpScriptableObject powerUpData)
    {
        activePowerUpsCount = Mathf.Max(0, activePowerUpsCount - 1);
        if (powerUpData.PowerUpType == "Speed")
        {
            baseSpeed -= powerUpData.PowerUpValue;
            bootsSpeed -= powerUpData.PowerUpValue;
        }
    }
}
