using UnityEngine;

public class CricketBall : MonoBehaviour
{
    private bool hasBounced = false;
    private bool hasHitWicket = false;
    private bool hasBeenScored = false;
    private bool hasBeenHitByBat = false;
    [SerializeField] private float _lifeTime = 10f;

    private void OnCollisionEnter(Collision collision)
    {
        // Check if this collision is with the ground/pitch
        if (collision.gameObject.CompareTag("Ground"))
        {
            hasBounced = true;
        }

        // Check if this collision is with wickets
        if (collision.gameObject.CompareTag("Wickets"))
        {
            hasHitWicket = true;
        
            // Immediately notify scoring system about the wicket hit
            ScoringSystem scoringSystem = FindObjectOfType<ScoringSystem>();
            if (scoringSystem != null && !hasBeenScored)
            {
                hasBeenScored = true;
                scoringSystem?.RegisterWicketHit(this);  // Add null conditional operator
            }
        }

        // Check if this collision is with the bat
        if (collision.gameObject.CompareTag("Bat"))
        {
            hasBeenHitByBat = true;
        }
    }

    public bool HasBouncedOnGround()
    {
        return hasBounced;
    }

    public bool HasHitWicket()
    {
        return hasHitWicket;
    }

    public bool HasBeenHitByBat()
    {
        return hasBeenHitByBat;
    }

    public void MarkAsScored()
    {
        hasBeenScored = true;
    }

    public bool HasBeenScored()
    {
        return hasBeenScored;
    }

    private void Update()
    {
        _lifeTime -= Time.deltaTime;
        if (_lifeTime <= 0 && !hasBeenScored)
        {
            MarkAsScored();

            ScoringSystem scoringSystem = FindObjectOfType<ScoringSystem>();
            if (scoringSystem != null)
            {
                scoringSystem.OnBoundaryHit?.Invoke(0);
            }
            Destroy(gameObject);
        }
    }

}