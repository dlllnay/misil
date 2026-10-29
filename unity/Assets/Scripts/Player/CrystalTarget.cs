
using UnityEngine;

public class CrystalTarget : MonoBehaviour
{
    [SerializeField] private Color activatedColor = Color.green;

    private Renderer crystalRenderer;
    private bool activated;

    private void Awake()
    {
        crystalRenderer = GetComponent<Renderer>();
    }

    public void Activate()
    {
        if (activated) return;

        activated = true;

        if (crystalRenderer != null)
        {
            crystalRenderer.material.color = activatedColor;
        }

        Debug.Log("Buyulu kristal etkinlestirildi!");
    }
}
