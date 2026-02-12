using UnityEngine;

public class AlphaRestart : MonoBehaviour
{
    private Health playerHealth;

    private void Awake()
    {
        playerHealth = GetComponent<Health>();
        if (playerHealth == null)
        {
            Debug.LogError("AlphaRestart: No Health component found on the GameObject.");
        }

        playerHealth.OnDeath += Restart;
    }

    public void Restart(Health health)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

}