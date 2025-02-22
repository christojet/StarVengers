using Fusion;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // si besoin de .Count()

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [Header("Référence au MapGenerator")]
    public MapGenerator mapGenerator;

    [Header("Prefab du joueur (NetworkPrefabRef)")]
    public GameObject PlayerPrefab;

    [Header("Paramètres de la zone de spawn de secours")]
    public float spawnAreaSize = 200f; // Taille de la zone de spawn "fallback"
    public int maxAttempts = 100;      // Nombre maximum de tentatives pour éviter un mur

    // Appelé quand un joueur rejoint la session
    public void PlayerJoined(PlayerRef player)
    {
        // Seul le Serveur/Hôte exécute le Spawn
        if (Runner.IsServer)
        {
            StartCoroutine(SpawnPlayerAfterDelay(player, 2f));
        }
    }

    private IEnumerator SpawnPlayerAfterDelay(PlayerRef player, float delay)
    {
        yield return new WaitForSeconds(delay);

        Vector3 spawnPos = GetSpawnPosition();
        Quaternion spawnRot = Quaternion.identity;

        // IMPORTANT : en passant "player" en 4e paramètre de Spawn, 
        // on lui donne "InputAuthority" => client authority.
        Runner.Spawn(PlayerPrefab, spawnPos, spawnRot, player);
    }

    /// <summary>
    /// Essaie de trouver une cellule de sol (via floorCells).
    /// Sinon, renvoie une position aléatoire libre de murs.
    /// </summary>
    private Vector3 GetSpawnPosition()
    {
        // Tente de récupérer une cellule "floor" depuis le mapGenerator
        var floorCells = mapGenerator?.floorCells;
        if (floorCells != null && floorCells.Count > 0)
        {
            var randomCell = floorCells[Random.Range(0, floorCells.Count)];
            return new Vector3(randomCell.x, 1f, randomCell.y);
        }

        // Sinon, on cherche un spot aléatoire sans mur
        return FindValidSpawnPosition();
    }

    private Vector3 FindValidSpawnPosition()
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            float x = Random.Range(-spawnAreaSize / 2, spawnAreaSize / 2);
            float z = Random.Range(-spawnAreaSize / 2, spawnAreaSize / 2);
            Vector3 potentialSpawn = new Vector3(x, 1f, z);

            // Vérifier s'il y a un mur (layer "Mur") dans un rayon de 1 autour de la position
            if (!Physics.CheckSphere(potentialSpawn, 1f, LayerMask.GetMask("Mur")))
            {
                return potentialSpawn;
            }
        }

        Debug.LogWarning("Impossible de trouver un spawn sans Mur après plusieurs tentatives.");
        // Dernier recours si aucune position valide n'a été trouvée
        return new Vector3(100, 1f, 100);
    }
}


