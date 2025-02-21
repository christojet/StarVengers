using Fusion;
using UnityEngine;
using UnityEngine.UI; // Importer UnityEngine.UI pour gérer les boutons UI

public class PlayerAttack : NetworkBehaviour
{
    public NetworkPrefabRef projectilePrefab;
    public float projectileSpeed = 10f;
    public Transform firePoint; // Point de tir (doit être défini dans l'Inspector)
    private Button fireButton; // Bouton qui déclenchera Fire()

    public override void Spawned()
    {
        if (!HasInputAuthority) return; // Seul le joueur local configure le bouton

        // Recherche du bouton avec le tag "B_attack"
        GameObject buttonObj = GameObject.FindGameObjectWithTag("B_attack");
        if (buttonObj != null)
        {
            fireButton = buttonObj.GetComponent<Button>();
            if (fireButton != null)
            {
                fireButton.onClick.AddListener(Fire);
                Debug.Log("Bouton B_attack trouvé et associé à Fire()");
            }
            else
            {
                Debug.LogWarning("L'objet trouvé avec le tag 'B_attack' n'a pas de composant Button !");
            }
        }
        else
        {
            Debug.LogWarning("Aucun bouton trouvé avec le tag 'B_attack' !");
        }
    }

    public void Fire()
    {
        if (!HasInputAuthority) return; // Seul le joueur local peut tirer

        if (firePoint == null)
        {
            Debug.LogWarning("FirePoint non assigné !");
            return;
        }

        if (!projectilePrefab.IsValid)
        {
            Debug.LogError("Le prefab du projectile n'est pas valide ! Vérifie qu'il est bien référencé dans Fusion.");
            return;
        }

        Debug.Log("Fire() appelé. Instanciation du projectile en cours...");

        // Instancier le projectile en réseau à partir du joueur
        NetworkObject projectile = Runner.Spawn(projectilePrefab, firePoint.position, firePoint.rotation, Object.InputAuthority);

        if (projectile != null)
        {
            Debug.Log("Projectile instancié avec succès !");
            projectile.GetComponent<Projectile>().Init(firePoint.forward * projectileSpeed);
        }
        else
        {
            Debug.LogError("Échec de l'instanciation du projectile !");
        }
    }
}









