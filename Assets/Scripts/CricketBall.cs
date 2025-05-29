using UnityEngine;

public class CricketBall : MonoBehaviour
{
    private bool hasBounced = false;
    private bool hasHitWicket = false;
    private bool hasBeenScored = false;
    private bool hasBeenHitByBat = false;  // New property to track bat contact
    private Rigidbody rb;
    [SerializeField] private float _lifeTime = 10f;
    
    private void Start()
    {
        Destroy(gameObject, _lifeTime);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

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
}