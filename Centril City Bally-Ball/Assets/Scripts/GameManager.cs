using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events; // Use events
using UnityEngine.UI; // Use UI
using TMPro;  // Use text mesh pro

public enum matchType
{
    Singles,
    Doubles
}

public enum GameState
{
    Start,
    Match,
    End,
    Paused
}

public class GameManager : MonoBehaviour
{
    public EndMenu endMenu;
    public GameObject[] canvases;
    public TMP_Text leftScoreText;
    public TMP_Text rightScoreText;

    private int winScore = 7;
    private int leftScore = 0;
    private int rightScore = 0;

    private float ballGroundedTimer = 0;
    public float ballGroundedDuration;


    public GameObject[] allAthletes;
    public GameObject ball;
    public PauseMenu pauseMenu;

    private matchType matchType;
    public GameState gameState = GameState.Start;

    // Start is called before the first frame update
    void Start()
    {
        // Handle events
        Ball.OnBallLanded += IncrementScore;
        pauseMenu.resume.onClick.AddListener(delegate { ResumeGame(); });

        // Find the ball and athletes
        if (ball == null)
            ball = GameObject.FindGameObjectWithTag("Ball");
        allAthletes = GameObject.FindGameObjectsWithTag("Athlete");

        // Reset game on rematch
        endMenu.rematch.onClick.AddListener(delegate {
            ChangeScore('L', -leftScore);
            ChangeScore('R', -rightScore);

            canvases[(int)gameState].SetActive(false);
            gameState = GameState.Start;
            canvases[(int)gameState].SetActive(true);

            // Assign me as the server
            GameObject.Find("Left Player").GetComponent<Athlete>().serving = true;
            ball.GetComponent<Ball>().beholder = GameObject.Find("Left Player").GetComponent<Athlete>();
        });

        // Determine game type
        if (allAthletes.Length == 2)
            matchType = matchType.Singles;
        else if (allAthletes.Length == 4)
            matchType = matchType.Doubles;

        // Assign me as the server
        GameObject.Find("Left Player").GetComponent<Athlete>().serving = true;
        ball.GetComponent<Ball>().beholder = GameObject.Find("Left Player").GetComponent<Athlete>();

        // All athletes and ball should ignore collisions
        for (int i = 0; i < allAthletes.Length; i++)
        {
            Physics2D.IgnoreCollision(ball.GetComponent<Collider2D>(), allAthletes[i].GetComponent<Collider2D>());

            for (int n = 0; n < allAthletes.Length; n++)
                if (n != i)
                    Physics2D.IgnoreCollision(allAthletes[n].GetComponent<Collider2D>(), allAthletes[i].GetComponent<Collider2D>());
        }

        // Freeze time
        Time.timeScale = 0;
    }

    // Update is called once per frame
    void Update()
    {
        // Check game state
        switch (gameState)
        {
            case GameState.Start:

                if (Input.GetKeyDown(KeyCode.Return))
                {
                    foreach (GameObject athlete in allAthletes)
                    {
                        if (!athlete.GetComponent<Athlete>().aI)
                            athlete.GetComponent<Athlete>().controllable = true;
                    }

                    Time.timeScale = 1;

                    canvases[(int)gameState].SetActive(false);
                    gameState = GameState.Match;
                    canvases[(int)gameState].SetActive(true);

                }

                break;


            case GameState.Match:

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    foreach (GameObject athlete in allAthletes)
                        athlete.GetComponent<Athlete>().controllable = false;
                    Time.timeScale = 0;

                    canvases[(int)gameState].SetActive(false);
                    gameState = GameState.Paused;
                    canvases[(int)gameState].SetActive(true);

                    canvases[(int)gameState].GetComponentInChildren<Button>().Select();
                }
                else if (leftScore >= winScore || rightScore >= winScore)
                {
                    foreach (GameObject athlete in allAthletes)
                        athlete.GetComponent<Athlete>().controllable = false;
                    Time.timeScale = 0;

                    canvases[(int)gameState].SetActive(false);
                    gameState = GameState.End;
                    canvases[(int)gameState].SetActive(true);

                    if (leftScore > rightScore)
                        endMenu.result.text = "Left team wins!";
                    else
                        endMenu.result.text = "Right team wins!";
                }
                break;

            case GameState.End:

                break;

            case GameState.Paused:

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    ResumeGame();
                }

                break;
        }
    }


    // Event handler
    private void IncrementScore()
    {
        // Left side TECHNICALLY has an advantage here
        if (ball.transform.position.x > 0)
        {
            ChangeScore('L', 1);

            foreach (GameObject athlete in allAthletes)
            {
                if (!athlete.GetComponent<Athlete>().leftTeam)
                {
                    athlete.GetComponent<Athlete>().serving = true;
                    ball.GetComponent<Ball>().Respawn(athlete.GetComponent<Athlete>());
                    break;
                }
            }

        }
        else
        {
            ChangeScore('R', 1);

            foreach (GameObject athlete in allAthletes)
            {
                if (athlete.GetComponent<Athlete>().leftTeam)
                {
                    athlete.GetComponent<Athlete>().serving = true;
                    ball.GetComponent<Ball>().Respawn(athlete.GetComponent<Athlete>());
                    break;
                }
            }
        }
    }

    // Changes the value of a team's score, and updates the GUI
    private void ChangeScore(char team, int n)
    {
        if (team == 'L')
        {
            leftScore += n;
            leftScoreText.text = leftScore.ToString();
        }
        else if (team == 'R')
        {
            rightScore += n;
            rightScoreText.text = rightScore.ToString();
        }
    }

    private void ResumeGame()
    {
        foreach (GameObject athlete in allAthletes)
        {
            if (!athlete.GetComponent<Athlete>().aI)
                athlete.GetComponent<Athlete>().controllable = true;
        }
        Time.timeScale = 1;

        canvases[(int)gameState].SetActive(false);
        gameState = GameState.Match;
        canvases[(int)gameState].SetActive(true);
    }
}
