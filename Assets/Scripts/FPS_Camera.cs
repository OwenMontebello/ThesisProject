using UnityEngine;

public class FPS_Camera : MonoBehaviour
{
    [Header("References")]
    public Transform playerBody;   //Main player object
    public float mouseSensitivity = 100f;

    float xRotation = 0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (GameManager.I.isWin) return;
        if (GameManager.I.isLose) return;

        //Mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        //Vertical rotation
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        //Horizontal rotation 
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
