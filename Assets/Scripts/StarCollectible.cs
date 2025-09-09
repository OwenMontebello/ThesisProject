using UnityEngine;

[RequireComponent(typeof(Collider))]
public class StarCollectible : MonoBehaviour
{
    void Reset()
    {
        var c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        GameManager.I.AddStar(1);
        Destroy(gameObject);
    }
}
