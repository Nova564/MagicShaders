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
    [SerializeField] Animator _eventAnimator;
    [SerializeField] float AtkCoolDown = 1.0f;
    [SerializeField] float AtkCoolDown2 = 4.0f;
    [SerializeField] float AtkCoolDown3 = 10.0f;
    [SerializeField] float AtkCoolDown4 = 50.0f;
    [SerializeField] Transform _handTransform;
    private ConstraintSource _constraintSource;
    float nextAtk;
    float nextAtk2;
    float nextAtk3;
    float nextAtk4;
    public event Action<float, int> OnCoolDownAtk;
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
        
        _constraintSource = new ConstraintSource();
        _constraintSource.sourceTransform = _handTransform;
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
            _eventAnimator.SetTrigger("AutoAttack");
            nextAtk = Time.time + AtkCoolDown;
            OnCoolDownAtk?.Invoke(AtkCoolDown, 0);
        }
    }

    void Attack2(InputAction.CallbackContext ctx)
    {
        if (Time.time > nextAtk2)
        {
            _moveAnimator.SetTrigger("FireAttack");
            _eventAnimator.SetTrigger("FireAttack");
            nextAtk2 = Time.time + AtkCoolDown2;
            OnCoolDownAtk?.Invoke(AtkCoolDown2, 1);
            
        }
    }
    
    void Attack3(InputAction.CallbackContext ctx)
    {
        if (Time.time > nextAtk3)
        {
            _moveAnimator.SetTrigger("LightningAttack");
            _eventAnimator.SetTrigger("LightningAttack");
            nextAtk3 = Time.time + AtkCoolDown3;
            OnCoolDownAtk?.Invoke(AtkCoolDown3, 2);
        }
    }
    
    void Attack4(InputAction.CallbackContext ctx)
    {
        if (Time.time > nextAtk4)
        {
            _moveAnimator.SetTrigger("IceAttack");
            _eventAnimator.SetTrigger("IceAttack");
            nextAtk4 = Time.time + AtkCoolDown4;
            OnCoolDownAtk?.Invoke(AtkCoolDown4, 3);
            
        }
    }
    
    void Attack5(InputAction.CallbackContext ctx)
    {
        if (Time.time > nextAtk4)
        {
            _moveAnimator.SetTrigger("VortexAttack");
            _eventAnimator.SetTrigger("VortexAttack");
            nextAtk4 = Time.time + AtkCoolDown4;
            OnCoolDownAtk?.Invoke(AtkCoolDown4, 3);
        }
    }

    public void LauchAutoAttack()
    {
        Vector3 spawnPosition = transform.position;
        spawnPosition.y += YProjOffSet;
        Quaternion spawnRotation = transform.rotation;
        GameObject Hitbox = Instantiate(FlameThrowerPrefab, spawnPosition, spawnRotation);
        //Hitbox.transform.SetParent(this.transform);
        Projectile projectile = new Projectile();
        projectile.projectile = Hitbox;
        projectile.Direction = transform.forward.normalized;
        projectile.rotation = transform.rotation;
        projectile.projectileSpeed = 12.0f;
        projectile.Damage = _playerEntity.Damage * 1.5f;
        projectile.IsPlayer = _playerEntity.IsPlayer;
        Hitbox.GetComponent<ProjectileStats>().projectile = projectile;
        _playerProjectiles.Add(projectile);
    }
    
    public void LauchFireAttack()
    {
        Vector3 spawnPosition = transform.position;
        spawnPosition.y += YProjOffSet;
        Quaternion spawnRotation = transform.rotation;
        GameObject Hitbox = Instantiate(FlameThrowerPrefab, spawnPosition, spawnRotation);
        //Hitbox.transform.SetParent(this.transform);
        Projectile projectile = new Projectile();
        projectile.projectile = Hitbox;
        projectile.Direction = transform.forward.normalized;
        projectile.rotation = transform.rotation;
        projectile.projectileSpeed = 12.0f;
        projectile.Damage = _playerEntity.Damage * 1.5f;
        projectile.IsPlayer = _playerEntity.IsPlayer;
        Hitbox.GetComponent<ProjectileStats>().projectile = projectile;
        _playerProjectiles.Add(projectile);
    }
    
    public void LauchLightningAttack()
    {
        Vector3 spawnPosition = transform.position;
        spawnPosition.y += YProjOffSet;
        Quaternion spawnRotation = transform.rotation;
        GameObject Hitbox = Instantiate(LightningBoltPrefab, spawnPosition, spawnRotation);
        //Hitbox.transform.SetParent(this.transform);
        Projectile projectile = new Projectile();
        projectile.projectile = Hitbox;
        projectile.Direction = transform.forward.normalized;
        projectile.rotation = transform.rotation;
        projectile.projectileSpeed = 12.0f;
        projectile.Damage = _playerEntity.Damage * 2.0f;
        projectile.IsPlayer = _playerEntity.IsPlayer;
        Hitbox.GetComponent<ProjectileStats>().projectile = projectile;
        _playerProjectiles.Add(projectile);
    }
    
    public void LauchIceAttack()
    {
        Vector3 offset = transform.forward * 1.0f;
        Vector3 spawnPosition = transform.position + offset;
        spawnPosition.y += YProjOffSet;
        Quaternion spawnRotation = transform.rotation;
        GameObject Hitbox = Instantiate(IcePrefab, spawnPosition, spawnRotation);
        Projectile projectile = new Projectile();
        projectile.projectile = Hitbox;
        projectile.Direction = transform.forward.normalized;
        projectile.rotation = transform.rotation;
        projectile.projectileSpeed = 5.0f;
        projectile.Damage = _playerEntity.Damage * 10.0f;
        projectile.IsPlayer = _playerEntity.IsPlayer;
        Hitbox.GetComponent<ProjectileStats>().projectile = projectile;
        _playerProjectiles.Add(projectile);
    }
    
    public void LauchVortexAttack()
    {
        Vector3 offset = transform.forward * 1.0f;
        Vector3 spawnPosition = transform.position + offset;
        spawnPosition.y += YProjOffSet;
        Quaternion spawnRotation = transform.rotation;
        GameObject Hitbox = Instantiate(VortexPrefab, _handTransform.position, spawnRotation);
        Projectile projectile = new Projectile();
        projectile.projectile = Hitbox;
        projectile.Direction = transform.forward.normalized;
        projectile.rotation = transform.rotation;
        projectile.projectileSpeed = 5.0f;
        projectile.Damage = _playerEntity.Damage * 10.0f;
        projectile.IsPlayer = _playerEntity.IsPlayer;
        Hitbox.GetComponent<ProjectileStats>().projectile = projectile;
        _playerProjectiles.Add(projectile);
        if (Hitbox.GetComponent<ParentConstraint>())
        {
            ParentConstraint constraint  =  Hitbox.GetComponent<ParentConstraint>();
            constraint.AddSource(_constraintSource);
            constraint.constraintActive = true;
            constraint.locked = true;
        }
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