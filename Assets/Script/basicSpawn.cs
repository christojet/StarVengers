using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner _runner;

    [SerializeField] private NetworkPrefabRef _playerPrefab;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    // Références aux éléments UI à assigner dans l'inspecteur
    public TMP_InputField numberInputField; // Champ pour saisir le numéro de session/host
    public Button hostButton;               // Bouton pour lancer en mode Host
    public Button clientButton;             // Bouton pour lancer en mode Client

    private void Awake()
    {
        // Assignation des événements de clic si ce n'est pas fait via l'inspecteur
        if (hostButton != null)
            hostButton.onClick.AddListener(OnHostButtonClicked);
        if (clientButton != null)
            clientButton.onClick.AddListener(OnClientButtonClicked);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            // Récupérer le MapGenerator présent dans la scène
            MapGenerator mapGenerator = GameObject.FindObjectOfType<MapGenerator>();
            if (mapGenerator == null || mapGenerator.floorCells.Count == 0)
            {
                Debug.LogError("MapGenerator non trouvé ou aucune cellule de sol disponible.");
                return;
            }

            // Choisir une cellule aléatoire qui est un sol (pas de mur)
            Vector2Int cell = mapGenerator.floorCells[UnityEngine.Random.Range(0, mapGenerator.floorCells.Count)];
            Vector3 spawnPosition = new Vector3(cell.x, 1, cell.y);

            // Spawn du joueur sur la position choisie
            NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
            _spawnedCharacters.Add(player, networkPlayerObject);

            // Si ce n'est pas le host (qui garde la couleur bleue par défaut), changer la couleur du matériau
            if (player != runner.LocalPlayer)
            {
                MeshRenderer renderer = networkPlayerObject.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.red;
                }
            }
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        // Input clavier
        if (Input.GetKey(KeyCode.W))
            data.direction += Vector3.forward;
        if (Input.GetKey(KeyCode.S))
            data.direction += Vector3.back;
        if (Input.GetKey(KeyCode.A))
            data.direction += Vector3.left;
        if (Input.GetKey(KeyCode.D))
            data.direction += Vector3.right;

        // Input du joystick
        GameObject joyObj = GameObject.FindGameObjectWithTag("JoyStick");
        if (joyObj != null)
        {
            Joystick joystick = joyObj.GetComponent<Joystick>();
            if (joystick != null)
            {
                data.direction += new Vector3(joystick.Horizontal, 0f, joystick.Vertical);
            }
        }

        if (data.direction != Vector3.zero)
            data.direction = data.direction.normalized;

        input.Set(data);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

    // Démarrage de la partie avec le mode et le nom de session personnalisé
    async void StartGame(GameMode mode, string sessionName)
    {
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid)
        {
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
        }

        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = sessionName,
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    // Méthode appelée lors du clic sur le bouton Host
    public void OnHostButtonClicked()
    {
        // Désactivation des boutons et du champ de saisie pour masquer l'UI
        if (hostButton != null)
            hostButton.gameObject.SetActive(false);
        if (clientButton != null)
            clientButton.gameObject.SetActive(false);
        if (numberInputField != null)
            numberInputField.gameObject.SetActive(false);

        string sessionNumber = numberInputField != null ? numberInputField.text : "";
        if (string.IsNullOrEmpty(sessionNumber))
            sessionNumber = "0"; // valeur par défaut si aucun numéro n'est saisi

        string sessionName = "TestRoom_" + sessionNumber;
        StartGame(GameMode.Host, sessionName);
    }

    // Méthode appelée lors du clic sur le bouton Client
    public void OnClientButtonClicked()
    {
        // Désactivation des boutons et du champ de saisie pour masquer l'UI
        if (hostButton != null)
            hostButton.gameObject.SetActive(false);
        if (clientButton != null)
            clientButton.gameObject.SetActive(false);
        if (numberInputField != null)
            numberInputField.gameObject.SetActive(false);

        string sessionNumber = numberInputField != null ? numberInputField.text : "";
        if (string.IsNullOrEmpty(sessionNumber))
            sessionNumber = "0"; // valeur par défaut si aucun numéro n'est saisi

        string sessionName = "TestRoom_" + sessionNumber;
        StartGame(GameMode.Client, sessionName);
    }
}



