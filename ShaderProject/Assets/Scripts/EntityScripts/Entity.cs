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
    [SerializeField] private ParticleSystem _HitParticle;
    private int _gold = 0;
    private float _XptoLvlUp = 100;
    private List<Item> _inventory = new List<Item>();
    
    public float Health { get { return _health; } set { _health = value; } }
    public float MaxHealth { get { return _Maxhealth; } set { _Maxhealth = value; } }
    public int GOLD { get { return _gold; } set { _gold = value; } }
    [SerializeField] public List<Item> Inventory { get { return _inventory; } private set{ _inventory = value; } }
    public float Damage { get { return _damage; } set { _damage = value; } }
    public bool IsPlayer {get { return _isPlayer; } set { _isPlayer = value; }}
    bool tookdamage = false;
    public event Action OnDamageTaken;
    public static event Action<bool> OnDeath;

    private CinemachineImpulseSource _impulse;

    private void Start()
    {
        _impulse = GetComponent<CinemachineImpulseSource>();
    }

    public void TakeDamage(float damage)
    {
        Health -= damage;
        UpdateHealth();
        
        if (soundeffectdamage != null)
        {
            SoundSystem.Instance.PlaySFX(soundeffectdamage, 0.3f);
        }

        if (_HitParticle != null)
        {
            Debug.Log("Hit");
            Instantiate(_HitParticle, transform.position, Quaternion.identity);
        }
        OnDamageTaken?.Invoke();
    }
    

    public void AddItem(Item item)
    {
        Inventory.Add(item);
        Debug.Log(Inventory.Count);
    }

    public void AddGold(int amount)
    {
        GOLD += amount;
    }
    

    void UpdateHealth()
    {
        if (Health <= 0)
        {
            Debug.Log("Enemy DIED");
            Health = 0;
            OnDeath?.Invoke(_isPlayer);
        }
    }
}