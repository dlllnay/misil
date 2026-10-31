
using UnityEngine;

public class CrystalTarget : MonoBehaviour
{
    [SerializeField] private Color activatedColor = Color.green;

    private bool activated;

    public bool IsActivated => activated;

    public void Activate()
    {
        if (activated) return;

        activated = true;

        // Kristalin icindeki gorunur 3D modelleri bul.
        Renderer[] renderers =
            GetComponentsInChildren<Renderer>();

        foreach (Renderer rend in renderers)
        {
            if (!rend.enabled) continue;

            foreach (Material mat in rend.materials)
            {
                mat.color = activatedColor;

                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.SetColor(
                        "_EmissionColor",
                        activatedColor * 2f
                    );
                }
            }
        }

        Debug.Log(gameObject.name + " etkinlestirildi!");
    }
}
