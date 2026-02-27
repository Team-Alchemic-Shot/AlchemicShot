using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public static bool IsAnyUIOpen = false;

    [Header("UI References")]
    public GameObject pauseMenuUI;
    public GameObject deathScreenUI;

    [Header("Player Components to Disable")]
    [Tooltip("PlayerMovement, MouseLook, and Weapon scripts here")]
    public MonoBehaviour[] scriptsToDisable; 

    [Header("Player State")]
    public GameObject player;

    [Header("UI State")]
    [SerializeField]
    private ElementSelector elementSelector;
    
    // We only need to read health, not inherit it
    private Health playerHealthComponent;

    [SerializeField]
    public string mainMenuSceneName;
    public string gameSceneName;


    void Start()
    {
        Time.timeScale = 1f;
        GameIsPaused = false;
        IsAnyUIOpen = false;

        if (elementSelector == null)
        {
            elementSelector = FindObjectOfType<ElementSelector>();
        }

        // Auto-find player if not assigned
        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
        }

        if (player != null)
        {
            playerHealthComponent = player.GetComponent<Health>();
        }
    }

    void Update()
    {
        // 1. Handle Pause Input
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else if (elementSelector != null && elementSelector.IsOpen)
            {
                elementSelector.Toggle();
                Pause();
            }
            else
            {
                Pause();
            }
        }

        // 2. Handle Death Logic
        // We check health here to see if we should trigger the death screen
        if (playerHealthComponent != null && playerHealthComponent.CurrentHealth <= 0 && !GameIsPaused)
        {
            GameOver();
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
        IsAnyUIOpen = elementSelector != null && elementSelector.IsOpen;

        // Re-enable controls
        TogglePlayerScripts(true);
        
        // Hide Cursor and lock it to center
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
        IsAnyUIOpen = true;

        // Disable controls
        TogglePlayerScripts(false);

        // Unlock Cursor so you can click buttons
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void GameOver()
    {
        deathScreenUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
        IsAnyUIOpen = true;

        // Disable controls
        TogglePlayerScripts(false);

        // Unlock Cursor for buttons
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // --- Helper to turn off/on all player scripts (Movement, Look, Shoot) ---
    private void TogglePlayerScripts(bool status)
    {
        foreach (MonoBehaviour script in scriptsToDisable)
        {
            if (script != null)
            {
                script.enabled = status;
            }
        }
    }

    // --- Button Functions ---

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        GameIsPaused = false;
        IsAnyUIOpen = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        GameIsPaused = false;
        IsAnyUIOpen = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public static void SetUIOpenState(bool isOpen)
    {
        IsAnyUIOpen = isOpen;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void LoadGame() {
        SceneManager.LoadScene(gameSceneName);
    }
}