using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public NetworkRunner networkRunnerPrefab;

    void Start()
    {
        StartGame();
    }

    async void StartGame()
    {
        NetworkRunner runner = Instantiate(networkRunnerPrefab);
        await runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.AutoHostOrClient,
            SessionName = "GameRoom",
            SceneManager = runner.GetComponent<NetworkSceneManagerDefault>()
        });
    }
}

