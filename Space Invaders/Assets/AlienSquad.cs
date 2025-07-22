using UnityEngine;

public class AlienSquad : MonoBehaviour
{
    [SerializeField] private GameObject AlienPrefab;
    private int aliens_total = 55;
    private int aliens_column = 5;
    private int aliens_line = 11;
    private Rigidbody2D rb;
    // movement
    private Vector2 m_Velocity = Vector2.zero;
	[SerializeField]
    [Range(0.05f, 0.3f)] 
    private float m_MovementSmoothing = 0.1f;
    [SerializeField] private float speed = 15.0f;
    private int dir = 1;
    public enum MoveDirectionType
    {
        horizontal,
        vertical
    };
    [SerializeField] 
    private MoveDirectionType MoveDirection 
    = MoveDirectionType.horizontal;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        for (int j = 0; j < aliens_column; j++)
        {
            for (int i = 0; i < aliens_line; i++)
            {
                GameObject alien_created =
                Instantiate(AlienPrefab, transform.position 
                + new Vector3(i * 1.1f,-j * 1.1f,0), 
                Quaternion.identity);

                alien_created.transform.SetParent(transform);
            }
        }
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (MoveDirection == MoveDirectionType.horizontal)
        {
            Move_Horizontal();
        }
        else
        {
            Move_Vertical();
        }
    }
    void Move_Horizontal()
    {
		Vector2 targetVelocity = 
		new Vector2( dir * speed * 10 * 
        Time.fixedDeltaTime, 0);
		
        rb.linearVelocity = 
		Vector2.SmoothDamp(rb.linearVelocity, 
		targetVelocity, ref m_Velocity, 
		m_MovementSmoothing);
    }
    void Move_Vertical()
    {
		Vector2 targetVelocity = 
		new Vector2( 0, 
        -speed * 10 * 
        Time.fixedDeltaTime);
		
        rb.linearVelocity = 
		Vector2.SmoothDamp(rb.linearVelocity, 
		targetVelocity, ref m_Velocity, 
		m_MovementSmoothing);
    }

    void OnCollisionEnter2D(Collision2D col)
	{

    }
}
