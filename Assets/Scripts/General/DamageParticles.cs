using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamageParticles : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float moveDistance = 2f;
    public AnimationCurve moveCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Timing Settings")]
    public float aliveTime = 3f;
    
    [Header("Text Settings")]
    public TextMeshProUGUI damageText;
    public float fadeOutSpeed = 2f;

    private Vector3 moveDirection;
    private float elapsedTime;
    private Transform playerCamera;
    private bool isMoving = true;
    private bool isFading = false;
    private Vector3 startPosition;
    private float distanceTraveled;

    void Start()
    {
        moveDirection = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ).normalized;

        playerCamera = Camera.main.transform;
        startPosition = transform.position;
        
        // Start the alive timer immediately
        StartCoroutine(AliveTimer());
    }

    void Update()
    {
        // Look at camera
        if (playerCamera != null)
        {
            transform.LookAt(playerCamera);
        }

        if (isMoving)
        {
            // Calculate distance traveled so far
            distanceTraveled = Vector3.Distance(startPosition, transform.position);
            float normalizedDistance = distanceTraveled / moveDistance;

            // Calculate current speed based on curve
            float currentSpeed = moveSpeed * moveCurve.Evaluate(normalizedDistance);

            // Move in random direction
            transform.position += moveDirection * currentSpeed * Time.deltaTime;

            // Stop moving after reaching target distance
            if (distanceTraveled >= moveDistance)
            {
                isMoving = false;
                StartCoroutine(StayTimer());
            }
        }
        
        // Handle text fade out
        if (isFading && damageText != null)
        {
            Color textColor = damageText.color;
            textColor.a -= fadeOutSpeed * Time.deltaTime;
            damageText.color = textColor;

            if (textColor.a <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    private IEnumerator AliveTimer()
    {
        // Wait for the total alive time
        yield return new WaitForSeconds(aliveTime);
        
        // Start fade out
        isFading = true;
    }

    private IEnumerator StayTimer()
    {
        // Only wait for stay time (we're already done moving)
        yield return new WaitForSeconds(aliveTime - elapsedTime);
        
        // Start fade out
        isFading = true;
    }
}
