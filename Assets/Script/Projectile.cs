using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float lifetime = 1f; // Durée avant destruction

    void Start()
    {
        // Détruire l'objet après un certain temps
        //Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other != null && other.CompareTag("Enemy")) // Vérifie si l'objet touché existe encore
        {
            Destroy(other.gameObject); // Détruit l’ennemi
        }

        if (gameObject != null) // Vérifie si le projectile existe toujours avant de le détruire
        {
            Destroy(gameObject, lifetime);
            //gameObject.SetActive(false); // Désactive au lieu de détruire
        }
    }
}
