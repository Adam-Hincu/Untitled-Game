using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float launchForce = 10f;
    [SerializeField] private string jumpPadSound = "JumpPad";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object is on the player layer
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            // Try to get the rigidbody component from the parent
            Rigidbody rb = other.GetComponentInParent<Rigidbody>();
            if (rb != null)
            {
                // Launch the player upward
                rb.AddForce(Vector3.up * launchForce, ForceMode.Impulse);
                // Play the jump pad sound
                AudioManager.Instance.Play(jumpPadSound);
            }
        }
    }
}
