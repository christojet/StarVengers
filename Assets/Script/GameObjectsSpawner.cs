using UnityEngine;

public class GameObjectsSpawner : MonoBehaviour
{
    public GameObject blueShipPrefab; // Le prefab du vaisseau bleu
    public GameObject redShipPrefab;  // Le prefab du vaisseau rouge
    public GameObject enemyStarvengerPrefab; // Le prefab du StarVenger Rouge

    public Vector3 blueShipPosition = new Vector3(0, 0, 0); // Position du vaisseau bleu
    public Vector3 redShipPosition = new Vector3(10, 0, 0); // Position du vaisseau rouge
    public Vector3 enemyStarvengerPosition = new Vector3(5, 0, 5); // Position de l'ennemi

    void Start()
    {
        // Instancier les objets sur la carte
        Instantiate(blueShipPrefab, blueShipPosition, Quaternion.identity);
        Instantiate(redShipPrefab, redShipPosition, Quaternion.identity);
        Instantiate(enemyStarvengerPrefab, enemyStarvengerPosition, Quaternion.identity);
    }
}
