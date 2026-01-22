using UnityEngine;

public class CollisionSystem : MonoBehaviour
{
    public static CollisionSystem Instance { get; set; }
    bool AIsType;
    bool BIsType;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void HandleItemPickup(GameObject PlayerSphere, GameObject Itempick)
    {
        Entity EntityA = null;
        if (EntityA == null)
        {
            EntityA = HasEntityCompInParents(PlayerSphere);
        }
        Item item = Itempick.GetComponent<Item>();
        if (item.IsGold)
        {
            EntityA.AddGold(item.GoldAmount);
            Destroy(Itempick);
        }
        else
        {
            Item newItem = new Item();
            newItem.Name = item.Name;
            newItem.ID = item.ID;
            if (item != null && EntityA != null && EntityA.IsPlayer)
            {
                EntityA.AddItem(newItem);
                Destroy(Itempick);
            }
        }
    }

    public void HandleCollision(GameObject objA, GameObject objB)
    {
        if (objB.GetComponent<DetectionSphere>() || objB.GetComponent<RangedSphereHitBox>()|| objB.GetComponent<SphereHitBox>())
        {
            return;
        }
        ProjectileStats ProjA = null;
        Entity EntityB = null;
       
        if (ProjA == null && EntityB == null)
        {
            ProjA = objA.GetComponent<ProjectileStats>();
            EntityB = HasEntityCompInParents(objB);
            
        }


        if (ProjA != null && EntityB != null && ProjA.projectile.IsPlayer != EntityB.IsPlayer)
        {
            if (ProjA.CollisionParticle != null)
            {
                ProjA.SpawnParticle(ProjA.CollisionParticle);
            }
            EntityB.TakeDamage(ProjA.projectile.Damage);
            Destroy(objA);
        }
        
    }

    private Entity HasEntityCompInParents(GameObject obj)
    {
        Transform current = obj.transform;
        while (current != null)
        {
            if (current.GetComponent<Entity>())
                return current.GetComponent<Entity>();
            current = current.parent;
        }
        return null;
    }
    


}