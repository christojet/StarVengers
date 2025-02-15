using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    [Header("Paramètres de la carte")]
    public int mapWidth = 50;
    public int mapHeight = 50;

    [Header("Prefabs")]
    public GameObject wallPrefab;
    public GameObject characterPrefab;
    public GameObject AI; 

    [Header("Paramètres des chemins")]
    public int corridorMinWidth = 3;
    public int corridorMaxWidth = 5;

    [Header("Placement des Objets")]
    public GameObject objectPrefab1;
    public GameObject objectPrefab2;
    [Tooltip("Rayon autour d'une cellule pour vérifier qu'elle est bien dégagée.")]
    public int clearRadius = 2;
    [Tooltip("Distance minimale entre les deux objets placés.")]
    public float minDistanceBetweenObjects = 10f;

    [Header("Obstacles Naturels au Centre")]
    [Tooltip("Active ou désactive l'ajout d'obstacles naturels dans la région centrale.")]
    public bool enableCenterObstacles = true;
    [Tooltip("Définit l'étendue (en cellules) autour du centre dans laquelle placer des obstacles.")]
    public int centerRegionMargin = 10;
    [Tooltip("Échelle appliquée aux coordonnées pour le bruit de Perlin.")]
    public float obstacleNoiseScale = 0.1f;
    [Tooltip("Seuil du bruit pour convertir une cellule en mur (valeur entre 0 et 1).")]
    public float obstacleThreshold = 0.6f;

    // Tableau de la carte : true = sol, false = mur
    private bool[,] map;
    // Liste des cellules accessibles (sols) pour le spawn et le placement d'objets
    private List<Vector2Int> floorCells = new List<Vector2Int>();

    void Start()
    {
        GenerateMap();
        EnforceBorderWalls();
        if (enableCenterObstacles)
            CreateCenterObstacles();
        InstantiateWalls();
        SpawnCharacter();
        SpawnCharacter2();
        PlaceObjects();
    }

    // Génère la carte en creusant des chemins dans une zone initialement remplie de murs
    void GenerateMap()
    {
        map = new bool[mapWidth, mapHeight];
        int totalCells = mapWidth * mapHeight;
        int targetFloorCount = (int)(totalCells * 0.7f); // Par exemple, 40% de sols
        int floorCount = 0;

        // Démarrage au centre de la carte
        Vector2Int currentPos = new Vector2Int(mapWidth / 2, mapHeight / 2);
        SetFloor(currentPos);
        floorCount++;

        // Algorithme du "drunkard's walk"
        while (floorCount < targetFloorCount)
        {
            int corridorWidth = Random.Range(corridorMinWidth, corridorMaxWidth + 1);
            int corridorLength = Random.Range(3, 10);
            Vector2Int direction = GetRandomDirection();

            for (int i = 0; i < corridorLength; i++)
            {
                if (direction.x != 0) // chemin horizontal
                {
                    for (int offset = -corridorWidth / 2; offset <= corridorWidth / 2; offset++)
                    {
                        Vector2Int cell = new Vector2Int(currentPos.x, currentPos.y + offset);
                        if (IsInBounds(cell) && !map[cell.x, cell.y])
                        {
                            SetFloor(cell);
                            floorCount++;
                        }
                    }
                }
                else if (direction.y != 0) // chemin vertical
                {
                    for (int offset = -corridorWidth / 2; offset <= corridorWidth / 2; offset++)
                    {
                        Vector2Int cell = new Vector2Int(currentPos.x + offset, currentPos.y);
                        if (IsInBounds(cell) && !map[cell.x, cell.y])
                        {
                            SetFloor(cell);
                            floorCount++;
                        }
                    }
                }

                currentPos += direction;
                if (!IsInBounds(currentPos))
                {
                    currentPos = floorCells[Random.Range(0, floorCells.Count)];
                    break;
                }
            }

            if (floorCells.Count > 0 && Random.value < 0.3f)
            {
                currentPos = floorCells[Random.Range(0, floorCells.Count)];
            }
        }
    }

    // Force la création de murs sur la bordure de la carte et retire ces cellules de la liste des sols
    void EnforceBorderWalls()
    {
        // Bordures horizontales
        for (int x = 0; x < mapWidth; x++)
        {
            map[x, 0] = false;
            map[x, mapHeight - 1] = false;
        }
        // Bordures verticales
        for (int y = 0; y < mapHeight; y++)
        {
            map[0, y] = false;
            map[mapWidth - 1, y] = false;
        }
        // Retirer les cellules de bordure de la liste des sols accessibles
        floorCells.RemoveAll(cell => cell.x == 0 || cell.x == mapWidth - 1 || cell.y == 0 || cell.y == mapHeight - 1);
    }

    // Crée des obstacles naturels dans une région centrale définie à l'aide du bruit de Perlin
    void CreateCenterObstacles()
    {
        int centerX = mapWidth / 2;
        int centerY = mapHeight / 2;
        int startX = Mathf.Max(0, centerX - centerRegionMargin);
        int endX = Mathf.Min(mapWidth - 1, centerX + centerRegionMargin);
        int startY = Mathf.Max(0, centerY - centerRegionMargin);
        int endY = Mathf.Min(mapHeight - 1, centerY + centerRegionMargin);

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                // Si c'est actuellement un sol
                if (map[x, y])
                {
                    // Calcul du bruit de Perlin pour cette cellule
                    float noise = Mathf.PerlinNoise(x * obstacleNoiseScale, y * obstacleNoiseScale);
                    if (noise > obstacleThreshold)
                    {
                        // Transformer la cellule en mur
                        map[x, y] = false;
                        // Retirer la cellule de la liste des sols accessibles
                        floorCells.RemoveAll(cell => cell.x == x && cell.y == y);
                    }
                }
            }
        }
    }

    // Retourne une direction aléatoire parmi les 4 directions principales (haut, bas, gauche, droite)
    Vector2Int GetRandomDirection()
    {
        int rand = Random.Range(0, 4);
        switch (rand)
        {
            case 0: return new Vector2Int(1, 0);    // Droite
            case 1: return new Vector2Int(-1, 0);   // Gauche
            case 2: return new Vector2Int(0, 1);    // Haut
            default: return new Vector2Int(0, -1);  // Bas
        }
    }

    // Vérifie si une position est dans les limites de la carte
    bool IsInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < mapWidth && pos.y >= 0 && pos.y < mapHeight;
    }

    // Marque une cellule comme sol et l'ajoute à la liste des cellules accessibles
    void SetFloor(Vector2Int pos)
    {
        map[pos.x, pos.y] = true;
        floorCells.Add(pos);
    }

    // Instancie les murs (à partir du prefab) sur toutes les cellules marquées comme mur
    void InstantiateWalls()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (!map[x, y])
                {
                    Vector3 position = new Vector3(x, 0, y);
                    Instantiate(wallPrefab, position, Quaternion.identity, transform);
                }
            }
        }
    }

    // Repositionne (ou instancie) le personnage sur une cellule accessible à l'intérieur de la carte
    void SpawnCharacter()
    {
        if (floorCells.Count > 0)
        {
            Vector2Int pos = floorCells[Random.Range(0, floorCells.Count)];
            Vector3 spawnPos = new Vector3(pos.x, 1, pos.y);

            // Si un personnage existe déjà (doit avoir le tag "Player"), on le repositionne
            GameObject existingCharacter = GameObject.FindWithTag("Player");
            if (existingCharacter != null)
            {
                existingCharacter.transform.position = spawnPos;
            }
            else
            {
                Instantiate(characterPrefab, spawnPos, Quaternion.identity);
            }
        }
        else
        {
            Debug.LogWarning("Aucune cellule accessible pour repositionner le personnage !");
        }
    }
    void SpawnCharacter2()
    {
        if (floorCells.Count > 0)
        {
            Vector2Int pos = floorCells[Random.Range(0, floorCells.Count)];
            Vector3 spawnPos = new Vector3(pos.x, 1, pos.y);

            // Si un personnage existe déjà (doit avoir le tag "Player"), on le repositionne
            GameObject existingCharacter = GameObject.FindWithTag("AI");
            if (existingCharacter != null)
            {
                existingCharacter.transform.position = spawnPos;
            }
            else
            {
                Instantiate(AI, spawnPos, Quaternion.identity);
            }
        }
        else
        {
            Debug.LogWarning("Aucune cellule accessible pour repositionner le personnage !");
        }
    }

    // Vérifie si la zone autour d'une position donnée est suffisamment dégagée
    bool IsClearArea(Vector2Int pos, int radius)
    {
        for (int x = pos.x - radius; x <= pos.x + radius; x++)
        {
            for (int y = pos.y - radius; y <= pos.y + radius; y++)
            {
                Vector2Int checkPos = new Vector2Int(x, y);
                // Si la position est hors limites ou si c'est un mur, la zone n'est pas dégagée
                if (!IsInBounds(checkPos) || !map[x, y])
                {
                    return false;
                }
            }
        }
        return true;
    }

    // Place deux objets dans des zones dégagées et suffisamment éloignées l'un de l'autre
    void PlaceObjects()
    {
        if (floorCells.Count == 0)
        {
            Debug.LogWarning("Aucune cellule de sol disponible pour placer les objets.");
            return;
        }

        // Créer une copie de floorCells et la mélanger
        List<Vector2Int> shuffledCells = new List<Vector2Int>(floorCells);
        for (int i = 0; i < shuffledCells.Count; i++)
        {
            Vector2Int temp = shuffledCells[i];
            int randomIndex = Random.Range(i, shuffledCells.Count);
            shuffledCells[i] = shuffledCells[randomIndex];
            shuffledCells[randomIndex] = temp;
        }

        // Chercher une cellule dégagée pour le premier objet
        bool foundPos1 = false;
        Vector2Int pos1 = Vector2Int.zero;
        foreach (Vector2Int cell in shuffledCells)
        {
            if (IsClearArea(cell, clearRadius))
            {
                pos1 = cell;
                foundPos1 = true;
                break;
            }
        }
        if (!foundPos1)
        {
            Debug.LogWarning("Aucune zone dégagée trouvée pour le premier objet.");
            return;
        }

        // Chercher une cellule dégagée pour le deuxième objet, éloignée du premier
        bool foundPos2 = false;
        Vector2Int pos2 = Vector2Int.zero;
        foreach (Vector2Int cell in shuffledCells)
        {
            if (IsClearArea(cell, clearRadius) &&
                Vector2.Distance(new Vector2(cell.x, cell.y), new Vector2(pos1.x, pos1.y)) >= minDistanceBetweenObjects)
            {
                pos2 = cell;
                foundPos2 = true;
                break;
            }
        }
        if (!foundPos2)
        {
            Debug.LogWarning("Aucune zone dégagée trouvée pour le deuxième objet avec une distance suffisante.");
            return;
        }

        // Instancier les deux objets aux positions trouvées (ajustez la hauteur si besoin)
        Vector3 spawnPos1 = new Vector3(pos1.x, 1, pos1.y);
        Vector3 spawnPos2 = new Vector3(pos2.x, 1, pos2.y);
        Instantiate(objectPrefab1, spawnPos1, Quaternion.identity);
        Instantiate(objectPrefab2, spawnPos2, Quaternion.identity);
    }
}









