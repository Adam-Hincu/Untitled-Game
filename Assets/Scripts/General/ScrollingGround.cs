using UnityEngine;

public class ScrollingGround : MonoBehaviour
{
    [SerializeField] private Transform ground1;
    [SerializeField] private Transform ground2;
    [SerializeField] public float scrollSpeed = 20f;
    [SerializeField] private float groundLength = 2000f;
    
    private Vector3 resetPosition;
    private bool isScrolling = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        resetPosition = new Vector3(groundLength, 0, 0);
    }

    public void StartScrolling()
    {
        isScrolling = true;
    }

    public void StopScrolling()
    {
        isScrolling = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isScrolling) return;

        // Move both grounds to the left
        ground1.Translate(Vector3.left * scrollSpeed * Time.deltaTime);
        ground2.Translate(Vector3.left * scrollSpeed * Time.deltaTime);

        // Check if ground1 has moved completely off screen
        if (ground1.position.x <= -groundLength)
        {
            ground1.position += resetPosition * 2;
        }

        // Check if ground2 has moved completely off screen
        if (ground2.position.x <= -groundLength)
        {
            ground2.position += resetPosition * 2;
        }
    }
}
