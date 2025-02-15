using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 10f;
    public Joystick joystick; // Pour le contrôle mobile

    void Update()
    {
        // Recherche de l'objet tagué "JoyStick" si le joystick n'est pas déjà assigné
        if (joystick == null)
        {
            GameObject joyObj = GameObject.FindGameObjectWithTag("JoyStick");
            if (joyObj != null)
            {
                joystick = joyObj.GetComponent<Joystick>();
            }
        }

        // Si le joystick a été trouvé, on gère le déplacement du joueur
        if (joystick != null)
        {
            float moveX = joystick.Horizontal;
            float moveZ = joystick.Vertical;

            Vector3 movement = new Vector3(moveX, 0f, moveZ) * moveSpeed * Time.deltaTime;
            transform.Translate(movement, Space.World);

            // Orientation du personnage dans la direction du mouvement
            if (movement != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(movement);
            }
        }
    }
}