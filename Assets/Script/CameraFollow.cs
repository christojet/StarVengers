using Fusion;
using UnityEngine;

public class SuiviPlayer : NetworkBehaviour
{
    private Transform player;
    public Vector3 offset = new Vector3(0, 10, 0);

    void Start()
    {
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        FindLocalPlayer();
    }

    void LateUpdate()
    {
        if (player == null)
        {
            FindLocalPlayer();
        }

        if (player != null)
        {
            transform.position = player.position + offset;
        }
    }

    void FindLocalPlayer()
    {
        foreach (var networkObject in FindObjectsOfType<NetworkObject>())
        {
            if (networkObject.CompareTag("Player") && networkObject.HasInputAuthority)
            {
                player = networkObject.transform;
                break;
            }
        }
    }
}



