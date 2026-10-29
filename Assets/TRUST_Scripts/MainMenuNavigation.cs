using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuNavigation : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("THEMA");
    }
}