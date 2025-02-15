using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public int width = 40;  // Largeur de la carte
    public int height = 40; // Hauteur de la carte
    public GameObject wallPrefab; // Préfabriqué pour les murs
    public GameObject groundPrefab; // Préfabriqué pour le sol
    public GameObject blueShipPrefab; // Préfabriqué pour le vaisseau bleu
    public GameObject redShipPrefab;  // Préfabriqué pour le vaisseau rouge
    public GameObject enemyStarvengerPrefab; // Préfabriqué pour le StarVenger Rouge

    public LayerMask wallLayer; // Layer des murs pour la vérification des positions

    void Start()
    {
        GenerateMap();
        PlaceGameObjects();
    }

    // Génération de la carte
    void GenerateMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                // Générer le sol
                Instantiate(groundPrefab, new Vector3(x, 0, z), Quaternion.identity);

                // Ajouter des murs sauf dans la zone centrale
                if ((x < width / 3 || x > 2 * width / 3) || (z < height / 3 || z > 2 * height / 3))
                {
                    if (Random.value > 0.7f) // 30% de chance d'avoir un mur
                    {
                        Instantiate(wallPrefab, new Vector3(x, 0.5f, z), Quaternion.identity);
                    }
                }
            }
        }
    }

    // Fonction pour vérifier si une position est libre
    bool IsPositionFree(Vector3 position)
    {
        RaycastHit hit;
        // Effectuer un raycast pour vérifier si la position est libre (sans mur)
        if (Physics.Raycast(position + Vector3.up * 10, Vector3.down, out hit, 20f, wallLayer))
        {
            return false; // Position occupée par un mur
        }
        return true; // Position libre
    }

    // Fonction pour placer les vaisseaux et le StarVenger
    void PlaceGameObjects()
    {
        // Placer le vaisseau bleu aléatoirement
        Vector3 blueShipPosition = GetRandomPosition();
        if (IsPositionFree(blueShipPosition))
        {
            GameObject blueShip = Instantiate(blueShipPrefab, blueShipPosition, Quaternion.identity);

            // Placer le StarVenger bleu près du vaisseau bleu
            Vector3 blueStarvengerPosition = blueShipPosition + new Vector3(2, 0, 2); // À côté du vaisseau bleu
            if (IsPositionFree(blueStarvengerPosition))
            {
                Instantiate(enemyStarvengerPrefab, blueStarvengerPosition, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("Position pour le StarVenger bleu occupée par un mur.");
            }
        }
        else
        {
            Debug.LogWarning("Position pour le vaisseau bleu occupée par un mur.");
        }

        // Placer le vaisseau rouge aléatoirement
        Vector3 redShipPosition = GetRandomPosition();
        if (IsPositionFree(redShipPosition))
        {
            GameObject redShip = Instantiate(redShipPrefab, redShipPosition, Quaternion.identity);

            // Placer le StarVenger rouge près du vaisseau rouge
            Vector3 redStarvengerPosition = redShipPosition + new Vector3(2, 0, 2); // À côté du vaisseau rouge
            if (IsPositionFree(redStarvengerPosition))
            {
                Instantiate(enemyStarvengerPrefab, redStarvengerPosition, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("Position pour le StarVenger rouge occupée par un mur.");
            }
        }
        else
        {
            Debug.LogWarning("Position pour le vaisseau rouge occupée par un mur.");
        }
    }

    // Fonction pour obtenir une position aléatoire valide
    Vector3 GetRandomPosition()
    {
        Vector3 randomPosition = new Vector3(Random.Range(0, width), 0, Random.Range(0, height));
        // S'assurer que la position aléatoire est valide (pas un mur)
        while (!IsPositionFree(randomPosition))
        {
            randomPosition = new Vector3(Random.Range(0, width), 0, Random.Range(0, height));
        }
        return randomPosition;
    }
}
