using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;


public class MapGenerator : NetworkBehaviour
{
    [Header("Paramètres de la carte")]
    public int mapWidth = 50;
    public int mapHeight = 50;

    [Header("Prefabs")]
    public GameObject wallPrefab;
    public NetworkPrefabRef characterPrefab;

    [Header("Paramètres des chemins")]
    public int corridorMinWidth = 3;
    public int corridorMaxWidth = 5;

    [Header("Placement des Objets")]
    public NetworkPrefabRef objectPrefab1;
    public NetworkPrefabRef objectPrefab2;
    public int clearRadius = 2;
    public float minDistanceBetweenObjects = 10f;

    [Header("Obstacles Naturels au Centre")]
    public bool enableCenterObstacles = true;
    public int centerRegionMargin = 10;
    public float obstacleNoiseScale = 0.1f;
    public float obstacleThreshold = 0.6f;

    private bool[,] map;
    public List<Vector2Int> floorCells = new List<Vector2Int>();

    // ------------------------------------------------------------------------------
    // Remplacez "Spawned()" (ancienne version) par "OnNetworkSpawn()" en Fusion 2
    // ------------------------------------------------------------------------------
    public override void Spawned()
    {
        // Seul le serveur (host) génère la map et spawne les objets
        if (Object.HasStateAuthority) // => runner.IsServer
        {
            GenerateMap();
            EnforceBorderWalls();

            if (enableCenterObstacles)
                CreateCenterObstacles();

            InstantiateWalls();
            PlaceObjects();

            Debug.Log("Carte générée par le serveur/host (OnNetworkSpawn).");
        }
    }

    // ------------------------------------------------------------------------------
    // Surcharger OnPlayerJoined : appelé AUTOMATIQUEMENT sur TOUTES les entités
    // NetworkBehaviour de la scène, mais seul le serveur va effectuer le Spawn.
    // ------------------------------------------------------------------------------
    

    // ------------------------------------------------------------------------------
    // Logique de génération de la carte
    // ------------------------------------------------------------------------------
    public void GenerateMap()
    {
        map = new bool[mapWidth, mapHeight];
        int totalCells = mapWidth * mapHeight;
        int targetFloorCount = (int)(totalCells * 0.7f);
        int floorCount = 0;

        Vector2Int currentPos = new Vector2Int(mapWidth / 2, mapHeight / 2);
        SetFloor(currentPos);
        floorCount++;

        while (floorCount < targetFloorCount)
        {
            int corridorWidth = Random.Range(corridorMinWidth, corridorMaxWidth + 1);
            int corridorLength = Random.Range(3, 10);
            Vector2Int direction = GetRandomDirection();

            for (int i = 0; i < corridorLength; i++)
            {
                if (!IsInBounds(currentPos))
                {
                    currentPos = floorCells[Random.Range(0, floorCells.Count)];
                    break;
                }

                for (int offset = -corridorWidth / 2; offset <= corridorWidth / 2; offset++)
                {
                    Vector2Int cell = direction.x != 0
                        ? new Vector2Int(currentPos.x, currentPos.y + offset)
                        : new Vector2Int(currentPos.x + offset, currentPos.y);

                    if (IsInBounds(cell) && !map[cell.x, cell.y])
                    {
                        SetFloor(cell);
                        floorCount++;
                    }
                }
                currentPos += direction;
            }

            if (floorCells.Count > 0 && Random.value < 0.3f)
            {
                currentPos = floorCells[Random.Range(0, floorCells.Count)];
            }
        }
    }

    public void EnforceBorderWalls()
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

        // Retirer toute cellule en bordure
        floorCells.RemoveAll(cell =>
            cell.x == 0 || cell.x == mapWidth - 1 ||
            cell.y == 0 || cell.y == mapHeight - 1);
    }

    void CreateCenterObstacles()
    {
        int centerX = mapWidth / 2;
        int centerY = mapHeight / 2;

        for (int x = centerX - centerRegionMargin; x <= centerX + centerRegionMargin; x++)
        {
            for (int y = centerY - centerRegionMargin; y <= centerY + centerRegionMargin; y++)
            {
                if (IsInBounds(new Vector2Int(x, y)) && map[x, y])
                {
                    float noise = Mathf.PerlinNoise(x * obstacleNoiseScale, y * obstacleNoiseScale);
                    if (noise > obstacleThreshold)
                    {
                        map[x, y] = false;
                        floorCells.RemoveAll(cell => cell.x == x && cell.y == y);
                    }
                }
            }
        }
    }

    Vector2Int GetRandomDirection()
    {
        switch (Random.Range(0, 4))
        {
            case 0: return new Vector2Int(1, 0);
            case 1: return new Vector2Int(-1, 0);
            case 2: return new Vector2Int(0, 1);
            default: return new Vector2Int(0, -1);
        }
    }

    bool IsInBounds(Vector2Int pos)
    {
        return (pos.x >= 0 && pos.x < mapWidth && pos.y >= 0 && pos.y < mapHeight);
    }

    void SetFloor(Vector2Int pos)
    {
        map[pos.x, pos.y] = true;
        floorCells.Add(pos);
    }

    void InstantiateWalls()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (!map[x, y])
                {
                    Vector3 position = new Vector3(x, 0, y);
                    Runner.Spawn(wallPrefab, position, Quaternion.identity);
                }
            }
        }
    }

    void PlaceObjects()
    {
        if (floorCells.Count < 2) return;

        Vector2Int pos1 = floorCells[Random.Range(0, floorCells.Count)];
        Vector2Int pos2 = floorCells[Random.Range(0, floorCells.Count)];

        // Uniquement le serveur spawne les objets en réseau
        if (Runner.IsServer)
        {
            Runner.Spawn(objectPrefab1, new Vector3(pos1.x, 1, pos1.y), Quaternion.identity);
            Runner.Spawn(objectPrefab2, new Vector3(pos2.x, 1, pos2.y), Quaternion.identity);
        }
    }
}














