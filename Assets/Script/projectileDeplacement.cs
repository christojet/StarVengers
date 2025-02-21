using Fusion;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [SerializeField] private float lifeTime = 10f; // Durée de vie du projectile avant destruction
    private Vector3 velocity;

    public void Init(Vector3 initialVelocity)
    {
        velocity = initialVelocity;
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority) // Seul le propriétaire met à jour la position
        {
            transform.position += velocity * Runner.DeltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!Object || !Runner) return; // Vérification anti-erreur si l'objet est détruit

        if (other.CompareTag("Enemy"))
        {
            if (Runner.IsServer)
            {
                Runner.Despawn(Object); // Détruit immédiatement le projectile
            }
        }
    }

    public override void Spawned()
    {
        if (Runner.IsServer)
        {
            Invoke(nameof(DestroyProjectile), lifeTime); // Lance la destruction après `lifeTime`
        }
    }

    private void DestroyProjectile()
    {
        if (Object && Runner)
        {
            Runner.Despawn(Object);
        }
    }
}



