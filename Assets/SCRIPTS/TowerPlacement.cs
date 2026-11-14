using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required to stop clicking through the UI

public class TowerPlacement : MonoBehaviour
{
    public enum BuildMode { None, BuildSprigatito, Remove }
    public BuildMode currentMode = BuildMode.None;

    [Header("Prefabs & Settings")]
    public GameObject sprigatitoPrefab;
    public Camera mainCamera;
    public float towerRadius = 1f; 
    public LayerMask blockingLayers; 

    [Header("Preview Settings")]
    public GameObject previewPrefab; 
    private GameObject currentPreview;
    private Renderer previewRenderer;
    public Color validColor = new Color(0, 1, 0, 0.5f); 
    public Color invalidColor = new Color(1, 0, 0, 0.5f);

    [Header("UI Buttons")]
    public Image sprigatitoButton;
    public Image removeButton;
    public Color defaultColor = Color.white;
    public Color highlightColor = Color.yellow;

    void Start()
    {
        currentPreview = Instantiate(previewPrefab);
        previewRenderer = currentPreview.GetComponentInChildren<Renderer>();
        currentPreview.SetActive(false);
    }

    // Called by the UI Buttons
    public void ToggleSprigatitoMode()
    {
        SetMode(currentMode == BuildMode.BuildSprigatito ? BuildMode.None : BuildMode.BuildSprigatito);
    }

    public void ToggleRemoveMode()
    {
        SetMode(currentMode == BuildMode.Remove ? BuildMode.None : BuildMode.Remove);
    }

    private void SetMode(BuildMode newMode)
    {
        currentMode = newMode;
        
        // Highlight the active button
        sprigatitoButton.color = (currentMode == BuildMode.BuildSprigatito) ? highlightColor : defaultColor;
        removeButton.color = (currentMode == BuildMode.Remove) ? highlightColor : defaultColor;
        
        if (currentMode == BuildMode.None) currentPreview.SetActive(false);
    }

    void Update()
    {
        // Cancel if we aren't in a mode OR if the mouse is hovering over a UI element!
        if (currentMode == BuildMode.None || EventSystem.current.IsPointerOverGameObject())
        {
            currentPreview.SetActive(false);
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            currentPreview.SetActive(true);
            currentPreview.transform.position = hit.point;

            bool canAction = false;

            if (currentMode == BuildMode.BuildSprigatito)
            {
                bool isValidTerrain = hit.collider.CompareTag("Off_Track");
                bool isClear = !Physics.CheckSphere(hit.point, towerRadius, blockingLayers, QueryTriggerInteraction.Ignore);
                canAction = isValidTerrain && isClear;

                previewRenderer.material.color = canAction ? validColor : invalidColor;

                if (Input.GetMouseButtonDown(0) && canAction)
                {
                    Instantiate(sprigatitoPrefab, hit.point, Quaternion.identity);
                }
            }
            else if (currentMode == BuildMode.Remove)
            {
                // Is the thing we are pointing at on the Tower layer (Layer 7)?
                bool hitTower = hit.collider.gameObject.layer == LayerMask.NameToLayer("Tower");
                canAction = hitTower; // Green if over a tower, Red if not

                previewRenderer.material.color = canAction ? validColor : invalidColor;

                if (Input.GetMouseButtonDown(0) && canAction)
                {
                    Destroy(hit.collider.gameObject);
                }
            }
        }
        else
        {
            currentPreview.SetActive(false);
        }
    }
}