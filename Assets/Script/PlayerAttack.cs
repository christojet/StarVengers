using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform firePoint; // Point de tir (devant le joueur)
    public float projectileSpeed = 10f; // Vitesse du projectile

    public void Fire()
    {
        // Instancier le projectile
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        // Définir la direction (toujours sur l'axe Z du StarVenger)
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * projectileSpeed;
        }
    }
}
