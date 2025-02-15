using UnityEngine;

public class ExplosionOnBallHit : MonoBehaviour
{
    // Assignez ici le prefab de votre effet d'explosion dans l'inspecteur
    public GameObject explosionEffect;

    // Cette m�thode est appel�e lorsqu'une collision se produit
    private void OnCollisionEnter(Collision collision)
    {
        // V�rifie si l'objet en collision poss�de le tag "ball"
        if (collision.gameObject.CompareTag("Balle"))
        {
            // Instancie l'effet d'explosion � la position et rotation de l'objet
            Instantiate(explosionEffect, transform.position, transform.rotation);

            // D�truit cet objet
            Destroy(gameObject);
        }
    }
}
