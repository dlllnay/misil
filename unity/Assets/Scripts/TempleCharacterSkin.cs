using System.Collections.Generic;
using UnityEngine;

// Reads the original animation rig after Animator has evaluated it.
[DefaultExecutionOrder(100)]
public sealed class TempleCharacterSkin : MonoBehaviour
{
    private struct Joint
    {
        public Transform source;
        public Transform target;
        public Quaternion offset;
    }

    private readonly List<Joint> joints = new();
    private readonly List<Material> materials = new();
    private readonly List<Renderer> hidden = new();
    private Transform sourceRoot;
    private Transform sourceHips;
    private Transform targetHips;
    private Vector3 sourceHipRest;
    private Vector3 targetHipRest;
    private Transform bow;
    private Transform bowHand;
    private Vector3 bowOffset;
    private Quaternion bowRotation;

    public static bool Attach(Transform parent, Material template, string assetName,
        Vector3 offset, bool isNyra = false)
    {
        var prefab = Resources.Load<GameObject>("TempleCharacters/" + assetName);
        if (prefab == null) return false;

        var model = Instantiate(prefab, parent, false);
        model.name = assetName + " • Karakter görünümü";
        model.transform.localPosition = offset;
        var skin = model.AddComponent<TempleCharacterSkin>();
        skin.sourceRoot = parent;
        if (assetName != "OrrinHead" && !skin.MapRig(isNyra))
        {
            model.SetActive(false);
            Destroy(model);
            Debug.LogWarning($"Karakter görünümü: {assetName} iskelet eşleşmesi eksik.", parent);
            return false;
        }

        skin.ApplyMaterials(template, assetName);
        if (assetName != "OrrinHead")
            skin.ReplaceOriginalRenderers(isNyra);
        return true;
    }

    private bool MapRig(bool nyra)
    {
        // The original Generic clips stay on their original rig. Convert each
        // mapped rest axis into the replacement rig's axis before following it.
        var source = Index(sourceRoot, transform);
        var target = Index(transform, null);
        string hips = nyra ? "pelvis_cr" : "pelvis";
        string spine = nyra ? "spine_cr_01" : "spine.01";
        string chest = nyra ? "spine_cr_02" : "spine.02";
        if (!AddJoint(source, target, "Hips", hips) ||
            !AddJoint(source, target, "Abdomen", spine) ||
            !AddJoint(source, target, "Torso", chest) ||
            !AddJoint(source, target, "Head", nyra ? "head_cr" : "head"))
            return false;
        AddJoint(source, target, "Neck", nyra ? "neck_cr" : "neck");

        foreach (string side in new[] { ".L", ".R" })
        {
            string suffix = nyra ? "_cr_" + side[1].ToString().ToLowerInvariant() : side;
            string upper = "upper_arm" + suffix;
            string fore = "forearm" + suffix;
            string hand = "hand" + suffix;
            string thigh = "thigh" + suffix;
            string shin = "shin" + suffix;
            string foot = "tarsal" + suffix;
            AddJoint(source, target, "Shoulder" + side, "shoulder" + suffix);
            if (!AddJoint(source, target, "UpperArm" + side, upper, "LowerArm" + side, fore) ||
                !AddJoint(source, target, "LowerArm" + side, fore, "Fist" + side, hand) ||
                !AddJoint(source, target, "Fist" + side, hand) ||
                !AddJoint(source, target, "UpperLeg" + side, thigh, "LowerLeg" + side, shin) ||
                !AddJoint(source, target, "LowerLeg" + side, shin, "Foot" + side, foot) ||
                !AddJoint(source, target, "Foot" + side, foot))
                return false;
        }

        sourceHips = source["Hips"];
        targetHips = target[hips];
        sourceHipRest = sourceRoot.InverseTransformPoint(sourceHips.position);
        targetHipRest = sourceRoot.InverseTransformPoint(targetHips.position);
        if (!nyra)
            bowHand = target["hand.R"];
        return true;
    }

    private static Dictionary<string, Transform> Index(Transform root, Transform exclude)
    {
        var map = new Dictionary<string, Transform>();
        foreach (var node in root.GetComponentsInChildren<Transform>(true))
            if (exclude == null || (node != exclude && !node.IsChildOf(exclude)))
                map.TryAdd(node.name, node);
        return map;
    }

    private bool AddJoint(Dictionary<string, Transform> sources, Dictionary<string, Transform> targets,
        string from, string to, string fromChild = null, string toChild = null)
    {
        if (!sources.TryGetValue(from, out var a) || !targets.TryGetValue(to, out var b))
            return false;

        Quaternion alignment = Quaternion.identity;
        if (fromChild != null && sources.TryGetValue(fromChild, out var aEnd) &&
            targets.TryGetValue(toChild, out var bEnd))
            alignment = Quaternion.FromToRotation(bEnd.position - b.position, aEnd.position - a.position);

        joints.Add(new Joint { source = a, target = b, offset = Quaternion.Inverse(a.rotation) * alignment * b.rotation });
        return true;
    }

    private void ReplaceOriginalRenderers(bool nyra)
    {
        Transform oldHand = null;
        if (!nyra)
            Index(sourceRoot, transform).TryGetValue("Fist.R", out oldHand);

        foreach (var renderer in sourceRoot.GetComponentsInChildren<Renderer>())
        {
            if (renderer.transform.IsChildOf(transform) || !renderer.enabled ||
                renderer is not (MeshRenderer or SkinnedMeshRenderer))
                continue;

            bool isBow = !nyra && renderer.name.IndexOf("Bow", System.StringComparison.OrdinalIgnoreCase) >= 0;
            if (isBow && !TryCopyBow(renderer, oldHand))
                continue;

            hidden.Add(renderer);
            renderer.enabled = false;
        }

        var animator = sourceRoot.GetComponent<Animator>();
        if (animator != null)
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
    }

    private bool TryCopyBow(Renderer renderer, Transform oldHand)
    {
        if (bow != null || oldHand == null || bowHand == null ||
            !renderer.TryGetComponent<MeshFilter>(out var filter) || filter.sharedMesh == null)
            return false;

        bow = new GameObject("Ranger • Yay").transform;
        bow.SetParent(transform, false);
        bow.gameObject.AddComponent<MeshFilter>().sharedMesh = filter.sharedMesh;
        bow.gameObject.AddComponent<MeshRenderer>().sharedMaterials = renderer.sharedMaterials;
        bowOffset = Quaternion.Inverse(oldHand.rotation) * (renderer.transform.position - oldHand.position);
        bowRotation = Quaternion.Inverse(oldHand.rotation) * renderer.transform.rotation;
        Vector3 size = renderer.transform.lossyScale;
        Vector3 basis = transform.lossyScale;
        bow.localScale = new Vector3(size.x / basis.x, size.y / basis.y, size.z / basis.z);
        return true;
    }

    private void ApplyMaterials(Material template, string assetName)
    {
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            var slots = renderer.sharedMaterials;
            for (int i = 0; i < slots.Length; i++)
            {
                string name = slots[i] != null ? slots[i].name : "";
                var material = new Material(template);
                material.SetColor("_EmissionColor", Color.black);
                material.SetFloat("_Smoothness", .27f);
                string texture;
                Color tint = Color.white;
                bool cutout = false;
                if (assetName == "OrrinHead")
                {
                    texture = renderer.name.Contains("Brows") ? "OrrinBrows" :
                        renderer.name.Contains("Eyes") ? "OrrinEyes" : "OrrinSkin";
                    cutout = texture == "OrrinBrows";
                }
                else if (assetName == "Nyra")
                {
                    texture = renderer.name.Contains("hair") ? "RangerHair" :
                        renderer.name.Contains("head") ? "RangerFace" : "RangerBody";
                    if (texture == "RangerBody")
                    {
                        tint = new Color(1.25f, .54f, 1.38f);
                        material.SetFloat("_Cull", 0f);
                    }
                    if (texture == "RangerHair") tint = new Color(1.35f, 1.42f, 1.60f);
                    cutout = texture == "RangerHair";
                }
                else
                {
                    bool tunic = name.Contains("Tunic");
                    bool trousers = name.Contains("Trousers");
                    texture = tunic || trousers ? null : "RangerArmor";
                    if (tunic) tint = new Color(.095f, .19f, .135f);
                    if (trousers) tint = new Color(.12f, .085f, .064f);
                }
                material.SetTexture("_BaseMap", texture != null ? Resources.Load<Texture2D>("TempleCharacters/" + texture) : null);
                material.SetColor("_BaseColor", tint);
                if (cutout)
                {
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.SetFloat("_AlphaClip", 1f);
                    material.SetFloat("_Cutoff", .40f);
                }
                materials.Add(material);
                slots[i] = material;
            }
            renderer.sharedMaterials = slots;
        }
    }

    private void LateUpdate()
    {
        if (sourceHips != null)
        {
            Vector3 delta = sourceRoot.InverseTransformPoint(sourceHips.position) - sourceHipRest;
            targetHips.position = sourceRoot.TransformPoint(targetHipRest + delta * .75f);
        }
        foreach (var joint in joints)
            joint.target.rotation = joint.source.rotation * joint.offset;
        if (bow != null)
            bow.SetPositionAndRotation(bowHand.position + bowHand.rotation * bowOffset, bowHand.rotation * bowRotation);
    }

    private void OnDestroy()
    {
        foreach (var renderer in hidden)
            if (renderer != null) renderer.enabled = true;
        foreach (var material in materials)
            if (material != null) Destroy(material);
    }
}
