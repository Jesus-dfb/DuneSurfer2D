using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishLine : MonoBehaviour
{
    [SerializeField] private float reloadDelay = 1f; // Delay before reloading the scene
    [SerializeField] private ParticleSystem finishEffect; // Particle effect to play when the player crosses the finish line
    [SerializeField] private int lastLevel = 3;
    bool hasFinished;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasFinished)
        {
            return;
        }

        Rigidbody2D body = other.attachedRigidbody;

        if (body == null || !body.CompareTag("Player"))
        {
            return;
        }

        if (!body.TryGetComponent(out PlayerController player))
        {
            return;
        }

        hasFinished = true;
        player.CanControlPlayer = false;

        bool requiresKey = SceneManager.GetActiveScene().name == "Level3";

        if (requiresKey && !player.HasKey)
        {
            Debug.Log("Necesitas la llave para completar este nivel.");
                Invoke(nameof(RestartLevel), reloadDelay);
            return;
        }

        Debug.Log("Player has crossed the finish line!");

        if (finishEffect != null)
        {
            finishEffect.Play();
        }

        Invoke(nameof(NextLevel), reloadDelay);
    }

    void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
