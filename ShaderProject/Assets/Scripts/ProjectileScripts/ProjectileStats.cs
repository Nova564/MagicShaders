using UnityEngine;

public class ProjectileStats : MonoBehaviour
{
    public Projectile projectile {get; set;}
    [SerializeField] private ParticleSystem _projectileParticle;
    [SerializeField] private ParticleSystem _SwordParticle;
    public ParticleSystem ProjectileParticle { get { return _projectileParticle; } set{_projectileParticle = value;}}
    public ParticleSystem SwordParticle { get { return _SwordParticle; } set{_SwordParticle = value;}}

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