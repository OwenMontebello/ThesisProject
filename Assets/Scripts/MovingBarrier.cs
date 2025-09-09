using UnityEngine;

public class MovingBarrier : MonoBehaviour
{
    public Vector3 axis = Vector3.right; //move along X by default
    public float distance = 2f;
    public float speed = 1f;
    Vector3 _start;

    void Start() => _start = transform.position;

    void Update()
    {
        float t = Mathf.PingPong(Time.time * speed, 1f) * 2f - 1f; // -1..1
        transform.position = _start + axis.normalized * distance * t;
    }
    void OnTriggerEnter(Collider other)
    {
        //if (!other.CompareTag("Player")) return;
        //GameManager.I.Lose();
        //Destroy(gameObject);
    }
}
