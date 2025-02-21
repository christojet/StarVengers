using UnityEngine;
using System.Collections.Generic;

public class LOLMapGenerator : MonoBehaviour
{
    [Header("Paramètres de la carte")]
    public int mapWidth = 50;
    public int mapHeight = 50;

    [Header("Prefabs")]
    public GameObject wallPrefab;
    public GameObject characterPrefab;
    public GameObject objectPrefab1;
    public GameObject objectPrefab2;

    [Header("Paramètres des voies (lanes)")]
    [Tooltip("Largeur minimale d'une voie (en cases)")]
    public int minPathWidth = 3;
    [Tooltip("Largeur maximale d'une voie (en cases)")]
    public int maxPathWidth = 5;
    [Tooltip("Nombre de segments pour définir la forme de la voie")]
    public int pathSegmentCount = 20;
    [Tooltip("Amplitude maximale du décalage aléatoire sur chaque segment")]
    public float randomOffset = 3f;

    // Tableau qui représente la carte : true = voie (accessible), false = mur/obstacle
    private bool[,] map;
    // Liste des cellules accessibles (issues des voies) pour le spawn du personnage et le placement d'objets
    private List<Vector2Int> floorCells = new List<Vector2Int>();

    void Start()
    {
        // Initialisation de la carte remplie d'obstacles (false)
        map = new bool[mapWidth, mapHeight];
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                map[x, y] = false;
            }
        }

        // On "creuse" trois voies principales
        // Par exemple :
        // - Top lane : de (0, mapHeight-1) vers (mapWidth-1, 2/3 de la hauteur)
        // - Mid lane : de (mapWidth/4, 0) vers (mapWidth/2, mapHeight-1)
        // - Bot lane : de (mapWidth-1, 0) vers (mapWidth/4, 1/3 de la hauteur)
        CarvePath(new Vector2Int(0, mapHeight - 1), new Vector2Int(mapWidth - 1, Mathf.RoundToInt(mapHeight * 2f / 3f)));
        CarvePath(new Vector2Int(mapWidth / 4, 0), new Vector2Int(mapWidth / 2, mapHeight - 1));
        CarvePath(new Vector2Int(mapWidth - 1, 0), new Vector2Int(mapWidth / 4, Mathf.RoundToInt(mapHeight / 3f)));

        // Mise à jour de la liste des cellules accessibles
        UpdateFloorCells();

        // Instanciation des murs sur les cellules non "creusées"
        InstantiateWalls();

        // Repositionnement (ou instanciation) du personnage
        SpawnCharacter();

        // Placement de deux objets dans des zones dégagées et éloignées l'un de l'autre
        PlaceObjects();
    }

    /// <summary>
    /// Creuse une voie entre deux points en générant une polyligne avec des déviations aléatoires,
    /// puis en creusant un corridor autour de cette ligne.
    /// </summary>
    void CarvePath(Vector2Int start, Vector2Int end)
    {
        List<Vector2> points = new List<Vector2>();
        points.Add(start);

        // Création de points intermédiaires par interpolation linéaire + décalage aléatoire
        for (int i = 1; i < pathSegmentCount; i++)
        {
            float t = i / (float)pathSegmentCount;
            float x = Mathf.Lerp(start.x, end.x, t);
            float y = Mathf.Lerp(start.y, end.y, t);
            x += Random.Range(-randomOffset, randomOffset);
            y += Random.Range(-randomOffset, randomOffset);
            points.Add(new Vector2(x, y));
        }
        points.Add(end);

        // Pour chaque segment de la polyligne, creuser un corridor d'une largeur aléatoire
        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector2 p0 = points[i];
            Vector2 p1 = points[i + 1];
            int pathWidth = Random.Range(minPathWidth, maxPathWidth + 1);
            CarveCorridor(p0, p1, pathWidth);
        }
    }

    /// <summary>
    /// Creuse un corridor le long de la droite reliant deux points, en creusant un cercle (de rayon 'width') à chaque étape.
    /// </summary>
    void CarveCorridor(Vector2 p0, Vector2 p1, int width)
    {
        float dist = Vector2.Distance(p0, p1);
        int steps = Mathf.CeilToInt(dist);
        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            float x = Mathf.Lerp(p0.x, p1.x, t);
            float y = Mathf.Lerp(p0.y, p1.y, t);
            Vector2Int cell = new Vector2Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
            CarveCircle(cell, width);
        }
    }

    /// <summary>
    /// Creuse (met à true) toutes les cellules dans un cercle de rayon donné autour d'une cellule centrale.
    /// </summary>
    void CarveCircle(Vector2Int center, int radius)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector2Int pos = new Vector2Int(center.x + x, center.y + y);
                if (pos.x >= 0 && pos.x < mapWidth && pos.y >= 0 && pos.y < mapHeight)
                {
                    if (x * x + y * y <= radius * radius)
                    {
                        map[pos.x, pos.y] = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Met à jour la liste des cellules accessibles (floorCells) en fonction du tableau map.
    /// </summary>
    void UpdateFloorCells()
    {
        floorCells.Clear();
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (map[x, y])
                {
                    floorCells.Add(new Vector2Int(x, y));
                }
            }
        }
    }

    /// <summary>
    /// Instancie les murs (prefab) sur toutes les cellules où map vaut false.
    /// </summary>
    void InstantiateWalls()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (!map[x, y])
                {
                    Vector3 pos = new Vector3(x, 0, y);
                    Instantiate(wallPrefab, pos, Quaternion.identity, transform);
                }
            }
        }
    }

    /// <summary>
    /// Repositionne (ou instancie) le personnage sur une cellule accessible choisie aléatoirement.
    /// </summary>
    void SpawnCharacter()
    {
        if (floorCells.Count > 0)
        {
            Vector2Int pos = floorCells[Random.Range(0, floorCells.Count)];
            Vector3 spawnPos = new Vector3(pos.x, 1, pos.y);
            GameObject existing = GameObject.FindWithTag("Player");
            if (existing != null)
            {
                existing.transform.position = spawnPos;
            }
            else
            {
                Instantiate(characterPrefab, spawnPos, Quaternion.identity);
            }
        }
        else
        {
            Debug.LogWarning("Aucune cellule accessible pour le spawn du personnage !");
        }
    }

    /// <summary>
    /// Place deux objets (prefabs) dans des zones dégagées et suffisamment éloignées l'une de l'autre.
    /// </summary>
    void PlaceObjects()
    {
        if (floorCells.Count == 0)
        {
            Debug.LogWarning("Aucune cellule accessible pour placer les objets.");
            return;
        }

        // Mélanger la liste des cellules accessibles
        List<Vector2Int> shuffled = new List<Vector2Int>(floorCells);
        for (int i = 0; i < shuffled.Count; i++)
        {
            Vector2Int temp = shuffled[i];
            int j = Random.Range(i, shuffled.Count);
            shuffled[i] = shuffled[j];
            shuffled[j] = temp;
        }

        // Recherche d'une zone dégagée pour le premier objet
        bool foundPos1 = false;
        Vector2Int pos1 = Vector2Int.zero;
        foreach (Vector2Int cell in shuffled)
        {
            if (IsClearArea(cell, 2))
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

        // Recherche d'une zone dégagée pour le deuxième objet, éloignée du premier
        bool foundPos2 = false;
        Vector2Int pos2 = Vector2Int.zero;
        foreach (Vector2Int cell in shuffled)
        {
            if (IsClearArea(cell, 2) && Vector2.Distance(cell, pos1) > 10f)
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

        Vector3 spawnPos1 = new Vector3(pos1.x, 1, pos1.y);
        Vector3 spawnPos2 = new Vector3(pos2.x, 1, pos2.y);
        Instantiate(objectPrefab1, spawnPos1, Quaternion.identity);
        Instantiate(objectPrefab2, spawnPos2, Quaternion.identity);
    }

    /// <summary>
    /// Vérifie si toutes les cellules dans un carré de côté (2*radius+1) autour d'une cellule sont accessibles.
    /// </summary>
    bool IsClearArea(Vector2Int cell, int radius)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector2Int pos = new Vector2Int(cell.x + x, cell.y + y);
                if (pos.x < 0 || pos.x >= mapWidth || pos.y < 0 || pos.y >= mapHeight)
                    return false;
                if (!map[pos.x, pos.y])
                    return false;
            }
        }
        return true;
    }
}






