using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAttack : NetworkBehaviour
{
    public NetworkPrefabRef projectilePrefab;
    public float projectileSpeed = 10f;
    public Transform firePoint; // Doit être défini dans l'Inspector
    private Button fireButton;

    public override void Spawned()
    {
        if (!HasInputAuthority)
            return; // Seul le joueur local configure le bouton

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

    // Méthode appelée localement quand le bouton est pressé
    public void Fire()
    {
        if (!HasInputAuthority)
            return;

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

        Debug.Log("Fire() appelé. Envoi d'une demande de tir via RPC...");
        // Appel de l'RPC pour demander au serveur de spawn le projectile
        RpcFire();
    }

    // Cette méthode sera exécutée sur le serveur (State Authority)
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RpcFire(RpcInfo info = default)
    {
        if (firePoint == null)
        {
            Debug.LogWarning("FirePoint non assigné !");
            return;
        }

        Debug.Log("RpcFire() appelé sur le serveur. Instantiation du projectile...");
        // Le projectile est spawné au point de tir avec l'InputAuthority correspondant au joueur initiateur
        NetworkObject projectile = Runner.Spawn(projectilePrefab, firePoint.position, firePoint.rotation, info.Source);
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










