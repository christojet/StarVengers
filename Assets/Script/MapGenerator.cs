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

    [Header("Paramètres des chemins")]
    public int corridorMinWidth = 3;
    public int corridorMaxWidth = 5;

    // Tableau pour stocker la carte (true = sol, false = mur)
    private bool[,] map;
    // Liste des cellules qui sont devenues du sol (pour choisir des points de départ)
    private List<Vector2Int> floorCells = new List<Vector2Int>();

    void Start()
    {
        GenerateMap();
        EnforceBorderWalls();
        InstantiateWalls();
        SpawnCharacter();
    }

    // Génère la carte en creusant des chemins dans une zone initialement remplie de murs
    void GenerateMap()
    {
        map = new bool[mapWidth, mapHeight];
        int totalCells = mapWidth * mapHeight;
        int targetFloorCount = (int)(totalCells * 0.4f); // Par exemple 40% de sols
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

    // Force la création d'un mur sur toute la bordure de la carte
    void EnforceBorderWalls()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            map[x, 0] = false;
            map[x, mapHeight - 1] = false;
        }
        for (int y = 0; y < mapHeight; y++)
        {
            map[0, y] = false;
            map[mapWidth - 1, y] = false;
        }
        // Supprime les cellules de bordure de la liste des sols accessibles
        floorCells.RemoveAll(cell => cell.x == 0 || cell.x == mapWidth - 1 || cell.y == 0 || cell.y == mapHeight - 1);
    }

    // Retourne une direction aléatoire parmi les 4 directions principales
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

    // Instancie les murs sur les cellules qui ne sont pas du sol
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

    // Repositionne le personnage sur une cellule accessible
    void SpawnCharacter()
    {
        if (floorCells.Count > 0)
        {
            // Choisir aléatoirement une cellule accessible
            Vector2Int pos = floorCells[Random.Range(0, floorCells.Count)];
            Vector3 spawnPos = new Vector3(pos.x, 1, pos.y); // Ajuster la hauteur (y) si nécessaire

            // Vérifier si un personnage existe déjà dans la scène (doit avoir le tag "Player")
            GameObject existingCharacter = GameObject.FindWithTag("Player");
            if (existingCharacter != null)
            {
                existingCharacter.transform.position = spawnPos;
            }
            else
            {
                // Sinon, instancier le personnage à partir du prefab
                Instantiate(characterPrefab, spawnPos, Quaternion.identity);
            }
        }
        else
        {
            Debug.LogWarning("Aucune cellule accessible pour repositionner le personnage !");
        }
    }
}