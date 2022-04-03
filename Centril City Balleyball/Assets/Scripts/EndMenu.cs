using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Use scene management
using UnityEngine.UI; // Use UI
using UnityEngine.EventSystems; // Use event system
using TMPro; // Use text mesh pro

public class EndMenu : MonoBehaviour
{
    // Fields
    public TMP_Text result;
    public Button rematch;
    public Button charSelect;
    public Button quitToMenu;

    private Button[] buttons;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize button array
        buttons = new Button[3] {
            rematch,
            charSelect,
            quitToMenu
        };

        rematch.Select();

        // Manage button presses
        charSelect.onClick.AddListener(delegate { SceneManager.LoadScene("Main Menu"); });  // Swap to main menu for now
        quitToMenu.onClick.AddListener(delegate { SceneManager.LoadScene("Main Menu"); }); // Swap to main menu
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnEnable()
    {
        // UI is stupid and you have to first select nothing for the button to highlight
        EventSystem.current.SetSelectedGameObject(null);
        rematch.Select();
    }
}
