using UnityEngine;

[CreateAssetMenu(menuName = "Game_SO/Enemy")]
public class SO_Enemy : ScriptableObject
{
    public string Name;
    public float Health;
    public float MaxHealth;
    public bool IsDead;
    public float Damage;
    public float Speed;
    public float AttackSpeed;
    public float AttackRange;
    public float AggroRange;
    public bool IsRanged;
    public float ProjectileSpeed;
    public GameObject EntityModel;
    public bool IsBoss;
}
