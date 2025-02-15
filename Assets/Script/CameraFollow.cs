using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Référence au StarVenger
    public Vector3 offset = new Vector3(0, 10, -5); // Position relative de la caméra

    void LateUpdate()
    {
        if (target != null)
        {
            // Suivre uniquement le StarVenger en ZX (sans rotation)
            transform.position = new Vector3(target.position.x, transform.position.y, target.position.z) + offset;
        }
    }
}
