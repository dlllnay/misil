using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public sealed class AncientDiamond : MonoBehaviour
{
    public bool IsCollected { get; private set; }
    private Mesh mesh;

    private void Awake()
    {
        mesh = new Mesh { name = "Kadim elmas" };
        mesh.vertices = new[]
        {
            Vector3.up * 0.48f, Vector3.right * 0.32f,
            Vector3.forward * 0.32f, Vector3.left * 0.32f,
            Vector3.back * 0.32f, Vector3.down * 0.48f
        };
        mesh.triangles = new[]
        {
            0, 2, 1, 0, 3, 2, 0, 4, 3, 0, 1, 4,
            5, 1, 2, 5, 2, 3, 5, 3, 4, 5, 4, 1
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        GetComponent<MeshFilter>().sharedMesh = mesh;

        var colors = new MaterialPropertyBlock();
        colors.SetColor("_BaseColor", new Color(0.12f, 0.94f, 0.85f));
        colors.SetColor("_EmissionColor", new Color(0.16f, 1.4f, 1.15f));
        GetComponent<MeshRenderer>().SetPropertyBlock(colors);
    }

    private void Update()
    {
        transform.Rotate(0f, 42f * Time.deltaTime, 0f);
    }

    public void Collect()
    {
        if (IsCollected)
            return;

        IsCollected = true;
        TempleAudio.Play(TempleSound.Diamond, transform.position);
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (mesh != null)
            Destroy(mesh);
    }
}
