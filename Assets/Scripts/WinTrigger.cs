using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WinTrigger : MonoBehaviour
{
    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        GameManager.I.Win();
        Destroy(gameObject);

    }

}
