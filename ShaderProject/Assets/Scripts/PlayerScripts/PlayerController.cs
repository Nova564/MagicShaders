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
    private List<bool> isBattle = new List<bool>();
    [SerializeField] InputActionProperty _autoAttack;
    [SerializeField] InputActionProperty _fireAttack;
    [SerializeField] InputActionProperty _lightningAttack;
    [SerializeField] InputActionProperty _IceAttack;
    [SerializeField] InputActionProperty _vortexAttack;
    [SerializeField] InputActionProperty _interact;
    [SerializeField] GameObject AutoAttackPrefab;

    // Références aux SpellCasters au lieu des préfabs directs
    private SpellFlameCaster _flameCaster;
    private SpellLightningCaster _lightningCaster;
    private SpellIceCaster _iceCaster;
    private SpellVoidCaster _vortexCaster;

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
    public static event Action<int, bool> OnSpellStateChanged; 
    private List<Projectile> _playerProjectiles = new List<Projectile>();
    [SerializeField] List<Quest> _ListQuests = new List<Quest>();
    public List<bool> IsBattle { get { return isBattle; } set { isBattle = value; } }
    public List<Quest> QuestList { get { return _ListQuests; } set { _ListQuests = value; } }
    public InputActionProperty InteractAction { get { return _interact; } set { _interact = value; } }
    private float YProjOffSet = 1.5f;

    private bool _isFlameInTelegraph = false;
    private bool _isLightningInTelegraph = false;
    private bool _isIceInTelegraph = false;
    private bool _isVortexInTelegraph = false;

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

        _flameCaster = GetComponent<SpellFlameCaster>();
        _lightningCaster = GetComponent<SpellLightningCaster>();
        _iceCaster = GetComponent<SpellIceCaster>();
        _vortexCaster = GetComponent<SpellVoidCaster>();

        if (_flameCaster == null) _flameCaster = gameObject.AddComponent<SpellFlameCaster>();
        if (_lightningCaster == null) _lightningCaster = gameObject.AddComponent<SpellLightningCaster>();
        if (_iceCaster == null) _iceCaster = gameObject.AddComponent<SpellIceCaster>();
        if (_vortexCaster == null) _vortexCaster = gameObject.AddComponent<SpellVoidCaster>();

        _flameCaster.enabled = false;
        _lightningCaster.enabled = false;
        _iceCaster.enabled = false;
        _vortexCaster.enabled = false;
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
            OnCoolDownAtk?.Invoke(AtkCoolDown, 0);
        }
    }

    void Attack2(InputAction.CallbackContext ctx)
    {
        if (!_isFlameInTelegraph && Time.time > nextAtk2)
        {
            _isFlameInTelegraph = true;
            OnSpellStateChanged?.Invoke(1, true); 
            _flameCaster.ActivateTelegraph();
            _flameCaster.enabled = true;
        }
        else if (_isFlameInTelegraph)
        {
            _flameCaster.CastSpell();
            _moveAnimator.SetTrigger("FireAttack");
            nextAtk2 = Time.time + AtkCoolDown2;
            OnCoolDownAtk?.Invoke(AtkCoolDown2, 1); 
            OnSpellStateChanged?.Invoke(1, false); 
            _isFlameInTelegraph = false;
            _flameCaster.enabled = false;
        }
    }

    void Attack3(InputAction.CallbackContext ctx)
    {
        if (!_isLightningInTelegraph && Time.time > nextAtk3)
        {
            _isLightningInTelegraph = true;
            OnSpellStateChanged?.Invoke(2, true); 
            _lightningCaster.ActivateTelegraph();
            _lightningCaster.enabled = true;
        }
        else if (_isLightningInTelegraph)
        {
            _lightningCaster.CastSpell();
            _moveAnimator.SetTrigger("LightningAttack");
            nextAtk3 = Time.time + AtkCoolDown3;
            OnCoolDownAtk?.Invoke(AtkCoolDown3, 2);
            OnSpellStateChanged?.Invoke(2, false);
            _isLightningInTelegraph = false;
            _lightningCaster.enabled = false;
        }
    }

    void Attack4(InputAction.CallbackContext ctx)
    {
        if (!_isIceInTelegraph && Time.time > nextAtk4)
        {
            _isIceInTelegraph = true;
            OnSpellStateChanged?.Invoke(3, true);
            _iceCaster.ActivateTelegraph();
            _iceCaster.enabled = true;
        }
        else if (_isIceInTelegraph)
        {
            _iceCaster.CastSpell();
            _moveAnimator.SetTrigger("IceAttack");
            nextAtk4 = Time.time + AtkCoolDown4;
            OnCoolDownAtk?.Invoke(AtkCoolDown4, 3); 
            OnSpellStateChanged?.Invoke(3, false);
            _isIceInTelegraph = false;
            _iceCaster.enabled = false;
        }
    }

    void Attack5(InputAction.CallbackContext ctx)
    {
        if (!_isVortexInTelegraph && Time.time > nextAtk5)
        {
            _isVortexInTelegraph = true;
            OnSpellStateChanged?.Invoke(4, true); 
            _vortexCaster.ActivateTelegraph();
            _vortexCaster.enabled = true;
        }
        else if (_isVortexInTelegraph)
        {

            _vortexCaster.CastSpell();
            _moveAnimator.SetTrigger("VortexAttack");
            nextAtk5 = Time.time + AtkCoolDown5;
            OnCoolDownAtk?.Invoke(AtkCoolDown5, 4); 
            OnSpellStateChanged?.Invoke(4, false);
            _isVortexInTelegraph = false;
            _vortexCaster.enabled = false;
        }
    }

    public void LaunchAutoAttack()
    {
        Vector3 spawnPosition = transform.position;
        spawnPosition.y += YProjOffSet;
        Quaternion spawnRotation = transform.rotation;
        GameObject Hitbox = Instantiate(AutoAttackPrefab, spawnPosition, spawnRotation);
        CreateProjectile(Hitbox, 7, 1.5f, 0.3f, AtkCoolDown, -1); 
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

        if (hitbox.GetComponent<ProjectileStats>() != null)
        {
            hitbox.GetComponent<ProjectileStats>().projectile = projectile;
        }

        _playerProjectiles.Add(projectile);
    }

    void Update()
    {
        UpdatePlayerProjectiles();

        if (Input.GetMouseButtonDown(1))
        {
            if (_isFlameInTelegraph)
            {
                _flameCaster.CancelPreview();
                _isFlameInTelegraph = false;
                _flameCaster.enabled = false;
                OnSpellStateChanged?.Invoke(1, false);
            }
            if (_isLightningInTelegraph)
            {
                _lightningCaster.CancelPreview();
                _isLightningInTelegraph = false;
                _lightningCaster.enabled = false;
                OnSpellStateChanged?.Invoke(2, false);
            }
            if (_isIceInTelegraph)
            {
                _iceCaster.CancelPreview();
                _isIceInTelegraph = false;
                _iceCaster.enabled = false;
                OnSpellStateChanged?.Invoke(3, false);
            }
            if (_isVortexInTelegraph)
            {
                _vortexCaster.CancelPreview();
                _isVortexInTelegraph = false;
                _vortexCaster.enabled = false;
                OnSpellStateChanged?.Invoke(4, false);
            }
        }
    }

    void UpdatePlayerProjectiles()
    {
        for (int i = _playerProjectiles.Count - 1; i >= 0; i--)
        {
            if (_playerProjectiles[i].projectile == null || _playerProjectiles[i].projectile.IsDestroyed())
            {
                _playerProjectiles.RemoveAt(i);
                continue;
            }
            _playerProjectiles[i].projectile.transform.position += _playerProjectiles[i].Direction * _playerProjectiles[i].projectileSpeed * Time.deltaTime;
            _playerProjectiles[i].projectile.transform.rotation = _playerProjectiles[i].rotation;
        }
    }
}