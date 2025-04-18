using UnityEngine;

public class Face : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float moveSpeed = 1f;       // Vertical rising speed
    public float amplitude = 0.5f;     // How far to oscillate horizontally
    public float frequency = 1f;       // How fast the oscillation happens

    private Rigidbody2D rb;
    private Vector3 startPosition;     // Store the initial position

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
    }

    void Update()
    {
        // Calculate horizontal oscillation using a sine wave.
        float horizontalOffset = Mathf.Sin(Time.time * frequency) * amplitude;
        // Increase the y-position for upward movement.
        float newY = transform.position.y + moveSpeed * Time.deltaTime;

        // Apply both horizontal oscillation and upward movement.
        transform.position = new Vector3(startPosition.x + horizontalOffset, newY, transform.position.z);
    }
   

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ( collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
        
      



        }


}
