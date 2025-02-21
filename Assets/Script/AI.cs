using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("D�placement")]
    public float moveSpeed = 5f;           // Vitesse de d�placement de l'ennemi
    public float attackRange = 10f;        // Distance � partir de laquelle l'ennemi commence � attaquer

    [Header("Attaque")]
    public GameObject projectilePrefab;    // Pr�fabriqu� du projectile � tirer
    public float projectileSpeed = 10f;      // Vitesse du projectile
    public float fireRate = 1f;            // Nombre de tirs par seconde
    public Transform gunTransform;         // Point de tir (doit �tre assign� dans l'inspecteur)

    private Transform player;              // R�f�rence au joueur (recherch� via son tag "Player")
    private float fireCooldown = 0f;       // Timer pour g�rer la cadence de tir

    void Start()
    {
        // Recherche le joueur dans la sc�ne via son tag "Player"
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("Aucun objet avec le tag 'Player' n'a �t� trouv� dans la sc�ne !");
        }
    }

    void Update()
    {
        if (player == null)
            return;

        // Calcul de la distance entre l'ennemi et le joueur
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > attackRange)
        {
            // L'ennemi se d�place vers le joueur
            Vector3 direction = (player.position - transform.position).normalized;
            transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);

            // Oriente l'ennemi dans la direction du mouvement
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
        else
        {
            // Le joueur est � port�e, l'ennemi tente de tirer
            if (fireCooldown <= 0f)
            {
                Fire();
                fireCooldown = 1f / fireRate; // R�initialise le timer de tir
            }
            else
            {
                fireCooldown -= Time.deltaTime;
            }
        }
    }

    // M�thode pour instancier et propulser le projectile
    void Fire()
    {
        if (gunTransform == null)
        {
            Debug.LogWarning("Le point de tir (gunTransform) n'est pas assign� !");
            return;
        }

        // Instanciation du projectile � la position et rotation du point de tir
        GameObject projectile = Instantiate(projectilePrefab, gunTransform.position, gunTransform.rotation);

        // Application de la v�locit� au projectile dans la direction o� le gun est orient�
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = gunTransform.forward * projectileSpeed;
        }
    }
}

