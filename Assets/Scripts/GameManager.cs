using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Game References")]
    [SerializeField] private Animator bowlerAnimator;
    [SerializeField] private ScoringSystem scoringSystem;

    [Header("UI Elements")]
    [SerializeField] private GameObject fourUI;
    [SerializeField] private GameObject sixUI;
    [SerializeField] private GameObject outUI;
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private TextMeshProUGUI runsWicketsText;
    [SerializeField] private TextMeshProUGUI oversText;
    [SerializeField] private TextMeshProUGUI targetText;
    [SerializeField] private TextMeshProUGUI gameResultText;

    [Header("Game Settings")]
    [SerializeField] private float initialDelay = 3f;
    [SerializeField] private float playResultDisplayTime = 2f;
    [SerializeField] private int maxWickets = 10;
    [SerializeField] private int maxOvers = 5;
    [SerializeField] private int ballsPerOver = 6;
    [SerializeField] private int scoretoWin = 50;

    [Header("Events")]
    public UnityEvent OnGameStarted;
    public UnityEvent OnBallThrown;
    public UnityEvent OnPlayCompleted;
    public UnityEvent<bool> OnGameOver; // true = win, false = lose

    [SerializeField] private GameObject[] _wickets;

    private Transform[] _wicketTransforms;
    private bool gameActive = false;
    private bool playInProgress = false;
    private CricketBall currentBall;
    
    // Match state tracking
    private int currentOver = 0;
    private int ballsInOver = 0;

    private void Start()
    {
        HideAllUI();
        StoreWicketTransforms();
        UpdateMatchDisplay();

        if (scoringSystem != null)
        {
            Debug.Log("Subscribing to scoring events");
            scoringSystem.OnBoundaryHit.AddListener(HandleBoundaryHit);
            scoringSystem.OnWicketHit.AddListener(HandleWicketHit);
            scoringSystem.OnScoreChanged.AddListener(_ => UpdateMatchDisplay());
        }
        else
        {
            Debug.LogError("ScoringSystem reference not set in GameManager!");
        }
    }

    public void StartGame()
    {
        if (gameActive) return;

        gameActive = true;
        scoringSystem?.ResetScore();
        currentOver = 0;
        ballsInOver = 0;
        OnGameStarted?.Invoke();
        Debug.Log("Cricket game started!");
        UpdateMatchDisplay();

        StartCoroutine(StartNextBowl());
    }

    public void EndGame(bool isWin)
    {
        if (!gameActive) return;

        gameActive = false;
        scoringSystem?.EndGame();

        HideAllUI();
        StopAllCoroutines();
        
        // Show game over UI with appropriate message
        if (gameResultText != null)
        {
            if (isWin)
                gameResultText.text = "You Win!\nScore: " + scoringSystem.GetScore();
            else
                gameResultText.text = "Game Over!\nWickets: " + scoringSystem.GetWickets() + "/" + maxWickets;
        }
        
        ShowUI(gameOverUI);
        OnGameOver?.Invoke(isWin);
        Debug.Log("Cricket game ended! Win: " + isWin);
    }

    private IEnumerator StartNextBowl()
    {
        Debug.Log("StartNextBowl called - gameActive: " + gameActive);
        while (!gameActive)
        {
            Debug.LogWarning("StartNextBowl exiting early because gameActive is false");
            yield break;
        }
        
        // Check if match is over (overs completed)
        if (currentOver >= maxOvers)
        {
            EndGame(false);
            yield break;
        }

        UpdateMatchDisplay();
        yield return new WaitForSeconds(initialDelay);

        if (bowlerAnimator != null)
        {
            ThrowBall();
            Debug.Log("Bowler started running");
        }
        else
        {
            Debug.LogWarning("Bowler animator not assigned!");
        }
    }

    public void ThrowBall()
    {
        if (!gameActive || playInProgress) return;

        ResetWicketTransforms();
        playInProgress = true;

        if (bowlerAnimator != null)
        {
            bowlerAnimator.SetTrigger("isRunning");
            OnBallThrown?.Invoke();
            
            Debug.Log("Ball thrown");
        }
        else
        {
            Debug.LogError("Bowler Animator not assigned!");
            playInProgress = false;
        }
    }


    private void HandleBoundaryHit(int runs)
    {
        if (runs == 4)
        {
            ShowUI(fourUI);
        }
        else if (runs == 6)
        {
            ShowUI(sixUI);
        }
        else
        {
            Debug.Log("Ball missed, no runs awarded");
        }
        CompletePlay();
    }

    private void HandleWicketHit()
    {
        ShowUI(outUI);
        CompletePlay();
    }

    private void CompletePlay()
    {
        if (!playInProgress) return;
        Debug.Log("Completing play, ready for next bowl");

        CricketBall[] balls = FindObjectsOfType<CricketBall>();
        foreach(CricketBall ball in balls)
        {
            Destroy(ball.gameObject);
        }
        playInProgress = false;
        
        // Increment ball count
        ballsInOver++;
        if (ballsInOver >= ballsPerOver)
        {
            ballsInOver = 0;
            currentOver++;
        }
        
        UpdateMatchDisplay();
        
        // Check win/lose conditions
        if (scoringSystem.GetScore() >= scoretoWin)
        {
            EndGame(true); // Win
            return;
        }
        
        if (scoringSystem.GetWickets() >= maxWickets)
        {
            EndGame(false); // Lose
            return;
        }
        
        OnPlayCompleted?.Invoke();
        StartCoroutine(PrepareForNextBowl());
    }

    private IEnumerator PrepareForNextBowl()
    {
        Debug.Log("Preparing for next bowl");
        yield return new WaitForSeconds(playResultDisplayTime);

        HideAllUI();
        Debug.Log("UI hidden, about to start next bowl");
        StartCoroutine(StartNextBowl());
        Debug.Log("StartNextBowl coroutine started");
    }
    
    private void UpdateMatchDisplay()
    {
        // Update runs and wickets display
        if (runsWicketsText != null && scoringSystem != null)
        {
            runsWicketsText.text = $"{scoringSystem.GetScore()}-{scoringSystem.GetWickets()}";
        }
        
        // Update overs display
        if (oversText != null)
        {
            oversText.text = $"{currentOver}.{ballsInOver}";
        }
        
        // Update target score display
        if (targetText != null && scoringSystem != null)
        {
            int runsNeeded = scoretoWin - scoringSystem.GetScore();
            targetText.text = $"{runsNeeded}";
        }
    }

    private void ShowUI(GameObject uiElement)
    {
        HideAllUI();
        if (uiElement != null)
        {
            uiElement.SetActive(true);
        }
    }

    private void HideAllUI()
    {
        if (fourUI != null) fourUI.SetActive(false);
        if (sixUI != null) sixUI.SetActive(false);
        if (outUI != null) outUI.SetActive(false);
    }

    private void StoreWicketTransforms()
    {
        if (_wickets == null || _wickets.Length == 0)
        {
            Debug.LogError("No wickets assigned in GameManager!");
            return;
        }

        _wicketTransforms = new Transform[_wickets.Length];
        for (int i = 0; i < _wickets.Length; i++)
        {
            if (_wickets[i] != null)
            {
                _wicketTransforms[i] = _wickets[i].transform;
            }
            else
            {
                Debug.LogWarning($"Wicket at index {i} is not assigned!");
            }
        }
    }

    private void ResetWicketTransforms()
    {
        if (_wickets == null || _wickets.Length == 0 || _wicketTransforms == null || _wicketTransforms.Length != _wickets.Length)
        {
            Debug.LogError("_wickets == null || _wickets.Length == 0 || _wicketTransforms == null || _wicketTransforms.Length != _wickets.Length");
            return;
        }

        for (int i = 0; i < _wickets.Length; i++)
        {
            if (_wickets[i] != null && _wicketTransforms != null)
            {
                _wickets[i].transform.position = _wicketTransforms[i].position;
                _wickets[i].transform.rotation = _wicketTransforms[i].rotation;
            }
            else
            {
                Debug.LogWarning($"Wicket at index {i} is not assigned!");
            }
        }
    }

    public void Restart()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}