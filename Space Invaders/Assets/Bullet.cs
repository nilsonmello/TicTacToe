using UnityEngine;

public class Bullet : MonoBehaviour
{
	private Rigidbody2D rb;
	[SerializeField] private float speed = 10;
	private GameObject shooter;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
		rb.AddForce(new Vector2(0,speed), ForceMode2D.Impulse);
    }
	void OnCollisionEnter2D(Collision2D col)
	{
		Debug.Log(col.gameObject);
		Destroy(gameObject);
		shooter.GetComponent<Player>().Set_bulletInScene(false);
	}
	public void Set_Shooter(GameObject _shooter)
	{
		shooter = _shooter;
	}
}
