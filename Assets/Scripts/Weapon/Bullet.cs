using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 100f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private GameObject impactEffectPrefab;
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private float maxDistance = 1000f; // Maximum distance to project the target point
    [SerializeField] private bool useHitObjectMaterial = false; // Toggle for material matching feature

    private Vector3 previousPosition;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        
        // Get the direction towards the center of screen
        Camera mainCam = Camera.main;
        Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 targetPoint;

        // If we hit something with our raycast, use that point, otherwise project forward
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, collisionMask))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.origin + ray.direction * maxDistance;
        }

        // Calculate direction from bullet to target and set velocity
        Vector3 direction = (targetPoint - transform.position).normalized;
        rb.linearVelocity = direction * speed;
        
        // Rotate bullet to face travel direction
        transform.forward = direction;
        
        previousPosition = transform.position;
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        Vector3 currentPosition = transform.position;
        RaycastHit hit;
        Vector3 direction = currentPosition - previousPosition;
        float distance = direction.magnitude;

        if (distance > 0 && Physics.Raycast(previousPosition, direction, out hit, distance, collisionMask))
        {
            HandleCollision(hit);
        }

        previousPosition = currentPosition;
    }

    private void HandleCollision(RaycastHit hit)
    {
        if (impactEffectPrefab != null)
        {
            GameObject impact = Instantiate(impactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            
            // Only apply material if the feature is enabled
            if (useHitObjectMaterial)
            {
                Material hitMaterial = null;
                Renderer hitRenderer = hit.collider.GetComponent<Renderer>();
                if (hitRenderer != null && hitRenderer.material != null)
                {
                    hitMaterial = hitRenderer.material;
                    
                    if (hitMaterial != null)
                    {
                        ParticleSystem[] particleSystems = impact.GetComponentsInChildren<ParticleSystem>();
                        foreach (ParticleSystem ps in particleSystems)
                        {
                            ParticleSystemRenderer psRenderer = ps.GetComponent<ParticleSystemRenderer>();
                            if (psRenderer != null)
                            {
                                psRenderer.material = hitMaterial;
                            }
                        }
                    }
                }
            }
            
            Destroy(impact, 2f);
        }
        Destroy(gameObject);
    }
}
