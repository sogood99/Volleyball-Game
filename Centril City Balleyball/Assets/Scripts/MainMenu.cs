using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Use scene management
using UnityEngine.UI; // Use UI

public class MainMenu : MonoBehaviour
{
    // Fields
    public Button campaign1v1;
    public Button campaign2v2;
    public Button freePlay1v1;
    public Button freePlay2v2;
    public Button settings;
    public Button credits;
    public Button quit;

    private Button[] buttons;
    private bool noneSelected = true;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize button array
        buttons = new Button[7] {
            campaign1v1,
            campaign2v2,
            freePlay1v1,
            freePlay2v2,
            settings,
            credits,
            quit
        };

        // Manage button presses
        campaign1v1.onClick.AddListener(delegate { ChangeScene(2); }); // Change to campaign scene when clicked
        campaign2v2.onClick.AddListener(delegate { ChangeScene(2); }); // Change to campaign scene when clicked
        freePlay1v1.onClick.AddListener(delegate { ChangeScene(2); }); // Change to free play scene when clicked
        freePlay2v2.onClick.AddListener(delegate { ChangeScene(2); }); // Change to free play scene when clicked
        settings.onClick.AddListener(delegate { ChangeScene(3); }); // Change to settings scene when clicked
        credits.onClick.AddListener(delegate { ChangeScene(4); }); // Change to credits scene when clicked
        quit.onClick.AddListener(delegate { Application.Quit(); }); // Close the app when clicked
    }

    // Update is called once per frame
    void Update()
    {
        // Select first button on keypress
        if (noneSelected)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                buttons[0].Select();
                noneSelected = false;
            }
        }
    }


    // Loads a scene based on build index
    void ChangeScene(int index) 
    {
        SceneManager.LoadScene(index);
    }
}
