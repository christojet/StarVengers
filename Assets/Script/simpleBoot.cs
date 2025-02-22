using Fusion;
using Fusion.Sockets;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SimpleFusionBootstrap : MonoBehaviour
{
    [Header("Runner & UI")]
    public NetworkRunner RunnerPrefab;
    public TMP_InputField CodeInputField; // Champ pour saisir le code/nom de session
    public Button HostButton; // Bouton pour démarrer en Host
    public Button ClientButton; // Bouton pour démarrer en Client

    [Header("Paramètres Réseau")]
    public string DefaultRoomName = "DefaultRoom";

    void Start()
    {
        if (HostButton != null)
            HostButton.onClick.AddListener(StartHost);
        if (ClientButton != null)
            ClientButton.onClick.AddListener(StartClient);
    }

    public void StartHost()
    {
        string roomCode = GetRoomCode();
        StartCoroutine(StartGameRoutine(GameMode.Host, roomCode));
    }

    public void StartClient()
    {
        string roomCode = GetRoomCode();
        StartCoroutine(StartGameRoutine(GameMode.Client, roomCode));
    }

    private string GetRoomCode()
    {
        return (CodeInputField != null && !string.IsNullOrEmpty(CodeInputField.text))
            ? CodeInputField.text
            : DefaultRoomName;
    }

    private void HideUI()
    {
        if (HostButton != null) HostButton.gameObject.SetActive(false);
        if (ClientButton != null) ClientButton.gameObject.SetActive(false);
        if (CodeInputField != null) CodeInputField.gameObject.SetActive(false);
    }

    IEnumerator StartGameRoutine(GameMode gameMode, string roomName)
    {
        // Masquer l'UI avant de commencer la connexion
        HideUI();

        // Instanciation du Runner depuis le prefab
        NetworkRunner runner = Instantiate(RunnerPrefab);
        DontDestroyOnLoad(runner.gameObject);
        runner.name = gameMode.ToString();

        if (runner.GetComponent<INetworkSceneManager>() == null)
            runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
        if (runner.GetComponent<INetworkObjectProvider>() == null)
            runner.gameObject.AddComponent<NetworkObjectProviderDefault>();

        Scene activeScene = SceneManager.GetActiveScene();
        SceneRef sceneRef = SceneRef.FromIndex(activeScene.buildIndex);
        NetworkSceneInfo sceneInfo = new NetworkSceneInfo();
        sceneInfo.AddSceneRef(sceneRef, LoadSceneMode.Single);

        StartGameArgs args = new StartGameArgs()
        {
            GameMode = gameMode,
            SessionName = roomName,
            Scene = sceneInfo,
            Address = default
        };

        Task startTask = runner.StartGame(args);
        while (!startTask.IsCompleted)
            yield return null;

        if (startTask.IsFaulted)
        {
            Debug.LogError("Erreur lors du démarrage du jeu : " + startTask.Exception);
        }
        else
        {
            Debug.Log("Jeu démarré en mode " + gameMode + " avec la session : " + roomName);
        }
    }
}



