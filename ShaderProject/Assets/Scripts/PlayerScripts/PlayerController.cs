using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    
    [SerializeField] Entity _playerEntity;
    private List<bool>isBattle = new List<bool>();
    [SerializeField] InputActionProperty _autoAttack;
    [SerializeField] InputActionProperty _fireAttack;
    [SerializeField] InputActionProperty _lightningAttack;
    [SerializeField] InputActionProperty _IceAttack;
    [SerializeField] InputActionProperty _vortexAttack;
    [SerializeField] InputActionProperty _interact;
    [SerializeField] GameObject AutoAttackPrefab;
    [SerializeField] GameObject FlameThrowerPrefab;
    [SerializeField] GameObject LightningBoltPrefab;
    [SerializeField] GameObject IcePrefab;
    [SerializeField] GameObject VortexPrefab;
    [SerializeField] private List<AudioClip> audioSource;
    [SerializeField] Animator _moveAnimator;
    [SerializeField] float AtkCoolDown = 1.0f;
    [SerializeField] float AtkCoolDown2 = 4.0f;
    [SerializeField] float AtkCoolDown3 = 10.0f;
    [SerializeField] float AtkCoolDown4 = 50.0f;
    [SerializeField] float AtkCoolDown5 = 50.0f;
    [SerializeField] Transform _handTransform;
    private ConstraintSource _constraintSource;
    float nextAtk;
    float nextAtk2;
    float nextAtk3;
    float nextAtk4;
    float nextAtk5;
    public static event Action<float, int> OnCoolDownAtk;
    private List<Projectile> _playerProjectiles = new List<Projectile>();
    [SerializeField] List<Quest> _ListQuests = new List<Quest>();
    public List <bool> IsBattle { get { return isBattle; } set { isBattle = value; } }
    public List <Quest> QuestList { get { return _ListQuests; } set { _ListQuests = value; } }
    public InputActionProperty InteractAction { get { return _interact; } set { _interact = value; } }
    private float YProjOffSet = 1.5f;
    

    void Awake()
    {
        _autoAttack.action.performed += Attack1;
        _fireAttack.action.performed += Attack2;
        _lightningAttack.action.performed += Attack3; 
        _IceAttack.action.performed += Attack4;
        _vortexAttack.action.performed += Attack5;
        
        _constraintSource = new ConstraintSource
        {
            sourceTransform = _handTransform,
            weight = 1f
        };
    }

    void OnDestroy()
    {
        _autoAttack.action.performed -= Attack1;
        _fireAttack.action.performed -= Attack2;
        _lightningAttack.action.performed -= Attack3;
        _IceAttack.action.performed -= Attack4;
        _vortexAttack.action.performed -= Attack5;
    }
    void Attack1(InputAction.CallbackContext ctx)
    {
        if (Time.time > nextAtk)
        {
            _moveAnimator.SetTrigger("AutoAttack");
            nextAtk = Time.time + AtkCoolDown;
            LaunchAutoAttack();
        }
    }

    void Attack2(InputAction.CallbackContext ctx)
    {
        if (Time.time > nextAtk2)
        {
            _moveAnimator.SetTrigger("FireAttack");
            nextAtk2 = Time.time + AtkCoolDown2;
            LaunchFireAttack();
        }
    }
    
    void Attack3(InputAction.CallbackContext ctx)
    {
        if (Time.time > nextAtk3)
        {
            _moveAnimator.SetTrigger("LightningAttack");
            nextAtk3 = Time.time + AtkCoolDown3;
            LaunchLightningAttack();
        }
    }
    
    void Attack4(InputAction.CallbackContext ctx)
    {
        if (Time.time > nextAtk4)
        {
            _moveAnimator.SetTrigger("IceAttack");
            nextAtk4 = Time.time + AtkCoolDown4;
            LaunchIceAttack();
        }
    }
    
    void Attack5(InputAction.CallbackContext ctx)
    {
        if (Time.time > nextAtk5)
        {
            _moveAnimator.SetTrigger("VortexAttack");
            nextAtk5 = Time.time + AtkCoolDown5;
            LaunchVortexAttack();
        }
    }

    public void LaunchAutoAttack()
    {
        Vector3 spawnPosition = transform.position;
        spawnPosition.y += YProjOffSet;
        Quaternion spawnRotation = transform.rotation;
        GameObject Hitbox = Instantiate(FlameThrowerPrefab, spawnPosition, spawnRotation);
        CreateProjectile(Hitbox, 7, 1.5f, 0.3f, AtkCoolDown, 0);
        ParentConstraint constraint = Hitbox.AddComponent<ParentConstraint>();
        constraint.AddSource(_constraintSource);
        constraint.locked = true;
        constraint.constraintActive = true;
    }
    
    public void LaunchFireAttack()
    {
        Vector3 spawnPosition = transform.position;
        spawnPosition.y += YProjOffSet;
        Quaternion spawnRotation = transform.rotation;
        GameObject Hitbox = Instantiate(FlameThrowerPrefab, spawnPosition, spawnRotation);
        CreateProjectile(Hitbox, 8, 1.5f, 0.85f, AtkCoolDown2, 1);
        ParentConstraint constraint = Hitbox.AddComponent<ParentConstraint>();
        constraint.AddSource(_constraintSource);
        constraint.locked = true;
        constraint.constraintActive = true;
    }
    
    public void LaunchLightningAttack()
    {
        Vector3 spawnPosition = transform.position;
        spawnPosition.y += YProjOffSet;
        Quaternion spawnRotation = transform.rotation;
        GameObject Hitbox = Instantiate(LightningBoltPrefab, spawnPosition, spawnRotation);
        CreateProjectile(Hitbox, 8, 2, 0.7f, AtkCoolDown3, 2);
        ParentConstraint constraint = Hitbox.AddComponent<ParentConstraint>();
        constraint.AddSource(_constraintSource);
        constraint.locked = true;
        constraint.constraintActive = true;
    }
    
    public void LaunchIceAttack()
    {
        Vector3 spawnPosition = transform.position;
        spawnPosition.y += YProjOffSet;
        Quaternion spawnRotation = transform.rotation;
        GameObject Hitbox = Instantiate(IcePrefab, spawnPosition, spawnRotation);
        CreateProjectile(Hitbox, 8, 10, 1.0f, AtkCoolDown4, 3);
        ParentConstraint constraint = Hitbox.AddComponent<ParentConstraint>();
        constraint.AddSource(_constraintSource);
        constraint.locked = true;
        constraint.constraintActive = true;
    }

    public void LaunchVortexAttack()
    {
        Vector3 offset = transform.forward * 1.0f;
        Vector3 spawnPosition = transform.position + offset;
        spawnPosition.y += YProjOffSet;
        Quaternion spawnRotation = transform.rotation;
        GameObject Hitbox = Instantiate(VortexPrefab, _handTransform.position, spawnRotation);
        CreateProjectile(Hitbox, 8, 10, 1.35f, AtkCoolDown5, 4);
        ParentConstraint constraint = Hitbox.AddComponent<ParentConstraint>();
        constraint.AddSource(_constraintSource);
        constraint.locked = true;
        constraint.constraintActive = true;
        
    }

    void CreateProjectile(GameObject hitbox, float ProjectileSpeed, float DamageMultiplier, float LaunchTimer, float AtkCD, int SpellIndex)
    {
        Projectile projectile = new Projectile()
        {
            projectile = hitbox,
            Direction = transform.forward.normalized,
            rotation = transform.rotation,
            projectileSpeed = ProjectileSpeed,
            Damage = _playerEntity.Damage * DamageMultiplier,
            IsPlayer = _playerEntity.IsPlayer,
            LaunchTimer = LaunchTimer,
        };
        hitbox.GetComponent<ProjectileStats>().projectile = projectile;
        _playerProjectiles.Add(projectile);
        OnCoolDownAtk?.Invoke(AtkCD, SpellIndex);
    }
    
    void Update()
    {
        UpdatePlayerProjectiles();
    }

    void UpdatePlayerProjectiles()
    {
        foreach (Projectile projectile in _playerProjectiles)
        {
            if (projectile.projectile.IsDestroyed())
            {
                _playerProjectiles.Remove(projectile);
                continue;
            }
            projectile.projectile.transform.position += projectile.Direction * projectile.projectileSpeed * Time.deltaTime;
            projectile.projectile.transform.rotation = projectile.rotation;
        }
    }
}