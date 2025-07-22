using UnityEngine;
using UnityEngine.InputSystem;
public class Player : MonoBehaviour
{
	InputSystem_Actions playerinput;
	
	private InputAction move;
	private InputAction shot;
	// movement
	private Vector2 m_Velocity = Vector2.zero;
	[SerializeField]
    [Range(0.05f, 0.3f)] 
	private float m_MovementSmoothing = 0.1f;
    [SerializeField] private float speed = 15.0f;
    private Rigidbody2D rb;
	// Bullet
	[SerializeField] private GameObject bulletprefab;
	private bool bulletInScene = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
	void Awake()
    {
        playerinput = new InputSystem_Actions();
    }
	void OnEnable()
	{
		move = playerinput.Player.Move;
		move.Enable();
		shot = playerinput.Player.Attack;
		shot.Enable();
	}
	void OnDisable()
	{
		move.Disable();
		shot.Disable();
	}
	void Update()
	{
		bool _shoot = shot.WasPressedThisFrame();
		if (_shoot == true && bulletInScene == false)
		{
			Debug.Log("shot "+_shoot);
			GameObject bullet_created = 
				Instantiate(bulletprefab, transform.GetChild(0));
			bulletInScene = true;
			bullet_created.GetComponent<Bullet>().Set_Shooter(gameObject);

		}
	}
	public void Set_bulletInScene(bool _bulletInScene)
	{
		bulletInScene = _bulletInScene;
	}
	
	void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    // Update is called once per frame
    void FixedUpdate()
    {
		Vector2 _move = move.ReadValue<Vector2>();
		Vector2 targetVelocity = 
		new Vector2( _move.x * speed * 10 * Time.fixedDeltaTime, 0);
		
        rb.linearVelocity = 
		Vector2.SmoothDamp(rb.linearVelocity, 
		targetVelocity, ref m_Velocity, 
		m_MovementSmoothing);
    }
}
