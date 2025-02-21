using Fusion;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 10f;
    private Joystick joystick;
    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Recherche du joystick si non assigné
        if (joystick == null)
        {
            GameObject joyObj = GameObject.FindGameObjectWithTag("JoyStick");
            if (joyObj != null)
            {
                joystick = joyObj.GetComponent<Joystick>();
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasInputAuthority) return; // Vérifie que c'est bien le joueur local

        if (joystick != null)
        {
            float moveX = joystick.Horizontal;
            float moveZ = joystick.Vertical;

            Vector3 movement = new Vector3(moveX, 0f, moveZ) * moveSpeed * Runner.DeltaTime;
            controller.Move(movement);

            // Orientation du personnage dans la direction du mouvement
            if (movement != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(movement);
            }
        }
    }
}


