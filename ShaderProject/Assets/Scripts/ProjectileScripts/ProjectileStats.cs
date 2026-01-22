using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;

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
        PlayerController.OnCoolDownAtk += CallRoutine;
    }

    void OnDestroy()
    {
        PlayerController.OnCoolDownAtk -= CallRoutine;
    }

    void CallRoutine(float a, int index)
    {
         StartCoroutine(LancerApresDelai());
         PlayerController.OnCoolDownAtk -= CallRoutine;
    }

    IEnumerator LancerApresDelai()
    {
        yield return new WaitForSeconds(projectile.LaunchTimer);
        FreeFromParent();
    }

    void FreeFromParent()
    {
        if (this.gameObject.GetComponent<ParentConstraint>())
        {
            ParentConstraint constraint = this.gameObject.GetComponent<ParentConstraint>();
            constraint.RemoveSource(0);
        }
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
    public float LaunchTimer;
}