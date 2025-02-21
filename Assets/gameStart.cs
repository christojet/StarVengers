using UnityEngine;
using Fusion;

public class LabyrinthPlayerSpawner : NetworkBehaviour
{
    [Header("Référence au MapGenerator")]
    public MapGenerator mapGenerator;

    [Header("Prefab du joueur (NetworkPrefabRef)")]
    public NetworkPrefabRef characterPrefab;

    //-------------------------------------------------------------------------
    // Exemple : Appeler cette méthode depuis un autre script ou un bouton UI
    //-------------------------------------------------------------------------
    public void SpawnPlayerManually(PlayerRef playerRef)
    {
        // 1) Seul le serveur/host peut spawner des objets en réseau
        if (!Runner.IsServer)
        {
            Debug.LogWarning("Tentative de spawn alors que ce n'est pas le serveur !");
            return;
        }

        // 2) Vérifier qu'on a des cellules disponibles
        var floorCells = mapGenerator.floorCells;
        if (floorCells == null || floorCells.Count == 0)
        {
            Debug.LogWarning("Aucune cellule praticable dans le labyrinthe !");
            return;
        }

        // 3) Choisir une cellule aléatoire
        var randomCell = floorCells[Random.Range(0, floorCells.Count)];
        Vector3 spawnPos = new Vector3(randomCell.x, 1f, randomCell.y);

        // 4) Spawner le joueur
        Debug.Log($"Spawning player {playerRef} at {spawnPos}");
        Runner.Spawn(characterPrefab, spawnPos, Quaternion.identity, playerRef);
    }
}

