using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [SerializeField] private bool _isPlayer;
    [SerializeField] private float _health = 100;
    [SerializeField] private float _Maxhealth = 100;
    [SerializeField] private float _damage = 5;
    [SerializeField] private AudioClip soundeffectdamage;
    private float _currentXP = 0;
    private int _LvL = 1;
    private float _XptoLvlUp = 100;
    private List<Item> _inventory = new List<Item>();
    
    public float Health { get { return _health; } set { _health = value; } }
    public float MaxHealth { get { return _Maxhealth; } set { _Maxhealth = value; } }
    public float XP { get { return _currentXP; } set { _currentXP = value; } }
    public int LVL { get { return _LvL; } set { _LvL = value; } }
    public float XPToLVLUP { get { return _XptoLvlUp; } private set{ _XptoLvlUp = value; } }
    [SerializeField] public List<Item> Inventory { get { return _inventory; } private set{ _inventory = value; } }
    public float Damage { get { return _damage; } set { _damage = value; } }
    public bool IsPlayer {get { return _isPlayer; } set { _isPlayer = value; }}
    bool tookdamage = false;
    public event Action OnDamageTaken;
    public event Action OnDeath;

    private CinemachineImpulseSource _impulse;

    private void Start()
    {
        _impulse = GetComponent<CinemachineImpulseSource>();
    }

    public void TakeDamage(float damage)
    {
        Health -= damage;
        OnDamageTaken?.Invoke();
        UpdateHealth();

        SoundSystem.Instance.PlaySFX(soundeffectdamage, 0.3f);

    }

    void LevelUp()
    {
        XP -= XPToLVLUP;
        LVL += 1;
        XPToLVLUP += XPToLVLUP;
        MaxHealth += MaxHealth;
        Health = MaxHealth;
        Damage += Damage;
    }

    void UpdateXP()
    {
        if (XP >= _XptoLvlUp)
        {
            LevelUp();
        }
    }

    public void AddItem(Item item)
    {
        Inventory.Add(item);
        Debug.Log(Inventory.Count);
    }

    public void AddXp(float amount)
    {
        XP += amount;
    }

    void Update()
    {
        UpdateXP();
    }

    void UpdateHealth()
    {
        if (Health <= 0)
        {
            Health = 0;
            OnDeath?.Invoke();
        }
    }
}