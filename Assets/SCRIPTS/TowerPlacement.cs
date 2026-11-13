using UnityEngine;

public class TowerPlacement : MonoBehaviour
{
    public GameObject sprigatitoPrefab;
    public Camera mainCamera;
    
    public float towerRadius = 1f; 
    public LayerMask blockingLayers; 

    [Header("Preview Settings")]
    public GameObject previewPrefab; 
    private GameObject currentPreview;
    private Renderer previewRenderer;

    // Semi-transparent Green and Red
    public Color validColor = new Color(0, 1, 0, 0.5f); 
    public Color invalidColor = new Color(1, 0, 0, 0.5f);

    void Start()
    {
        // Spawn the universal preview once at the start and hide it
        currentPreview = Instantiate(previewPrefab);
        previewRenderer = currentPreview.GetComponentInChildren<Renderer>();
        currentPreview.SetActive(false);
    }

    void Update()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        // QueryTriggerInteraction.Ignore forces the ray to pass through attack range bubbles
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            currentPreview.SetActive(true);
            currentPreview.transform.position = hit.point;

            // 1. Is it on the grass? (Using your Off_Track tag)
            bool isValidTerrain = hit.collider.CompareTag("Off_Track");
            
            // 2. Is it physically clear of the track and other towers?
            bool isClear = !Physics.CheckSphere(hit.point, towerRadius, blockingLayers, QueryTriggerInteraction.Ignore);
            
            bool canPlace = isValidTerrain && isClear;

            // Change the ghost color based on the checks
            if (canPlace)
                previewRenderer.material.color = validColor;
            else
                previewRenderer.material.color = invalidColor;

            // Place the tower if we click AND it's a valid spot
            if (Input.GetMouseButtonDown(0) && canPlace)
            {
                Instantiate(sprigatitoPrefab, hit.point, Quaternion.identity);
            }
        }
        else
        {
            // Hide the preview if hovering off into the void
            currentPreview.SetActive(false);
        }
    }
}