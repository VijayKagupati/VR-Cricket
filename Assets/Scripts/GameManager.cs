using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [Header("Game References")]
    [SerializeField] private Animator bowlerAnimator;
    [SerializeField] private ScoringSystem scoringSystem;
    [Header("UI Elements")]
    [SerializeField] private GameObject fourUI;
    [SerializeField] private GameObject sixUI;
    [SerializeField] private GameObject outUI;

    [Header("Game Settings")]
    [SerializeField] private float initialDelay = 3f;
    [SerializeField] private float playResultDisplayTime = 2f;
    [SerializeField] private float ballStoppedThreshold = 0.1f;
    [SerializeField] private float maxPlayTime = 5f;

    [Header("Events")]
    public UnityEvent OnGameStarted;
    public UnityEvent OnBallThrown;
    public UnityEvent OnPlayCompleted;

    private bool gameActive = false;
    private bool playInProgress = false;
    private CricketBall currentBall;

    private void Start()
    {
        HideAllUI();
    
        if (scoringSystem != null)
        {
            Debug.Log("Subscribing to scoring events");
            scoringSystem.OnBoundaryHit.AddListener(HandleBoundaryHit);
            scoringSystem.OnWicketHit.AddListener(HandleWicketHit);
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
        OnGameStarted?.Invoke();
        Debug.Log("Cricket game started!");
        
        StartCoroutine(StartNextBowl());
    }

    public void EndGame()
    {
        if (!gameActive) return;
        
        gameActive = false;
        scoringSystem?.EndGame();
        
        HideAllUI();
        StopAllCoroutines();
        Debug.Log("Cricket game ended!");
    }

    private IEnumerator StartNextBowl()
    {
        Debug.Log("StartNextBowl called - gameActive: " + gameActive);
        while (!gameActive) 
        {
            Debug.LogWarning("StartNextBowl exiting early because gameActive is false");
            yield break;
        }
    
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

}