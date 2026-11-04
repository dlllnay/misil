
using UnityEngine;

public class NyraRune : MonoBehaviour
{
    [SerializeField] private Transform nyra;
    [SerializeField] private float activationDistance = 1.8f;
    [SerializeField] private Color activatedColor = Color.cyan;

    private Renderer runeRenderer;
    private bool activated;

    public bool IsActivated => activated;

    private void Awake()
    {
        runeRenderer = GetComponent<Renderer>();
    }

    private void Update()
    {
        if (activated || nyra == null)
            return;

        Vector3 runePosition = transform.position;
        Vector3 nyraPosition = nyra.position;

        // Yalnizca yatay uzakligi kontrol et.
        runePosition.y = 0f;
        nyraPosition.y = 0f;

        float distance = Vector3.Distance(
            runePosition,
            nyraPosition
        );

        if (distance <= activationDistance)
        {
            Activate();
        }
    }

    private void Activate()
    {
        activated = true;

        if (runeRenderer != null)
        {
            runeRenderer.material.color = activatedColor;
        }

        Debug.Log("Nyra buyulu muhru etkinlestirdi!");
    }
}
