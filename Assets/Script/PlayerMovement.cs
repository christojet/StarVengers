using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 10f;
    public Joystick joystick; // Pour le contrôle mobile

    void Update()
    {
        float moveX = joystick.Horizontal;
        float moveZ = joystick.Vertical;

        Vector3 movement = new Vector3(moveX, 0f, moveZ) * moveSpeed * Time.deltaTime;
        //transform.Translate(movement);
        transform.Translate(movement, Space.World); // Utilise l'espace monde

        // Orienter le personnage dans la direction du mouvement
        if (movement != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(movement);
        }
    }
}