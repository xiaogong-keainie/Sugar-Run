using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class AcidFlow : MonoBehaviour
{
    [Header("Flow")]
    public float speed = 0.3f;

    [Header("Wave")]
    public float waveAmount = 0.3f;
    public float waveSpeed = 2f;
    public float waveFrequency = 3f;
    [Range(4, 100)] public int segments = 30;

    private Mesh mesh;
    private Vector3[] baseVerts;
    private float offset;

    void Start()
    {
        BuildWaveMesh();
    }

    void BuildWaveMesh()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;

        float w = sr.sprite.bounds.size.x;
        float h = sr.sprite.bounds.size.y;

        int count = segments + 1;
        baseVerts = new Vector3[count * 2];
        Vector3[] verts = new Vector3[count * 2];
        Vector2[] uv = new Vector2[count * 2];
        int[] tris = new int[segments * 6];

        float segW = w / segments;

        for (int i = 0; i < count; i++)
        {
            float t = (float)i / segments;
            float x = -w / 2f + i * segW;
            float bottom = -h / 2f;
            float top = h / 2f;

            baseVerts[i * 2] = new Vector3(x, bottom, 0);
            baseVerts[i * 2 + 1] = new Vector3(x, top, 0);
            verts[i * 2] = baseVerts[i * 2];
            verts[i * 2 + 1] = baseVerts[i * 2 + 1];
            uv[i * 2] = new Vector2(t, 0);
            uv[i * 2 + 1] = new Vector2(t, 1);
        }

        for (int i = 0; i < segments; i++)
        {
            int a = i * 2;
            int b = a + 1;
            int c = a + 2;
            int d = a + 3;

            tris[i * 6] = a;
            tris[i * 6 + 1] = c;
            tris[i * 6 + 2] = b;
            tris[i * 6 + 3] = b;
            tris[i * 6 + 4] = c;
            tris[i * 6 + 5] = d;
        }

        mesh = new Mesh();
        mesh.vertices = verts;
        mesh.uv = uv;
        mesh.triangles = tris;

        // Replace SpriteRenderer with MeshFilter + MeshRenderer
        var mf = gameObject.AddComponent<MeshFilter>();
        var mr = gameObject.AddComponent<MeshRenderer>();
        mr.material = sr.material;
        Destroy(sr);
    }

    void Update()
    {
        if (mesh == null || baseVerts == null) return;

        offset += Time.deltaTime * speed;
        int count = segments + 1;
        var verts = mesh.vertices;

        for (int i = 0; i < count; i++)
        {
            float t = (float)i / segments;
            float wave = Mathf.Sin(t * waveFrequency * Mathf.PI * 2 + Time.time * waveSpeed + offset) * waveAmount;

            // Bottom stays fixed
            verts[i * 2] = baseVerts[i * 2];
            // Top waves
            verts[i * 2 + 1] = baseVerts[i * 2 + 1] + new Vector3(0, wave, 0);
        }

        mesh.vertices = verts;
        mesh.RecalculateNormals();
    }
}
