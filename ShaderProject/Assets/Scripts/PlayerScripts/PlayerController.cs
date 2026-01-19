using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    
    [SerializeField] Entity _playerEntity;
    private List<bool>isBattle = new List<bool>();
    [SerializeField] InputActionProperty _attack1;
    [SerializeField] InputActionProperty _attack2;
    [SerializeField] InputActionProperty _attack3;
    [SerializeField] InputActionProperty _attack4;
    [SerializeField] InputActionProperty _interact;
    [SerializeField] GameObject HitBoxPrefab;
    [SerializeField] GameObject ArrowPrefab;
    [SerializeField] GameObject FireballPrefab;
    [SerializeField] GameObject UltPrefab;
    [SerializeField] private List<AudioClip> audioSource;
    [SerializeField] Animator _animator;
    [SerializeField] float AtkCoolDown = 1.0f;
    [SerializeField] float AtkCoolDown2 = 4.0f;
    [SerializeField] float AtkCoolDown3 = 10.0f;
    [SerializeField] float AtkCoolDown4 = 50.0f;
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
    
    [SerializeField] private AudioClip soundeffecthit;
    [SerializeField] private ParticleSystem _WinningQuestParticule;

    void Awake()
    {
        _attack1.action.Enable();
        _attack1.action.performed += Attack1;
        _attack2.action.performed += Attack2;
        _attack3.action.performed += Attack3; 
        _attack4.action.performed += Attack4;
    }

    void OnDestroy()
    {
        _attack1.action.performed -= Attack1;
        _attack2.action.performed -= Attack2;
        _attack3.action.performed -= Attack3;
        _attack4.action.performed -= Attack4;
    }
    void Attack1(InputAction.CallbackContext ctx)
    {
        if (Time.time > nextAtk)
        {
            _animator.SetTrigger("PlayerAttack");
            _animator.SetBool("IsIdle", false);
            nextAtk = Time.time + AtkCoolDown;
            OnCoolDownAtk?.Invoke(AtkCoolDown, 0);
            Vector3 offset = transform.forward * 1.0f;
            Vector3 spawnPosition = transform.position + offset;
            spawnPosition.y += 2.3f;
            Quaternion spawnRotation = transform.rotation;
            GameObject Hitbox = Instantiate(HitBoxPrefab, spawnPosition, spawnRotation);
            Projectile projectile = new Projectile();
            projectile.projectile = Hitbox;
            projectile.Direction = transform.forward.normalized;
            projectile.rotation = transform.rotation;
            projectile.projectileSpeed = 0.0f;
            projectile.Damage = _playerEntity.Damage;
            projectile.IsPlayer = _playerEntity.IsPlayer;
            Hitbox.GetComponent<ProjectileStats>().projectile = projectile;
            if (Hitbox.GetComponent<ProjectileStats>().SwordParticle != null)
            {
                Hitbox.GetComponent<ProjectileStats>().SpawnParticle(Hitbox.GetComponent<ProjectileStats>().SwordParticle);
            }
        }
    }

    void Attack2(InputAction.CallbackContext ctx)
    {
        if (Time.time > nextAtk2)
        {
            _animator.SetTrigger("PlayerAttack");
            _animator.SetBool("IsIdle", false);
            nextAtk2 = Time.time + AtkCoolDown2;
            OnCoolDownAtk?.Invoke(AtkCoolDown2, 1);
            Vector3 spawnPosition = transform.position;
            spawnPosition.y += 2.3f;
            Quaternion spawnRotation = transform.rotation;
            GameObject Hitbox = Instantiate(ArrowPrefab, spawnPosition, spawnRotation);
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
    }
    
    void Attack3(InputAction.CallbackContext ctx)
    {
        if (Time.time > nextAtk3)
        {
            _animator.SetTrigger("PlayerAttack");
            _animator.SetBool("IsIdle", false);
            nextAtk3 = Time.time + AtkCoolDown3;
            OnCoolDownAtk?.Invoke(AtkCoolDown3, 2);
            Vector3 spawnPosition = transform.position;
            spawnPosition.y += 2.3f;
            Quaternion spawnRotation = transform.rotation;
            GameObject Hitbox = Instantiate(FireballPrefab, spawnPosition, spawnRotation);
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
    }
    
    void Attack4(InputAction.CallbackContext ctx)
    {
        if (Time.time > nextAtk4)
        {
            _animator.SetTrigger("PlayerAttack");
            _animator.SetBool("IsIdle", false);
            nextAtk4 = Time.time + AtkCoolDown4;
            OnCoolDownAtk?.Invoke(AtkCoolDown4, 3);
            Vector3 offset = transform.forward * 1.0f;
            Vector3 spawnPosition = transform.position + offset;
            spawnPosition.y += 2.3f;
            Quaternion spawnRotation = transform.rotation;
            GameObject Hitbox = Instantiate(UltPrefab, spawnPosition, spawnRotation);
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
    }

    void UpdateCheckQuest()
    {
        foreach (Quest quest in QuestList)
        {
            quest.ItemActualAmount = 0;
            foreach (Item item in _playerEntity.Inventory)
            {
                Debug.Log(item.Name + " ID: " + item.ID + " Quest item ID: " + quest.ItemReq.ID);
                if (item.ID == quest.ItemReq.ID)
                {
                    quest.ItemActualAmount += 1;
                }
            }

            if (quest.ItemActualAmount >= quest.ItemAmountReq)
            {
                Instantiate(_WinningQuestParticule, transform.position, Quaternion.identity);
                QuestList.Remove(quest);
            }
        }
    }
    
    void Update()
    {
        UpdatePlayerProjectiles();
        UpdateCheckQuest();
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