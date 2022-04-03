using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Use scene management
using UnityEngine.UI; // Use UI
using UnityEngine.EventSystems; // Use event system

public class PauseMenu : MonoBehaviour
{
    // Fields
    public Button resume;
    public Button restart;
    public Button quitMatch;
    public Button quitToMenu;

    public Button exitGame;

    private Button[] buttons;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize button array
        buttons = new Button[5] {
            resume,
            restart,
            quitMatch,
            quitToMenu,
            exitGame
        };

        resume.Select();

        // Manage button presses
        restart.onClick.AddListener(delegate { SceneManager.LoadScene("Game"); });         // Just reload this scene
        quitMatch.onClick.AddListener(delegate { SceneManager.LoadScene("Main Menu"); });  // Swap to main menu for now
        quitToMenu.onClick.AddListener(delegate { SceneManager.LoadScene("Main Menu"); }); // Swap to main menu
        exitGame.onClick.AddListener(delegate { Application.Quit(); });                    // Close the game
    }

    private void OnEnable()
    {
        // UI is stupid and you have to first select nothing for the button to highlight
        EventSystem.current.SetSelectedGameObject(null);
        resume.Select();
    }
}
