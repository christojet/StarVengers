using UnityEngine;

public class ExplosionOnBallHit : MonoBehaviour
{
    // Assignez ici le prefab de votre effet d'explosion dans l'inspecteur
    public GameObject explosionEffect;

    // Cette méthode est appelée lorsqu'une collision se produit
    private void OnCollisionEnter(Collision collision)
    {
        // Vérifie si l'objet en collision possède le tag "ball"
        if (collision.gameObject.CompareTag("Balle"))
        {
            // Instancie l'effet d'explosion à la position et rotation de l'objet
            Instantiate(explosionEffect, transform.position, transform.rotation);

            // Détruit cet objet
            Destroy(gameObject);
        }
    }
}
