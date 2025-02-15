using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;

    // Méthode publique pour pouvoir l'utiliser avec un bouton UI
    public void Fire()
    {
        // Recherche l'objet avec le tag "Gun"
        GameObject gun = GameObject.FindGameObjectWithTag("Gun");
        if (gun == null)
        {
            Debug.LogWarning("Objet 'Gun' non trouvé !");
            return;
        }

        // Le point de tir est défini par la position et la rotation du gun
        Transform firePoint = gun.transform;

        // Instancier le projectile en utilisant la rotation du gun
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        // Appliquer la vélocité dans la direction vers laquelle le gun est orienté
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = firePoint.forward * projectileSpeed;
        }
    }

    // Optionnel : permet également de tirer avec le bouton défini dans l'Input Manager
  
}



