using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class MeshGenerator : MonoBehaviour
{
    public int meshWidth;
    public int meshHeight;

    public float noiseXOffset;
    public float noiseYOffset;
    public float noiseIntensity;
    public NoiseMapData[] noiseMaps;

    MeshFilter meshFilter;
    MeshCollider meshCollider;
    [SerializeField] Transform wallsColliderTransform;

    Material noiseMaterial;

    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;

    public bool useHeightmap = true;
    public bool autoUpdateMesh;
    public bool autoUpdateNoiseTexture;

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        noiseMaterial = GetComponent<MeshRenderer>().sharedMaterial;

        //CreateTexture();
    }

    public void CreateTexture()
    {
        Texture2D noiseTexture = new Texture2D(meshWidth, meshHeight);

        for (int x = 0; x < meshWidth; x++)
        {
            for (int y = 0; y < meshHeight; y++)
            {
                float noiseValue = 0;
                float totalStrength = 0;
                foreach (NoiseMapData mapData in noiseMaps)
                {
                    noiseValue += Mathf.PerlinNoise((noiseXOffset + x) / mapData.zoomDivider, (noiseYOffset + y) / mapData.zoomDivider) * mapData.strength;
                    totalStrength += mapData.strength;
                }
                noiseValue /= totalStrength;

                noiseTexture.SetPixel(x, y, new Color(noiseValue, noiseValue, noiseValue));
            }
        }

        noiseTexture.Apply();
        noiseMaterial.SetTexture("_MainTex", noiseTexture);
    }

    public void CreateMesh(int? _width, int? _height, float? _offsetNoiseX, float? _offsetNoiseY, float? _terrainHeight)
    {
        int width = _width ?? meshWidth;
        int height = _height ?? meshHeight;
        float offsetNoiseX = _offsetNoiseX ?? noiseXOffset;
        float offsetNoiseY = _offsetNoiseY ?? noiseYOffset;
        float terrainHeight = _terrainHeight ?? noiseIntensity;

        vertices = new Vector3[width * height];
        triangles = new int[(width - 1) * (height - 1) * 6];
        uvs = new Vector2[width * height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float noiseValue = 0;
                if (useHeightmap)
                {
                    foreach (NoiseMapData mapData in noiseMaps)
                    {
                        noiseValue += Mathf.PerlinNoise(offsetNoiseX + (x / mapData.zoomDivider), offsetNoiseY + (y / mapData.zoomDivider)) * mapData.strength;
                        //noiseValue += Random.Range(0, 15);
                    }
                }

                vertices[x * height + y] = new Vector3(x, noiseValue * terrainHeight, y);
                uvs[x * height + y] = new Vector2(x / (float)width, y / (float)height);
            }
        }


        for (int x = 0; x < width - 1; x++)
        {
            for (int y = 0; y < height - 1; y++)
            {
                int curQuadIdx = (x * (height - 1) + y) * 6;
                int curVertIdx = x * height + y;

                // first triangle of quad
                triangles[curQuadIdx] = curVertIdx;
                triangles[curQuadIdx + 1] = curVertIdx + 1;
                triangles[curQuadIdx + 2] = curVertIdx + height + 1;
                // second triangle of quad
                triangles[curQuadIdx + 3] = curVertIdx + height;
                triangles[curQuadIdx + 4] = curVertIdx;
                triangles[curQuadIdx + 5] = curVertIdx + height + 1;
            }
        }

        Mesh mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();

        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;
        //print($"vertices: {vertices.Length}, triangles: {triangles.Length / 6}, uvs: {uvs.Length}");

        // --MAYBE TRY USING MESH INSTEAD OF SHATEDMESH--
        //meshFilter.sharedMesh.SetVertices(vertices);
        //meshFilter.sharedMesh.SetUVs(0, uvs);
        //meshFilter.sharedMesh.SetTriangles(triangles, 0);
        //meshFilter.sharedMesh.RecalculateNormals();

        //meshCollider.sharedMesh.SetVertices(vertices);
        //meshCollider.sharedMesh.SetUVs(0, uvs);
        //meshCollider.sharedMesh.SetTriangles(triangles, 0);
        //meshCollider.sharedMesh.RecalculateNormals();

        //if (autoUpdateNoiseTexture)
        //    CreateTexture();
        //else
        //    noiseMaterial.SetTexture("_MainTex", null);

        wallsColliderTransform.position = new Vector3(width / 2, 100, height / 2);
        wallsColliderTransform.localScale = new Vector3(width / 2, 200, height / 2);
    }
}

[System.Serializable]
public struct NoiseMapData
{
    public float zoomDivider;
    [Range(0, 5)]
    public float strength;
}
