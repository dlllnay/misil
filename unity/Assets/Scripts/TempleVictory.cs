using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static TempleMenu;

public sealed class TempleVictory : MonoBehaviour
{
    private readonly RectTransform[] sparks = new RectTransform[24];
    private readonly CanvasGroup[] stages = new CanvasGroup[4];
    private Image veil;
    private Image flash;
    private CanvasGroup rays;
    private RectTransform seal;
    private float started;

    internal void Build(UnityAction replay, UnityAction returnToMenu)
    {
        var canvas = CreateCanvas(transform, "Tapınak • Zafer sekansı", 300);
        veil = Fill(canvas, Color.clear).GetComponent<Image>();
        var stage = Rect(canvas, 0, 0, 880, 650);
        rays = Group(stage, 88);
        for (int i = 0; i < 12; i++)
        {
            float angle = i * 30f + 15f;
            Vector2 center = Orbit(angle, 318);
            var ray = Tile((RectTransform)rays.transform, center.x, center.y, 230, 2,
                i % 2 == 0 ? Gold : Violet);
            ray.localEulerAngles = new Vector3(0, 0, angle);
        }
        seal = Rect(stage, 0, 88, 1, 1);
        for (int i = 0; i < 48; i++)
        {
            float angle = i * 360f / 48;
            Vector2 outer = Orbit(angle, 248);
            var stroke = Tile(seal, outer.x, outer.y, 30, 1.5f, new Color(.55f, .32f, .95f, .43f));
            stroke.localEulerAngles = new Vector3(0, 0, angle + 90);
            if (i % 4 == 0)
            {
                Vector2 inner = Orbit(angle, 226);
                var glyph = Tile(seal, inner.x, inner.y, 9, 9, i % 8 == 0 ? Gold : Teal);
                glyph.localEulerAngles = new Vector3(0, 0, angle + 45);
            }
        }

        for (int i = 0; i < sparks.Length; i++)
        {
            float size = 3f + i % 5;
            sparks[i] = Tile(stage, 0, 0, size, size, i % 3 == 0 ? Gold : i % 2 == 0 ? Teal : Violet);
        }

        var title = Group(stage, 130);
        Label((RectTransform)title.transform, "MÜHÜR KIRILDI!", 0, -3, 844, 110, 81,
            new Color(.18f, .07f, .23f)).fontStyle = FontStyles.Bold | FontStyles.Italic;
        Label((RectTransform)title.transform, "MÜHÜR KIRILDI!", 0, 0, 844, 110, 81,
            new Color(1f, .79f, .43f)).fontStyle = FontStyles.Bold | FontStyles.Italic;
        Tile((RectTransform)title.transform, 0, -57, 380, 2, Gold);

        var story = Group(stage, -17);
        var storyRoot = (RectTransform)story.transform;
        Label(storyRoot, "ZAFER SENİN!", 0, 44, 710, 61, 43, Teal).characterSpacing = 6f;
        Label(storyRoot, "Kadim tapınağın sırrını çözdün.\nOrrin'in güvenini kazandın.\nMühürlü kapı artık özgür.",
            0, -49, 735, 126, 26, Ivory);
        stages[0] = title;
        stages[1] = story;
        stages[2] = ButtonGroup(stage, -181, "YENİDEN OYNA", replay);
        stages[3] = ButtonGroup(stage, -251, "ANA MENÜYE DÖN", returnToMenu);
        flash = Fill(canvas, Color.clear).GetComponent<Image>();
        flash.raycastTarget = false;

        started = Time.unscaledTime;
        Animate(0f);
    }

    private static CanvasGroup Group(RectTransform parent, float y) =>
        Rect(parent, 0, y, 1, 1).gameObject.AddComponent<CanvasGroup>();

    private static CanvasGroup ButtonGroup(RectTransform parent, float y, string title, UnityAction action)
    {
        var group = Group(parent, y);
        Action((RectTransform)group.transform, title, 0, action);
        return group;
    }

    private void Update() => Animate(Time.unscaledTime - started);

    private void Animate(float elapsed)
    {
        float darkness = Fade(elapsed, .05f, 1.28f);
        veil.color = new Color(Night.r, Night.g, Night.b, darkness * .94f);
        float pulse = Mathf.Sin(Mathf.PI * Mathf.Clamp01(elapsed / .78f)) * .28f;
        flash.color = new Color(1f, .63f, .38f, pulse);
        rays.alpha = Mathf.Sin(Mathf.PI * Mathf.Clamp01(elapsed / 1.12f)) * .58f;
        rays.transform.localScale = Vector3.one * Mathf.Lerp(1.36f, .86f, Fade(elapsed, 0f, 1.12f));
        seal.localRotation = Quaternion.Euler(0, 0, elapsed * 5f);
        seal.localScale = Vector3.one * Mathf.Lerp(.66f, 1f, Fade(elapsed, .16f, 1.12f));

        for (int i = 0; i < sparks.Length; i++)
        {
            float angle = i * 137.5f + elapsed * (i % 2 == 0 ? 13f : -10f);
            float radius = 168f + i % 7 * 36f + Mathf.Sin(elapsed * 1.6f + i) * 19f;
            radius += (1f - Fade(elapsed, .1f, 1.2f)) * 270f;
            sparks[i].anchoredPosition = Orbit(angle, radius) + new Vector2(0, 76);
            sparks[i].localEulerAngles = new Vector3(0, 0, angle + 45);
        }

        SetStage(stages[0], elapsed, .9f, .74f, false);
        SetStage(stages[1], elapsed, 1.95f, .7f, false);
        SetStage(stages[2], elapsed, 3f, .58f, true);
        SetStage(stages[3], elapsed, 3.38f, .58f, true);
    }

    private static Vector2 Orbit(float angle, float radius)
    {
        float radians = angle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * radius;
    }

    private static void SetStage(CanvasGroup group, float elapsed, float start, float duration, bool isButton)
    {
        float progress = Fade(elapsed, start, duration);
        group.alpha = progress;
        group.transform.localScale = Vector3.one * Mathf.Lerp(.92f, 1f, progress);
        group.interactable = group.blocksRaycasts = isButton && elapsed >= start + duration;
    }

    private static float Fade(float elapsed, float start, float duration) =>
        Mathf.SmoothStep(0f, 1f, (elapsed - start) / duration);
}
