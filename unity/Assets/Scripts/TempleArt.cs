using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using TMPro;
using MuhurluKapi.Interaction;

// Scene dressing only. Generated meshes have no physics or navigation components.
public sealed class TempleArt : MonoBehaviour
{
    [SerializeField] private Transform orrin;
    [SerializeField] private Transform nyraVisual;
    [SerializeField] private Transform rangerVisual;
    [SerializeField] private Transform firstRune;
    [SerializeField] private Transform finalDoor;
    [SerializeField] private Transform finalPortal;
    [SerializeField] private SealStone orrinSeal;
    [SerializeField] private Material crystalMaterial;
    [SerializeField] private Material projectionGlass;
    [SerializeField] private GameObject firstHall;
    [SerializeField] private GameObject secondHall;
    [SerializeField] private CrystalTarget[] targetCrystals;
    [SerializeField] private CrystalDoorPuzzle crystalPuzzle;
    [SerializeField] private TMP_FontAsset inscriptionFont;

    private readonly List<Object> assets = new();
    private readonly List<GameObject> roots = new();
    private Transform nyraCharm;
    private Transform portalGlyph;
    private Transform sealCrown;
    private Material sealGemMaterial;
    private bool sealLit;
    private Transform projectionVisual;
    private Material projectionGlow;
    private Material projectionTrim;
    private readonly List<(CrystalTarget target, Transform halo)> targetHalos = new();

    private void Start()
    {
        if (crystalMaterial == null) return;
        var velvet = MaterialFor(new Color(.19f, .055f, .33f), Color.black, .25f);
        var lining = MaterialFor(new Color(.36f, .12f, .49f), Color.black, .3f);
        var shadow = MaterialFor(new Color(.027f, .013f, .052f), Color.black, .12f);
        var gold = MaterialFor(new Color(.66f, .43f, .19f), Color.black, .62f);
        var violet = MaterialFor(new Color(.55f, .21f, .94f), new Color(.95f, .22f, 2.3f), .75f);
        var teal = MaterialFor(new Color(.13f, .82f, .78f), new Color(.08f, 1.25f, 1.1f), .7f);
        var stone = MaterialFor(new Color(.72f, .76f, .91f), Color.black, .58f);
        stone.SetTexture("_BaseMap", MakeFlagstone());
        DressHall(firstHall, stone, lining, gold, violet, teal);
        DressHall(secondHall, stone, lining, gold, violet, teal);
        DressInnerHall(stone, gold, violet, teal);
        var tablet = MaterialFor(new Color(.66f, .59f, .72f), new Color(.05f, .04f, .064f), .23f);
        tablet.SetTexture("_BaseMap", stone.GetTexture("_BaseMap"));
        if (targetCrystals != null)
            foreach (var target in targetCrystals)
                if (target != null) MarkTargetCrystal(target, gold);
        if (crystalPuzzle != null)
        {
            var sites = new[] { new Vector3(-10f, 1.8f, -4f), new Vector3(10f, 1.8f, -4f),
                new Vector3(0, 1.8f, 10.8f) };
            for (int i = 0; i < sites.Length; i++)
                Inscription(sites[i], Vector3.zero, "Kadim Yazıt", crystalPuzzle.Clue(i),
                    tablet, gold, i == 1 ? teal : violet);
        }

        if (orrin != null)
        {
            if (orrin.TryGetComponent(out MeshRenderer original)) original.enabled = false;
            var wizard = Root(orrin, "Orrin • Cübbe ve asa", Vector3.zero);
            Profile(wizard, velvet, Vector3.zero, .70f, new(.48f, -.96f), new(.46f, -.83f),
                new(.32f, -.38f), new(.24f, .02f), new(.35f, .36f), new(.18f, .56f), new(0, .58f));
            Profile(wizard, gold, Vector3.zero, .71f, new(.482f, -.95f), new(.475f, -.90f));
            Profile(wizard, lining, new Vector3(0, .32f, 0), .72f, new(.38f, -.12f), new(.46f, -.025f),
                new(.37f, .12f), new(.19f, .24f));
            Profile(wizard, gold, new Vector3(0, .32f, 0), .74f, new(.385f, -.12f), new(.428f, -.075f));
            Profile(wizard, lining, new Vector3(0, .68f, -.018f), .94f, new(.19f, -.24f),
                new(.265f, -.13f), new(.29f, .045f), new(.25f, .20f), new(.13f, .30f), new(0, .37f));
            if (TempleCharacterSkin.Attach(wizard, crystalMaterial, "OrrinHead", new Vector3(0, .48f, .214f)))
            {
                var silver = MaterialFor(new Color(.65f, .63f, .68f), Color.black, .28f);
                var strands = MaterialFor(new Color(.86f, .82f, .77f), Color.black, .24f);
                for (int i = -4; i <= 4; i++)
                {
                    float edge = Mathf.Abs(i) / 4f;
                    var lockRoot = Child(wizard, "Sakal tutamı",
                        new Vector3(i * .023f, .51f + edge * .055f, .331f - edge * .055f));
                    lockRoot.localRotation = Quaternion.Euler(7 + edge * 12, i * 3, -i * 4);
                    float length = .40f - edge * .19f;
                    Profile(lockRoot, i % 2 == 0 ? silver : strands, Vector3.zero, .70f,
                        new(0, -length), new(.022f, -length * .8f), new(.033f, -length * .44f),
                        new(.035f, -.035f), new(.024f, .023f), new(0, .035f));
                }
                Rod(wizard, strands, new Vector3(-.007f, .603f, .368f), new Vector3(-.073f, .559f, .348f), .016f);
                Rod(wizard, strands, new Vector3(.007f, .603f, .368f), new Vector3(.073f, .559f, .348f), .016f);
                foreach (float side in new[] { -1f, 1f })
                    Profile(wizard, silver, new Vector3(side * .135f, .64f, .222f), .65f,
                        new(0, -.22f), new(.030f, -.15f), new(.042f, -.04f),
                        new(.034f, .092f), new(0, .14f));
            }
            else
            {
                Profile(wizard, shadow, new Vector3(0, .68f, .196f), .39f,
                    new(0, -.20f), new(.17f, -.13f), new(.205f, .025f), new(.16f, .17f), new(0, .20f));
            }
            Rod(wizard, gold, new Vector3(-.18f, .32f, .23f), new Vector3(-.11f, -.82f, .31f), .012f);
            Rod(wizard, gold, new Vector3(.18f, .32f, .23f), new Vector3(.11f, -.82f, .31f), .012f);
            Gem(wizard, violet, new Vector3(0, .24f, .33f), .075f, .11f);
            Rod(wizard, lining, new Vector3(-.30f, .25f, 0), new Vector3(-.40f, -.33f, .07f), .145f);
            Rod(wizard, velvet, new Vector3(.31f, .25f, 0), new Vector3(.56f, .05f, .15f), .13f);
            Rod(wizard, gold, new Vector3(.61f, -.96f, .16f), new Vector3(.61f, 1.13f, .16f), .035f);
            Gem(wizard, violet, new Vector3(.61f, 1.26f, .16f), .14f, .23f);
            Ring(wizard, gold, new Vector3(.61f, 1.24f, .16f), .19f, .015f);
            AddLight(wizard, new Vector3(.61f, 1.26f, .16f), new Color(.60f, .26f, 1), 2.8f, .8f);
            DressProjection(wizard);
        }

        if (nyraVisual != null)
        {
            if (!TempleCharacterSkin.Attach(nyraVisual, crystalMaterial, "Nyra", Vector3.zero, true))
                TintCharacter(nyraVisual, new Color(.73f, .60f, 1f));
            nyraCharm = Root(nyraVisual, "Nyra • Büyü odağı", new Vector3(.58f, 1.24f, -.13f));
            Gem(nyraCharm, teal, Vector3.zero, .105f, .18f);
            Ring(nyraCharm, violet, Vector3.zero, .23f, .014f);
        }
        if (rangerVisual != null &&
            !TempleCharacterSkin.Attach(rangerVisual, crystalMaterial, "RangerMale", Vector3.zero))
            TintCharacter(rangerVisual, new Color(.82f, .92f, .80f));

        if (firstRune != null)
        {
            var glyph = Root(firstRune, "Nyra • Rune halkaları", new Vector3(0, 1.025f, 0));
            glyph.localRotation = Quaternion.Euler(90, 0, 0);
            Sigil(glyph, violet, teal, .41f);
        }
        if (finalDoor != null)
        {
            var glyph = Root(finalDoor, "Son kapı • Mühür rozeti", new Vector3(0, .08f, .55f));
            Vector3 scale = finalDoor.lossyScale;
            glyph.localScale = new Vector3(1f / scale.x, 1f / scale.y, 1f / scale.z);
            Sigil(glyph, gold, violet, .66f);
        }
        if (finalPortal != null)
        {
            portalGlyph = Root(finalPortal, "Final • Büyülü geçit", new Vector3(0, 1.65f, 0));
            Sigil(portalGlyph, violet, teal, .88f);
            AddLight(portalGlyph, Vector3.zero, new Color(.42f, .24f, 1), 3.4f, 1f);
        }
        if (orrinSeal != null)
            DressSeal(stone, gold, violet);
        if (orrinSeal is OrrinSealStone trial)
            DressTrial(trial, stone, tablet, gold, violet);

        var accents = Root(transform, "Tapınak • Vurgu ışıkları", Vector3.zero);
        Surface(accents, stone, accents.InverseTransformPoint(new Vector3(-.38f, .531f, -13.514f)),
            Vector3.right * 1.15f, Vector3.back * 1.2f, 1f);
        AddLight(accents, accents.InverseTransformPoint(new Vector3(7.83f, 2.2f, -9.99f)),
            new Color(.20f, .79f, .94f), 6f, 1.3f);
        AddLight(accents, accents.InverseTransformPoint(new Vector3(-5.805f, 2.7f, -24.84f)),
            new Color(.54f, .24f, 1f), 6f, 1.2f);
    }

    private void Update()
    {
        if (projectionVisual != null)
        {
            float pulse = Mathf.Sin(Time.time * 1.8f);
            projectionVisual.localPosition = Vector3.up * (.16f + .055f * pulse);
            projectionGlow.SetColor("_EmissionColor", new Color(.38f, .13f, .83f) * (1f + .14f * pulse));
            projectionTrim.SetColor("_EmissionColor", new Color(.68f, .35f, 1.15f) * (1f + .12f * pulse));
        }
        if (nyraCharm != null) nyraCharm.Rotate(0, 34f * Time.deltaTime, 0);
        if (portalGlyph != null) portalGlyph.Rotate(0, 0, 12f * Time.deltaTime);
        if (sealCrown != null)
        {
            sealCrown.Rotate(0, (sealLit ? 42f : 19f) * Time.deltaTime, 0);
            if (!sealLit && orrinSeal.IsActivated)
            {
                sealLit = true;
                sealGemMaterial.SetColor("_BaseColor", new Color(.12f, .88f, .74f));
                sealGemMaterial.SetColor("_EmissionColor", new Color(.08f, 1.7f, 1.35f));
            }
        }
        foreach (var (target, halo) in targetHalos)
        {
            bool visible = target != null && target.isActiveAndEnabled && !target.IsActivated;
            if (halo.gameObject.activeSelf != visible)
                halo.gameObject.SetActive(visible);
            if (visible)
                halo.Rotate(0, 0, 24f * Time.deltaTime);
        }
    }

    private void DressProjection(Transform wizard)
    {
        if (projectionGlass == null) return;

        var reflection = Instantiate(wizard.gameObject, transform, false);
        reflection.name = "Orrin • Büyülü filigran";
        roots.Add(reflection);
        projectionVisual = reflection.transform;
        projectionVisual.localPosition = Vector3.up * .16f;
        if (rangerVisual != null)
        {
            Vector3 towardPlayer = Vector3.ProjectOnPlane(rangerVisual.position - transform.position, Vector3.up);
            if (towardPlayer.sqrMagnitude > .01f)
                projectionVisual.rotation = Quaternion.LookRotation(towardPlayer);
        }

        projectionGlow = new Material(projectionGlass);
        projectionGlow.SetColor("_BaseColor", new Color(.50f, .30f, .94f, .31f));
        projectionTrim = new Material(projectionGlass);
        projectionTrim.SetColor("_BaseColor", new Color(.72f, .57f, 1f, .49f));
        assets.Add(projectionGlow);
        assets.Add(projectionTrim);

        foreach (var light in reflection.GetComponentsInChildren<Light>())
            light.enabled = false;
        foreach (var renderer in reflection.GetComponentsInChildren<MeshRenderer>())
        {
            var slots = renderer.sharedMaterials;
            for (int i = 0; i < slots.Length; i++)
            {
                var original = slots[i];
                bool highlight = original != null && original.HasProperty("_BaseColor") &&
                    original.GetColor("_BaseColor").grayscale > .30f;
                slots[i] = highlight ? projectionTrim : projectionGlow;
            }
            renderer.sharedMaterials = slots;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        var circle = Root(transform, "Filigran • Zemindeki mühür", Vector3.down * .975f);
        circle.localRotation = Quaternion.Euler(90, 0, 0);
        Ring(circle, projectionTrim, Vector3.zero, .78f, .014f);
        Ring(circle, projectionGlow, Vector3.zero, .66f, .024f);
        if (TryGetComponent(out MeshRenderer oldProjection))
            oldProjection.enabled = false;
    }

    private void DressSeal(Material stone, Material gold, Material violet)
    {
        Transform seal = orrinSeal.transform;
        if (seal.TryGetComponent(out MeshRenderer original)) original.enabled = false;
        var shrine = Root(seal, "Orrin • Kadim mühür kaidesi", Vector3.zero);
        Vector3 scale = seal.lossyScale;
        shrine.localScale = new Vector3(1f / scale.x, 1f / scale.y, 1f / scale.z);
        Profile(shrine, stone, Vector3.zero, 1, new(.88f, -.60f), new(.88f, -.49f),
            new(.70f, -.43f), new(.62f, -.25f), new(.47f, .16f),
            new(.68f, .26f), new(.66f, .42f), new(0, .42f));
        Profile(shrine, gold, Vector3.zero, 1, new(.715f, -.465f), new(.715f, -.432f));
        Profile(shrine, gold, Vector3.zero, 1, new(.682f, .266f), new(.668f, .305f));
        var rune = Root(shrine, "Kaide • Ön mühür", new Vector3(0, -.06f, .63f));
        Sigil(rune, gold, violet, .22f);
        sealCrown = Root(shrine, "Kaide • Dönen mühür kristali", new Vector3(0, .92f, 0));
        sealGemMaterial = MaterialFor(new Color(.56f, .21f, .92f), new Color(.69f, .16f, 1.5f), .66f);
        Gem(sealCrown, sealGemMaterial, Vector3.zero, .29f, .43f);
        Ring(sealCrown, gold, Vector3.zero, .50f, .021f);
        var orbit = Child(sealCrown, "Yörünge rünü", Vector3.zero);
        orbit.localRotation = Quaternion.Euler(57, 23, 0);
        Ring(orbit, sealGemMaterial, Vector3.zero, .47f, .017f);
    }

    private void MarkTargetCrystal(CrystalTarget target, Material gold)
    {
        Color identity = target.Rune.Color();
        var glow = MaterialFor(identity, identity * .88f, .66f);
        foreach (var renderer in target.GetComponentsInChildren<MeshRenderer>())
        {
            if (!renderer.enabled) continue;
            foreach (var material in renderer.materials)
            {
                if (material == null) continue;
                assets.Add(material);
                if (material.HasProperty("_BaseColor"))
                {
                    var original = material.GetColor("_BaseColor");
                    material.SetColor("_BaseColor", new Color(identity.r, identity.g, identity.b, original.a));
                }
                if (material.HasProperty("_EmissionColor"))
                    material.SetColor("_EmissionColor", identity * .80f);
            }
        }

        var hitbox = target.GetComponent<Collider>();
        Vector3 center = hitbox != null ? hitbox.bounds.center : target.transform.position;
        float radius = hitbox != null ? Mathf.Max(.75f, hitbox.bounds.extents.magnitude + .12f) : .9f;
        var halo = Root(transform, "Atış hedefi • " + target.name, transform.InverseTransformPoint(center));
        var direction = Vector3.ProjectOnPlane(center - transform.position, Vector3.up);
        if (direction.sqrMagnitude > .01f)
            halo.rotation = Quaternion.LookRotation(direction);
        Ring(halo, glow, Vector3.zero, radius, .025f);
        Ring(halo, gold, Vector3.zero, radius * 1.12f, .017f);
        int markCount = (int)target.Rune + 3;
        for (int i = 0; i < markCount; i++)
        {
            var axis = Quaternion.Euler(0, 0, i * 360f / markCount) * Vector3.up;
            Rod(halo, glow, axis * (radius * 1.16f), axis * (radius * 1.31f), .023f);
        }
        targetHalos.Add((target, halo));
    }

    private void DressTrial(OrrinSealStone trial, Material stone, Material tablet, Material gold, Material violet)
    {
        var origin = trial.transform.position;
        Inscription(origin + new Vector3(0, 1.72f, -.35f), origin + Vector3.forward * 5f,
            "Üç Nefesin Yemini", "Önce derinliklerin sessiz nefesi uyanır.\n" +
            "Ardından gecenin sakladığı kudret yükselir.\nSonunda ilk ışık, karanlığın mührünü kırar.",
            tablet, gold, violet, true);
        var altar = Facing(new Vector3(origin.x, .552f, origin.z), origin, "Mühür • Zemin işlemesi");
        Ring(altar, gold, Vector3.zero, 1.24f, .026f);
        Ring(altar, violet, Vector3.up * .006f, 1.12f, .018f);
        for (int i = 0; i < 3; i++)
        {
            var rune = (TempleRune)i;
            Vector3 position = trial.RunePosition(rune);
            var node = Root(transform, "Mühür rünü • " + rune.Label(), transform.InverseTransformPoint(position));
            var color = rune.Color();
            var glow = MaterialFor(color, color * .72f, .64f);
            Profile(node, stone, Vector3.zero, 1f, new(.44f, -.63f), new(.44f, -.49f),
                new(.32f, -.45f), new(.28f, -.12f), new(0, -.12f));
            var emblem = Child(node, "Rün işareti", Vector3.up * .18f);
            var direction = Vector3.ProjectOnPlane(position - origin, Vector3.up);
            emblem.rotation = Quaternion.LookRotation(direction);
            Ring(emblem, gold, Vector3.zero, .36f, .018f);
            RuneRelief(emblem, rune, glow);
            var prompt = Facing(position + Vector3.up * .68f, origin, "Rün • Etkileşim");
            TabletText(prompt, "E", 0, new Vector2(.48f, .36f), 2.4f, color, FontStyles.Bold);
        }
    }

    private void RuneRelief(Transform emblem, TempleRune rune, Material glow)
    {
        if (rune == TempleRune.Turquoise)
        {
            for (int row = -1; row <= 1; row++)
            for (int i = 0; i < 8; i++)
            {
                Vector3 Wave(int step) => new Vector3(-.23f + step * .0575f,
                    row * .11f + Mathf.Sin(step * Mathf.PI / 4f) * .032f, 0);
                Rod(emblem, glow, Wave(i), Wave(i + 1), .016f);
            }
        }
        else if (rune == TempleRune.Purple)
        {
            for (int i = 0; i < 12; i++)
            {
                Vector3 Arc(int step, float width)
                {
                    float angle = Mathf.PI * step / 12f;
                    return new Vector3(.13f - Mathf.Sin(angle) * width, Mathf.Cos(angle) * .24f, 0);
                }
                Rod(emblem, glow, Arc(i, .33f), Arc(i + 1, .33f), .018f);
                Rod(emblem, glow, Arc(i, .18f), Arc(i + 1, .18f), .014f);
            }
        }
        else
        {
            Rod(emblem, glow, new Vector3(-.24f, -.11f), new Vector3(.24f, -.11f), .017f);
            for (int i = 0; i <= 6; i++)
            {
                float angle = i * Mathf.PI / 6f;
                Vector3 ray = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
                Vector3 horizon = Vector3.down * .11f;
                Rod(emblem, glow, horizon + ray * .14f, horizon + ray * .23f, .016f);
            }
        }
    }

    private Transform Facing(Vector3 position, Vector3 reader, string name)
    {
        var node = Root(transform, name, transform.InverseTransformPoint(position));
        Vector3 direction = Vector3.ProjectOnPlane(position - reader, Vector3.up);
        if (direction.sqrMagnitude > .01f) node.rotation = Quaternion.LookRotation(direction);
        return node;
    }

    private void Inscription(Vector3 position, Vector3 reader, string title, string body,
        Material stone, Material gold, Material glow, bool tall = false)
    {
        var board = Facing(position, reader, "Taş tablet • " + title);
        float width = tall ? 2.26f : 1.73f;
        float height = tall ? .82f : .61f;
        Vector3 u = Vector3.left * width, v = Vector3.up * height;
        Surface(board, stone, Vector3.forward * .03f, u, v, 1f);
        Surface(board, stone, Vector3.forward * .20f, -u, v, 1f);
        Surface(board, stone, Vector3.up * height + Vector3.forward * .115f,
            u, Vector3.forward * .085f, 1f);
        for (int side = -1; side <= 1; side += 2)
        {
            Surface(board, stone, new Vector3(side * width, 0, .115f),
                Vector3.forward * (-side * .085f), v, 1f);
            float x = side * (width - .10f), y = side * (height - .09f);
            Rod(board, gold, new Vector3(-x, y, .008f), new Vector3(x, y, .008f), .016f);
            Rod(board, gold, new Vector3(x, -y, .008f), new Vector3(x, y, .008f), .016f);
            Sigil(Child(board, "Köşe işlemesi", new Vector3(x - side * .18f, height - .20f, -.025f)),
                gold, glow, .087f);
        }
        TabletText(board, title, height - .24f, new Vector2(width * 2f - .72f, .39f),
            3.05f, new Color(1f, .82f, .48f), FontStyles.Bold | FontStyles.Italic).characterSpacing = 4f;
        Rod(board, gold, new Vector3(-width * .69f, height - .39f, .005f),
            new Vector3(width * .69f, height - .39f, .005f), .008f);
        TabletText(board, body, -.17f, new Vector2(width * 2f - .43f, height * 2f - .50f),
            2.7f, new Color(.96f, .92f, .83f), FontStyles.Normal);
    }

    private TextMeshPro TabletText(Transform parent, string value, float y, Vector2 bounds,
        float size, Color color, FontStyles style)
    {
        var text = Child(parent, "Taşa işlenen metin", new Vector3(0, y, -.022f))
            .gameObject.AddComponent<TextMeshPro>();
        if (inscriptionFont != null) text.font = inscriptionFont;
        text.text = value;
        text.color = color;
        text.fontStyle = style;
        text.fontSize = size;
        text.enableAutoSizing = true;
        text.fontSizeMin = size * .78f;
        text.fontSizeMax = size;
        text.alignment = TextAlignmentOptions.Center;
        text.rectTransform.sizeDelta = bounds;
        text.overflowMode = TextOverflowModes.Truncate;
        text.richText = false;
        return text;
    }

    private void DressInnerHall(Material stone, Material gold, Material violet, Material teal)
    {
        if (secondHall == null) return;
        var decor = Root(transform, "İç salon • Kadim mimari", Vector3.zero);
        foreach (float x in new[] { -12.7f, 11.15f })
        foreach (float z in new[] { -16.1f, -22.6f, -28.9f })
        {
            float inward = x < 0 ? 1 : -1;
            var pier = Child(decor, "Kırık sütun ve kristal kaidesi",
                decor.InverseTransformPoint(new Vector3(x, .54f, z)));
            float height = z > -20f ? 2.35f : z > -25f ? 1.55f : 2.10f;
            Profile(pier, stone, Vector3.zero, .86f, new(.68f, 0), new(.68f, .15f),
                new(.54f, .22f), new(.42f, .36f), new(.34f, height - .17f),
                new(.29f, height), new(0, height - .09f));
            Profile(pier, gold, Vector3.zero, .86f, new(.44f, .32f), new(.43f, .38f));
            Gem(pier, teal, new Vector3(inward * .52f, .43f, .35f), .15f, .38f);
            Gem(pier, teal, new Vector3(inward * .68f, .29f, .06f), .10f, .24f);
            var relief = Child(pier, "Duvar kabartması", new Vector3(-inward * .36f, 2.73f, 0));
            relief.rotation = Quaternion.Euler(0, -inward * 90f, 0);
            Sigil(relief, gold, violet, .44f);
            Gem(relief, violet, new Vector3(0, -.63f, -.19f), .11f, .25f);
        }
        var inlay = Child(decor, "İç salon • Taban rünleri",
            decor.InverseTransformPoint(new Vector3(-.76f, .552f, -25.4f)));
        Ring(inlay, gold, Vector3.zero, 1.75f, .025f);
        Ring(inlay, violet, Vector3.up * .006f, 1.61f, .014f);
        for (int i = 0; i < 8; i++)
        {
            Vector3 radial = Quaternion.Euler(0, i * 45f, 0) * Vector3.forward;
            Rod(inlay, gold, radial * 1.41f, radial * 1.58f, .016f);
        }
    }

    private void DressHall(GameObject hall, Material stone, Material cloth, Material gold,
        Material violet, Material teal)
    {
        if (hall == null) return;

        var wallTint = new MaterialPropertyBlock();
        foreach (var renderer in hall.GetComponentsInChildren<MeshRenderer>())
        {
            renderer.GetPropertyBlock(wallTint);
            wallTint.SetColor("_BaseColor", new Color(.57f, .62f, .82f));
            renderer.SetPropertyBlock(wallTint);
        }

        var dressing = Root(hall.transform, "Tapınak • Taş döşeme ve tonoz", Vector3.zero);
        Surface(dressing, stone, new Vector3(0, .023f, 0), new Vector3(9.98f, 0, 0),
            new Vector3(0, 0, -9.98f), 5f);
        Surface(dressing, stone, new Vector3(0, 4.22f, 0), new Vector3(9.98f, 0, 0),
            new Vector3(0, 0, 9.98f), 5f);

        // The arches frame the back wall, outside the hall's central walking line.
        foreach (float x in new[] { -6.4f, -3.2f, 3.2f, 6.4f })
        {
            var pillar = new Vector3(x, 0, 8.85f);
            Profile(dressing, stone, pillar, 1, new(.43f, .02f), new(.43f, .16f),
                new(.31f, .22f), new(.25f, .42f), new(.25f, 3.07f),
                new(.37f, 3.24f), new(.37f, 3.42f));
            Profile(dressing, gold, pillar, 1, new(.256f, 2.92f), new(.256f, 2.98f));
        }

        foreach (float x in new[] { -4.8f, 4.8f })
        {
            for (int i = 0; i < 12; i++)
            {
                float a = i * Mathf.PI / 12;
                float b = (i + 1) * Mathf.PI / 12;
                Vector3 Arc(float angle) => new Vector3(x + Mathf.Cos(angle) * 1.6f,
                    3.31f + Mathf.Sin(angle) * .88f, 8.85f);
                Rod(dressing, stone, Arc(a), Arc(b), .17f);
            }
            var banner = Root(dressing, "Mor tapınak sancağı", new Vector3(x, 2.79f, 8.94f));
            Surface(banner, cloth, Vector3.zero, Vector3.left * .53f, Vector3.up * .98f, 1);
            Rod(banner, gold, new Vector3(-.57f, .99f, -.025f), new Vector3(.57f, .99f, -.025f), .026f);
            Rod(banner, gold, new Vector3(-.5f, -.96f, -.025f), new Vector3(.5f, -.96f, -.025f), .014f);
            Sigil(Root(banner, "Sancak mührü", Vector3.back * .045f), gold, violet, .36f);
        }

        // Controlled local accents draw attention to the architecture without new shadows.
        AddLight(dressing, new Vector3(-4.8f, 2.1f, 7.8f), new Color(.55f, .26f, 1), 5.8f, 1.6f);
        AddLight(dressing, new Vector3(4.8f, 2.15f, 7.8f), new Color(.13f, .77f, .89f), 5.8f, 1.6f);
        Gem(dressing, teal, new Vector3(-3.2f, 3.54f, 8.55f), .10f, .17f);
        Gem(dressing, violet, new Vector3(3.2f, 3.54f, 8.55f), .10f, .17f);
    }

    private Texture2D MakeFlagstone()
    {
        const int size = 256;
        const int tile = 64;
        var pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            int localX = x % tile, localY = y % tile;
            int edge = Mathf.Min(Mathf.Min(localX, localY), Mathf.Min(tile - 1 - localX, tile - 1 - localY));
            float grain = Mathf.PerlinNoise(x * .18f, y * .18f) * .12f +
                Mathf.PerlinNoise(x * .031f, y * .031f) * .24f;
            float variation = Mathf.PerlinNoise((x / tile) * 2.3f, (y / tile) * 1.9f) * .15f;
            float value = edge < 2 ? .13f : .33f + grain + variation;
            if (edge == 2) value *= .65f;
            if (edge == 3) value *= 1.18f;
            pixels[y * size + x] = new Color(value * .83f, value * .88f, value);
        }
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, true) { name = "Tapınak taşı" };
        texture.SetPixels32(pixels);
        texture.Apply(true, true);
        assets.Add(texture);
        return texture;
    }

    private void Surface(Transform parent, Material material, Vector3 center, Vector3 u, Vector3 v, float repeat)
    {
        var mesh = new Mesh
        {
            name = "Tapınak yüzeyi",
            vertices = new[] { -u - v, u - v, u + v, -u + v },
            uv = new[] { Vector2.zero, Vector2.right * repeat, Vector2.one * repeat, Vector2.up * repeat },
            triangles = new[] { 0, 1, 2, 0, 2, 3 }
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        assets.Add(mesh);
        var surface = Child(parent, "Dekor yüzeyi", center);
        surface.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = surface.gameObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
    }

    private static void TintCharacter(Transform model, Color color)
    {
        var tint = new MaterialPropertyBlock();
        foreach (var renderer in model.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            renderer.GetPropertyBlock(tint);
            tint.SetColor("_BaseColor", color);
            renderer.SetPropertyBlock(tint);
        }
    }

    private Material MaterialFor(Color color, Color emission, float smoothness)
    {
        var material = new Material(crystalMaterial);
        material.SetColor("_BaseColor", color);
        material.SetColor("_EmissionColor", emission);
        material.SetFloat("_Smoothness", smoothness);
        assets.Add(material);
        return material;
    }

    private Transform Root(Transform parent, string name, Vector3 position)
    {
        var node = Child(parent, name, position);
        roots.Add(node.gameObject);
        return node;
    }

    private static Transform Child(Transform parent, string name, Vector3 position)
    {
        var node = new GameObject(name).transform;
        node.SetParent(parent, false);
        node.localPosition = position;
        return node;
    }

    private void Profile(Transform parent, Material material, Vector3 position, float depth, params Vector2[] profile)
    {
        const int sides = 20;
        var vertices = new Vector3[profile.Length * sides];
        var indices = new int[(profile.Length - 1) * sides * 6];
        for (int ring = 0; ring < profile.Length; ring++)
        for (int side = 0; side < sides; side++)
        {
            float angle = side * 2f * Mathf.PI / sides;
            vertices[ring * sides + side] = new Vector3(Mathf.Cos(angle) * profile[ring].x,
                profile[ring].y, Mathf.Sin(angle) * profile[ring].x * depth);
            if (ring == profile.Length - 1) continue;
            int a = ring * sides + side, b = ring * sides + (side + 1) % sides;
            int index = a * 6;
            indices[index] = a; indices[index + 1] = a + sides; indices[index + 2] = b;
            indices[index + 3] = b; indices[index + 4] = a + sides; indices[index + 5] = b + sides;
        }
        var mesh = new Mesh { name = "Tapınak profil ağı", vertices = vertices, triangles = indices };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        assets.Add(mesh);
        var node = Child(parent, "Görsel parça", position);
        node.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = node.gameObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
    }

    private void Rod(Transform parent, Material material, Vector3 from, Vector3 to, float radius)
    {
        var axis = Child(parent, "İşleme", from);
        axis.localRotation = Quaternion.FromToRotation(Vector3.up, to - from);
        Profile(axis, material, Vector3.zero, 1f, new(0, 0), new(radius, 0),
            new(radius, Vector3.Distance(from, to)), new(0, Vector3.Distance(from, to)));
    }

    private void Gem(Transform parent, Material material, Vector3 position, float radius, float height)
    {
        Profile(parent, material, position, 1f, new(0, -height), new(radius, -.1f * height),
            new(radius * .82f, .35f * height), new(0, height));
    }

    private void Ring(Transform parent, Material material, Vector3 position, float radius, float width)
    {
        var axis = Child(parent, "Rune halkası", position);
        axis.localRotation = Quaternion.Euler(90, 0, 0);
        Profile(axis, material, Vector3.zero, 1, new(radius - width, -.008f),
            new(radius, -.008f), new(radius, .008f), new(radius - width, .008f), new(radius - width, -.008f));
    }

    private void Sigil(Transform parent, Material border, Material core, float radius)
    {
        Ring(parent, border, Vector3.zero, radius, .025f);
        Ring(parent, core, Vector3.zero, radius * .78f, .018f);
        var up = Vector3.up * radius * .62f;
        var right = Vector3.right * radius * .39f;
        Rod(parent, core, up, right, .014f);
        Rod(parent, core, right, -up, .014f);
        Rod(parent, core, -up, -right, .014f);
        Rod(parent, core, -right, up, .014f);
    }

    private static void AddLight(Transform parent, Vector3 position, Color color, float range, float intensity)
    {
        var light = Child(parent, "Büyü ışığı", position).gameObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.range = range;
        light.intensity = intensity;
        light.shadows = LightShadows.None;
    }

    private void OnDestroy()
    {
        foreach (var root in roots)
            if (root != null) Destroy(root);
        foreach (var asset in assets)
            if (asset != null) Destroy(asset);
    }
}
