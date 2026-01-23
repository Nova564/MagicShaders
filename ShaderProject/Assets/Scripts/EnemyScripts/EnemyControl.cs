﻿
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.UIElements;


[RequireComponent(typeof(NavMeshAgent))]
public class EnemyControl : MonoBehaviour
{
    PlayerController playerController;
    private NavMeshAgent navAgent;
    private float wanderDistance = 4f;

    public float EDamage;
    public float EHealth;
    private float EMaxHealth;
    private float EAtkSpeed;
    private float EAtkRange;
    private float EAggroRange;
    private bool EIsBoss;
    [SerializeField] private bool EIsDead;
    bool isBattle;
    private Transform ETarget;

    private float recovery = 1.5f;
    private float hittime = 0;

    [SerializeField] private float EHeight;

    private float nextAtk;

    Renderer Erenderer;
    
    [SerializeField] SO_Enemy enemyData;

    [SerializeField] UnityEngine.UI.Image healthbar;

    [SerializeField] GameObject hitboxPrefab;

    [SerializeField] EnemyState EState;
    

    private GameObject player;
    [SerializeField] private List<AudioClip> ShootSounds = new List<AudioClip>();
    Animator EAnimator;
    
    bool tookdamage = false;
    [SerializeField] private Entity _enemyEntity;
    private List<Projectile> _enemyProjectiles = new List<Projectile>();
    private float _projectileSpeed;
    private bool EIsRanged;
    [SerializeField] private List<GameObject> _DropsPrefab;
    [SerializeField] private AudioClip soundeffecthit;
    [SerializeField] private AudioClip soundeffectdeath;


    public enum EnemyState
    {
        Walking,
        Idle,
        Air,
        Attack,
        Hit,
        Dead
    }

    private void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        if (enemyData != null)
            LoadEnemy(enemyData);
        else
        {
            AudioClip clip = Resources.Load<AudioClip>("SoundEffect/SoldatCac/Soldat_Slash_1");
            AudioClip clip0 = Resources.Load<AudioClip>("SoundEffect/SoldatCac/Soldat_Slash_2");
    
            if (clip != null) ShootSounds.Add(clip);
            if (clip0 != null) ShootSounds.Add(clip);
        }
        

        _enemyEntity.OnDamageTaken += UpdateHealth;
        Entity.OnDeath += EnemyDied;
        SetEntityStats();
    }

    void OnDestroy()
    {
        foreach (var drops in _DropsPrefab)
        {
            GameObject NewDrop = Instantiate(drops, transform.position, Quaternion.identity);
        }
        _enemyEntity.OnDamageTaken -= UpdateHealth;
        Entity.OnDeath -= EnemyDied;
    }

    void EnemyDied(bool isPlayer)
    {
        if (!isPlayer && _enemyEntity.Health <=0)
        {
            EIsDead = true;
            if (soundeffectdeath != null)
            {
                SoundSystem.Instance.PlaySFX(soundeffectdeath, 0.3f);
            }
        }
    }

    private void LoadEnemy(SO_Enemy _data)
    {

        // Instancie le modèle
        GameObject visuals = Instantiate(enemyData.EntityModel, transform);
        visuals.transform.localPosition = new Vector3(0,0,0);
        visuals.transform.rotation = Quaternion.identity;
        if (visuals.GetComponent<Animator>())
        {
            EAnimator = visuals.GetComponent<Animator>();
        }
        else
        {
            EAnimator = null;
        }

        // Statistiques
        navAgent.speed = enemyData.Speed;
        EDamage = enemyData.Damage;
        EHealth = enemyData.Health;
        EMaxHealth = enemyData.MaxHealth;
        EIsBoss = enemyData.IsBoss;
        EAtkSpeed = enemyData.AttackSpeed;
        EAtkRange = enemyData.AttackRange;
        EAggroRange = enemyData.AggroRange;
        EIsRanged = enemyData.IsRanged;
        _projectileSpeed = enemyData.ProjectileSpeed;
        EState = EnemyState.Idle;
        EIsDead = false;
        Erenderer = visuals.GetComponentInChildren<Renderer>();
        
        
        player = FindFirstObjectByType<PlayerMove>().gameObject;
    }


    private void Update()
    {
        UpdateHealthBarPosition();
        EnemyStateHandler();
        //UpdateEnemyProjectile();
    }

    void UpdateEnemyProjectile()
    {
        foreach (Projectile projectile in _enemyProjectiles)
        {
            if (projectile.projectile.IsDestroyed())
            {
                _enemyProjectiles.Remove(projectile);
                continue;
            }
            projectile.projectile.transform.position += projectile.Direction * projectile.projectileSpeed * Time.deltaTime;
            projectile.projectile.transform.rotation = projectile.rotation;
        }
    }
    
    void SetEntityStats()
    {
        _enemyEntity.Health =  EHealth;
        _enemyEntity.MaxHealth =  EMaxHealth;
        _enemyEntity.Damage = EDamage;
    }

    void UpdateEntityStats()
    {
        EHealth = _enemyEntity.Health;
        EMaxHealth = _enemyEntity.MaxHealth;
        EDamage = _enemyEntity.Damage;
    }
    
    void UpdateHealth()
    {
        UpdateEntityStats();
        
        float healthPercent = EHealth / EMaxHealth;

        healthbar.fillAmount = EHealth / EMaxHealth;
        
        tookdamage = true;
        hittime = Time.time;
    }
    
    void UpdateHealthBarPosition()
    {
        if (healthbar != null)
        {
            GameObject healthbarBackground = healthbar.transform.parent.gameObject;
            healthbarBackground.transform.position = new Vector3(transform.position.x, transform.position.y + EHeight + 1.0f, transform.position.z);
            healthbarBackground.transform.forward = Camera.main.transform.forward;
        }
    }

    private void EnemyStateHandler()
    {
        if (EIsDead)
        {

            EState = EnemyState.Dead;
            navAgent.enabled = false;
            if (EAnimator != null)
            {
                EAnimator.SetBool("IsDead", true);
                EAnimator.SetTrigger("OnDeath");
            }
            Destroy(gameObject, 0.8f);
        }
        else
        {
            if (EAnimator != null)
            EAnimator.SetBool("IsAttacking", false);
            if (tookdamage)
            {
                if (EAnimator != null)
                EAnimator.SetTrigger("GotHit");
                tookdamage = false;
                EState = EnemyState.Hit;
            }
            
            Transform closestTarget = null;
            float closestDistance = Mathf.Infinity;
            
            
                float distance = Vector3.Distance(transform.position, player.transform.position);
                Transform playerpos = player.transform;
                if (playerpos != null)
                {
                    if (distance <= EAggroRange && distance < closestDistance)
                    {

                        closestDistance = distance;
                        closestTarget = player.transform;
                    }
                    if (distance < EAtkRange && EState != EnemyState.Hit)
                {

                    EState = EnemyState.Attack;
                    Attack(EAtkSpeed);
                }
                else if (EState != EnemyState.Hit)
                {
                    EState = EnemyState.Idle;
                    if (EAnimator != null)
                    EAnimator.SetBool("IsIdle", true);
                }
                }
                
                
            // Change la cible si un joueur est dans la range d’aggro
            if (closestTarget != null)
            {
                ETarget = closestTarget;
            }
            else
            {
                // Si personne dans la range et aucune cible, reprendre la cible par défaut
                GameObject defaultTarget = FindFirstObjectByType<PlayerMove>().gameObject;
                if (defaultTarget != null)
                    ETarget = defaultTarget.transform;
            }

            Vector3 targetPostition = new Vector3( ETarget.position.x, transform.position.y, ETarget.position.z ) ;
            transform.LookAt( targetPostition ) ;

            if (EState != EnemyState.Dead && EState != EnemyState.Attack && EState != EnemyState.Hit && navAgent.enabled)
            {
                EState = EnemyState.Walking;
                if (EAnimator != null)
                {
                    EAnimator.SetBool("IsWalking", true);
                    EAnimator.SetBool("IsIdle", false);
                }


            }
            else if (EState == EnemyState.Hit && (Time.time > hittime + recovery))
            {
                EState = EnemyState.Idle;
                if (EAnimator != null)
                EAnimator.SetBool("IsIdle", true);
            }
            else if (EState != EnemyState.Attack && EState != EnemyState.Hit)
            {
                EState = EnemyState.Idle;
                if (EAnimator != null)
                EAnimator.SetBool("IsIdle", true);
            }
            Move();
            
        }
        
    }

    
    public void Attack(float AtkSpeed)
    {
        //attaque
        if (Time.time > nextAtk)
        {
            if (EAnimator != null)
            {
                EAnimator.SetBool("IsAttacking", true);
                EAnimator.SetBool("IsIdle", false);
            }

            nextAtk = Time.time + AtkSpeed;
           
            if (EIsRanged)
            {
                Vector3 offset = transform.forward * 0.0f;
                Vector3 spawnPosition = transform.position + offset;
                spawnPosition.y += EHeight;
                Quaternion spawnRotation = transform.rotation;
                GameObject Hitbox = Instantiate(hitboxPrefab, spawnPosition, spawnRotation);
                Projectile projectile = new Projectile();
                projectile.projectile = Hitbox;
                projectile.Direction = transform.forward.normalized;
                projectile.rotation = transform.rotation;
                projectile.projectileSpeed = _projectileSpeed;
                projectile.Damage = _enemyEntity.Damage;
                projectile.IsPlayer = _enemyEntity.IsPlayer;
                Hitbox.GetComponent<ProjectileStats>().projectile = projectile;
                _enemyProjectiles.Add(projectile);
                if(soundeffecthit != null) SoundSystem.Instance.PlaySFX(soundeffecthit, 0.3f);
            }
            else
            {
                Vector3 offset = transform.forward * 1.0f;
                Vector3 spawnPosition = transform.position + offset;
                spawnPosition.y += EHeight;
                Quaternion spawnRotation = transform.rotation;
                GameObject Hitbox = Instantiate(hitboxPrefab, spawnPosition, spawnRotation);
                Projectile projectile = new Projectile();
                projectile.projectile = Hitbox;
                projectile.Direction = transform.forward.normalized;
                projectile.rotation = transform.rotation;
                projectile.projectileSpeed = _projectileSpeed;
                projectile.Damage = _enemyEntity.Damage;
                projectile.IsPlayer = _enemyEntity.IsPlayer;
                Hitbox.GetComponent<ProjectileStats>().projectile = projectile;
                if(soundeffecthit != null) SoundSystem.Instance.PlaySFX(soundeffecthit, 0.3f);
            }
            PlayShootSounds();
            
        }
    }
    
    void Move()
    {
        if (EState != EnemyState.Walking)
        {
            if (EAnimator != null)
            EAnimator.SetBool("IsWalking", false);
            navAgent.isStopped = true;
            return;
        }

        if (ETarget != null)
        {
            float distance = Vector3.Distance(transform.position, ETarget.position);
            if (distance > EAtkRange)
            {
                //marche enemy
                navAgent.isStopped = false;
                navAgent.SetDestination(ETarget.position);
            }
            else
            {
                navAgent.isStopped = true;
            }
        }
    }
    
    
    void PlayShootSounds()
    {
        if (ShootSounds.Count() > 0)
        {
            SoundSystem.Instance.PlaySFX(ShootSounds,0.2f);
        }
    }

}