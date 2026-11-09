using UnityEngine;

public class TowerPlacement : MonoBehaviour
{
    public GameObject sprigatitoPrefab;
    public Camera mainCamera;

    void Update()
    {
        // Check for left mouse click
        if (Input.GetMouseButtonDown(0))
        {
            // Shoot a laser from the camera to where the mouse is pointing
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            
            // If the laser hits a 3D object...
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // ...and that object is our grass plane...
                if (hit.collider.CompareTag("Off_Track"))
                {
                    // Place the tower at the exact point the laser hit!
                    Instantiate(sprigatitoPrefab, hit.point, Quaternion.identity);
                }
            }
        }
    }
}