using UnityEngine;

public class CricketBallThrower : MonoBehaviour
{
    [Header("Ball Settings")]
    public GameObject ballPrefab;
    public Transform spawnPoint;
    
    [Header("Throw Settings")]
    public float throwForce = 12f;
    public float upwardAngle = 10f;
    
    [Header("Variation")]
    public bool addRandomness = true;
    public float forceVariation = 1.5f;
    public float directionVariation = 5f;
    public float spinIntensity = 0.5f;

    public void ThrowBall()
    {
        // Create ball at spawn position
        if (spawnPoint == null)
            spawnPoint = transform;
            
        GameObject ball = Instantiate(ballPrefab, spawnPoint.position, Quaternion.identity);
        Rigidbody ballRb = ball.GetComponent<Rigidbody>();
        
        if (ballRb == null)
        {
            ballRb = ball.AddComponent<Rigidbody>();
            ballRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
        
        // Calculate throw direction (forward with upward angle)
        Vector3 throwDirection = spawnPoint.forward;
        throwDirection = Quaternion.AngleAxis(upwardAngle, spawnPoint.right) * throwDirection;
        
        // Add randomness if enabled
        if (addRandomness)
        {
            // Randomize force
            float actualForce = throwForce + Random.Range(-forceVariation, forceVariation);
            
            // Randomize direction slightly
            float randomYaw = Random.Range(-directionVariation, directionVariation);
            float randomPitch = Random.Range(-directionVariation * 0.5f, directionVariation * 0.5f);
            throwDirection = Quaternion.Euler(randomPitch, randomYaw, 0) * throwDirection;
            
            // Apply force and spin
            ballRb.AddForce(throwDirection * actualForce, ForceMode.Impulse);
            ballRb.AddTorque(Random.insideUnitSphere * spinIntensity * actualForce, ForceMode.Impulse);
        }
        else
        {
            // Apply consistent force and spin
            ballRb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
            ballRb.AddTorque(Vector3.right * spinIntensity * throwForce, ForceMode.Impulse);
        }
    }
    
    // For testing in editor
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ThrowBall();
        }
    }
}