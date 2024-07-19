using UnityEngine;

public class CameraWraparound : MonoBehaviour
{
    private Vector2 screenBounds;
    private float objectWidth;
    private float objectHeight;

    void Start()
    {
        // Calculating screen bounds
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
    }

    public void WrapAround(Transform obj)
    {
        Vector3 viewPos = obj.position;

        // Check and wrap on the x-axis
        if (viewPos.x > screenBounds.x + objectWidth)
        {
            viewPos.x = -screenBounds.x - objectWidth;
        }
        else if (viewPos.x < -screenBounds.x - objectWidth)
        {
            viewPos.x = screenBounds.x + objectWidth;
        }

        // Check and wrap on the y-axis
        if (viewPos.y > screenBounds.y + objectHeight)
        {
            viewPos.y = -screenBounds.y - objectHeight;
        }
        else if (viewPos.y < -screenBounds.y - objectHeight)
        {
            viewPos.y = screenBounds.y + objectHeight;
        }

        obj.position = viewPos;
    }
}
