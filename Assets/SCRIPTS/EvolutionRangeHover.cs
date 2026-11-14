using UnityEngine;
using UnityEngine.EventSystems;

public class EvolutionRangeHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TowerDetailsPanel viewer;
    public void OnPointerEnter(PointerEventData eventData) { viewer?.SetEvolutionRangePreview(true); }
    public void OnPointerExit(PointerEventData eventData) { viewer?.SetEvolutionRangePreview(false); }
    void OnDisable() { viewer?.SetEvolutionRangePreview(false); }
}
