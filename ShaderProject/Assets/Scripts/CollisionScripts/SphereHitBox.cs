using System;
using UnityEngine;

public class SphereHitBox : MonoBehaviour
{
    private void Awake()
    {
        Destroy(this.gameObject, 0.3f);
    }

    private void OnTriggerEnter(Collider other)
    {
        CollisionSystem.Instance.HandleCollision(this.gameObject, other.gameObject);
    }
}
