using System;
using UnityEngine;

public class RangedSphereHitBox : MonoBehaviour
{
    private void Awake()
    {
        Destroy(this.gameObject, 2.5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        CollisionSystem.Instance.HandleCollision(this.gameObject, other.gameObject);
    }
}
