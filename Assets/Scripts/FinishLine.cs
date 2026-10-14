using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishLine : MonoBehaviour
{
    [SerializeField] private float reloadDelay = 1f; // Delay before reloading the scene
    [SerializeField] private ParticleSystem finishEffect; // Particle effect to play when the player crosses the finish line
    [SerializeField] private int lastLevel = 4;
    bool hasFinished;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasFinished)
        {
            hasFinished = true;
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController == null)
            {
                playerController = other.GetComponentInParent<PlayerController>();
            }

            if (playerController != null)
            {
                playerController.CanControlPlayer = false;
            }

            Debug.Log("Player has crossed the finish line!");
            if (finishEffect != null)
            {
                finishEffect.Play(); // Play the finish effect particles
            }
            
            Invoke(nameof(NextLevel), reloadDelay); // Reload the scene after the specified delay

        }
    }

    void NextLevel()
    {
        if (!TryGetCurrentLevelNumber(out int currentLevel))
        {
            SceneManager.LoadScene("Menu");
            return;
        }

        int finalLevel = Mathf.Max(1, lastLevel);
        int nextLevel = currentLevel + 1;
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
        int highestUnlockedLevel = Mathf.Clamp(Mathf.Max(unlockedLevel, nextLevel), 1, finalLevel);

        PlayerPrefs.SetInt("UnlockedLevel", highestUnlockedLevel); // Unlock the next level without passing the final level
        PlayerPrefs.Save();

        if (currentLevel >= finalLevel) // If the player has completed the last level, go back to the main menu
        {
            SceneManager.LoadScene("Menu"); //TODO: Go to Win Scene
        }
        else
        {
            SceneManager.LoadScene($"Level{nextLevel}"); // Load the next level
        }
        
    }

    bool TryGetCurrentLevelNumber(out int levelNumber)
    {
        string sceneName = SceneManager.GetActiveScene().name;
        string levelPrefix = "Level";

        if (sceneName.StartsWith(levelPrefix))
        {
            return int.TryParse(sceneName.Substring(levelPrefix.Length), out levelNumber);
        }

        levelNumber = 0;
        return false;
    }

}
