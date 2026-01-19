using UnityEngine;

public class ProjectileStats : MonoBehaviour
{
    public Projectile projectile {get; set;}
    [SerializeField] private ParticleSystem _collisionParticle;
    [SerializeField] private ParticleSystem _launchParticle;
    public ParticleSystem CollisionParticle { get { return _collisionParticle; } set{_collisionParticle = value;}}
    public ParticleSystem LaunchParticle { get { return _launchParticle; } set{_launchParticle = value;}}

    void Awake()
    {
        projectile = new Projectile();
    }

    public void SpawnParticle(ParticleSystem  particle)
    {
        Instantiate(particle, transform.position, Quaternion.identity);
    }
}

public class Projectile
{
    public GameObject projectile;
    public Vector3 Direction;
    public Quaternion rotation;
    public float projectileSpeed;
    public float Damage;
    public bool IsPlayer;
}