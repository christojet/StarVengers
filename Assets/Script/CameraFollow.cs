using UnityEngine;

public class SuiviPlayer : MonoBehaviour
{
    // Référence au transform du joueur
    private Transform player;

    // Décalage de la caméra par rapport au joueur (modifiable dans l'inspecteur)
    public Vector3 offset = new Vector3(0, 10, 0);

    void Start()
    {
        // Fixe la rotation de la caméra à 90° sur l'axe X
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }

    void LateUpdate()
    {
        // Si le joueur n'est pas encore trouvé, le rechercher par son tag
        if (player == null)
        {
            GameObject joueur = GameObject.FindGameObjectWithTag("Player");
            if (joueur != null)
            {
                player = joueur.transform;
            }
        }

        // Si le joueur a été trouvé, mettre à jour la position de la caméra
        if (player != null)
        {
            transform.position = player.position + offset;
        }
    }
}


