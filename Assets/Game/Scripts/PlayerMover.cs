using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    [SerializeField] private Transform spawner;
    [SerializeField] private float speed = 1f;
    [SerializeField] private float maxDistance = 10f;

    private void Update()
    {
        transform.Translate(transform.forward * (speed * Time.deltaTime));
        
        if (!(transform.position.z > maxDistance)) return;
        
        var teleportPosition = new Vector3(transform.position.x, transform.position.y, spawner.position.z);
        transform.position = teleportPosition;
    }
}
