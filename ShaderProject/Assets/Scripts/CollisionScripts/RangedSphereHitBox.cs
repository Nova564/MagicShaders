using System;
using Unity.VisualScripting;
using UnityEngine;

public class RangedSphereHitBox : MonoBehaviour
{
    private ProjectileStats projectileStats;
    private bool hasLeftParent = false;
    private Transform nearestEnemy;
    private float searchInterval = 0.1f;
    private float nextSearchTime = 0f;
    private float homingSpeed = 15f;

    private void Awake()
    {
        projectileStats = GetComponent<ProjectileStats>();
        Destroy(this.gameObject, 2.5f);
    }

    private void Update()
    {
        if (transform.parent == null && !hasLeftParent)
        {
            hasLeftParent = true;
        }

        if (hasLeftParent)
        {
            if (Time.time >= nextSearchTime)
            {
                FindNearestEnemy();
                nextSearchTime = Time.time + searchInterval;
            }

            if (nearestEnemy != null)
            {
                MoveTowardsEnemy();
            }
        }
    }

    private void FindNearestEnemy()
    {
        EnemyControl[] enemies = FindObjectsByType<EnemyControl>(FindObjectsSortMode.None);
        float closestDistance = Mathf.Infinity;
        Transform closest = null;

        foreach (EnemyControl enemy in enemies)
        {
            if (enemy != null && !enemy.gameObject.IsDestroyed())
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = enemy.transform;
                }
            }
        }

        nearestEnemy = closest;
    }

    private void MoveTowardsEnemy()
    {
        if (nearestEnemy == null) return;

        Vector3 direction = (nearestEnemy.position - transform.position).normalized;

        transform.position += direction * homingSpeed * Time.deltaTime;

        transform.forward = direction;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<EnemyControl>() != null)
        {
            CollisionSystem.Instance.HandleCollision(this.gameObject, other.gameObject);

            if (projectileStats != null && projectileStats.CollisionParticle != null)
            {
                projectileStats.SpawnParticle(projectileStats.CollisionParticle);
            }

            Destroy(this.gameObject);
        }
    }
}