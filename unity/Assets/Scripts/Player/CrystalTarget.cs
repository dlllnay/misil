
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CrystalTarget : MonoBehaviour
{
    [SerializeField] private Color activatedColor = Color.green;

    private bool activated;
    private CrystalDoorPuzzle puzzle;
    private readonly List<(Material material, Color color, Color emission)> rest = new();
    private Coroutine flash;

    public bool IsActivated => activated;
    public TempleRune Rune { get; private set; }

    public void Bind(CrystalDoorPuzzle owner, TempleRune rune)
    {
        puzzle = owner;
        Rune = rune;
        activatedColor = rune.Color();
    }

    public void Activate()
    {
        if (activated) return;
        if (puzzle != null) puzzle.Hit(this);
        else AcceptHit();
    }

    public void AcceptHit()
    {
        if (activated) return;
        CaptureMaterials();
        StopFlash();
        activated = true;
        TempleAudio.Play(TempleSound.Crystal, transform.position);
        Paint(activatedColor, 1.65f);
        Debug.Log(gameObject.name + " etkinlestirildi!");
    }

    public void ResetCrystal()
    {
        activated = false;
        foreach (var slot in rest)
        {
            slot.material.color = slot.color;
            if (slot.material.HasProperty("_EmissionColor"))
                slot.material.SetColor("_EmissionColor", slot.emission);
        }
    }

    public void FlashMistake()
    {
        CaptureMaterials();
        StopFlash();
        flash = StartCoroutine(Flash());
    }

    private IEnumerator Flash()
    {
        Paint(new Color(1f, .08f, .22f), 1.5f);
        yield return new WaitForSeconds(.22f);
        if (!activated) ResetCrystal();
        flash = null;
    }

    private void StopFlash()
    {
        if (flash == null) return;
        StopCoroutine(flash);
        flash = null;
    }

    private void CaptureMaterials()
    {
        if (rest.Count > 0) return;
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            if (!renderer.enabled) continue;
            foreach (var mat in renderer.materials)
                if (mat != null && mat.HasProperty("_BaseColor"))
                    rest.Add((mat, mat.color, mat.HasProperty("_EmissionColor") ? mat.GetColor("_EmissionColor") : Color.black));
        }
    }

    private void Paint(Color color, float emission)
    {
        foreach (var slot in rest)
        {
            slot.material.color = new Color(color.r, color.g, color.b, slot.color.a);
            if (slot.material.HasProperty("_EmissionColor"))
                slot.material.SetColor("_EmissionColor", color * emission);
        }
    }
}
