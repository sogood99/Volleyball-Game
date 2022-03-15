using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public enum GameType
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
    public TMP_Text leftScoreText;
    public TMP_Text rightScoreText;
    private int leftScore = 0;
    private int rightScore = 0;


    public GameObject[] allAthletes;
    public GameObject ball;

    private GameType gameType;
    private GameState gameState;

    // Start is called before the first frame update
    void Start()
    {
        // Handle the event
        Ball.OnBallLanded += UpdateScore;

        // Find the ball and athletes
        ball = GameObject.FindGameObjectWithTag("Ball");
        allAthletes = GameObject.FindGameObjectsWithTag("Athlete");

        // Determine game type
        if (allAthletes.Length == 2)
            gameType = GameType.Singles;
        else if (allAthletes.Length == 4)
            gameType = GameType.Doubles;
        
        // All athletes and ball should ignore collisions
        for (int i = 0; i < allAthletes.Length; i++)
        {
            Physics2D.IgnoreCollision(ball.GetComponent<Collider2D>(), allAthletes[i].GetComponent<Collider2D>());

            for (int n = 0; n < allAthletes.Length; n++)
                if (n != i)
                    Physics2D.IgnoreCollision(allAthletes[n].GetComponent<Collider2D>(), allAthletes[i].GetComponent<Collider2D>());
        }
    }

    // Update is called once per frame
    void Update()
    {
        // I guess this always needs to be updated
        allAthletes = GameObject.FindGameObjectsWithTag("Athlete");
    }


    // Event handler
    private void UpdateScore()
    {
        // Right side TECHNICALLY has an advantage, here
        if (ball.transform.position.x < 0)
        {
            rightScore++;
            rightScoreText.text = rightScore.ToString();
        }
        else
        {
            leftScore++;
            leftScoreText.text = leftScore.ToString();
        }
    }
}
