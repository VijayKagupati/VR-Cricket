using UnityEngine;
using UnityEngine.Events;

public class ScoringSystem : MonoBehaviour
{
    [SerializeField] private int score;
    [SerializeField] private bool isGameOver;
    [SerializeField] private GameManager gameManager;

    [Header("Events")]
    public UnityEvent<int> OnScoreChanged;
    public UnityEvent<int> OnBoundaryHit;
    public UnityEvent OnWicketHit;

    [Header("Debug")]
    [SerializeField] private int boundaryRuns;
    [SerializeField] private int totalBoundaries;
    [SerializeField] private int totalSixes;
    [SerializeField] private int totalFours;
    [SerializeField] private int wicketsTaken;

    public void ResetScore()
    {
        score = 0;
        boundaryRuns = 0;
        totalBoundaries = 0;
        totalSixes = 0;
        totalFours = 0;
        wicketsTaken = 0;
        isGameOver = false;

        OnScoreChanged?.Invoke(score);
    }

    public void AddRuns(int runs)
    {
        score += runs;
        OnScoreChanged?.Invoke(score);
    }

    public int GetScore()
    {
        return score;
    }

    public int GetWickets()
    {
        return wicketsTaken;
    }

    public void EndGame()
    {
        isGameOver = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (isGameOver) return;

        CricketBall ball = other.GetComponent<CricketBall>();
        if (ball != null && !ball.HasBeenScored())
        {
            ball.MarkAsScored();

            // Check if wicket was hit (out)
            if (ball.HasHitWicket())
            {
                wicketsTaken++;
                OnWicketHit?.Invoke();
                Debug.Log("OUT! Ball hit the wickets!");
            }
            // Only score runs if ball was hit by bat
            else if (ball.HasBeenHitByBat())
            {
                // Check if boundary was crossed with bat hit
                if (ball.HasBouncedOnGround())
                {
                    // Four runs - ball bounced and crossed boundary
                    AddRuns(4);
                    boundaryRuns += 4;
                    totalBoundaries++;
                    totalFours++;
                    OnBoundaryHit?.Invoke(4);
                    Debug.Log("FOUR! Ball bounced and crossed boundary");
                }
                else
                {
                    // Six runs - ball went over boundary without bouncing
                    AddRuns(6);
                    boundaryRuns += 6;
                    totalBoundaries++;
                    totalSixes++;
                    OnBoundaryHit?.Invoke(6);
                    Debug.Log("SIX! Ball crossed boundary directly");
                }
            }
            else
            {
                OnBoundaryHit?.Invoke(0);
                Debug.Log("Ball missed, no runs awarded");
            }
        }
    }
}