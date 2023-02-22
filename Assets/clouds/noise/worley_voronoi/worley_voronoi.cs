using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class worley_voronoi : MonoBehaviour
{

    public ComputeShader computeShader;
    public int seed;
    private ComputeBuffer buffer;
    public RenderTexture renderTexture;
    private Renderer renderer;
    private int resolution = 132;

    // UI
    public bool MakeSomeNoise;


    void Start()
    {

    }
    void createTexture()
    {

        var format = UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_UNorm;
        TextureWrapMode wrapMode = TextureWrapMode.Repeat;
        renderTexture = new RenderTexture(resolution, resolution, 0);
        renderTexture.graphicsFormat = format;
        renderTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        renderTexture.wrapMode = wrapMode;
        renderTexture.volumeDepth = resolution;
        renderTexture.enableRandomWrite = true;
        renderTexture.filterMode = FilterMode.Bilinear;
        renderTexture.Create();
        AssetDatabase.CreateAsset(renderTexture, "Assets/clouds/noise/temp/worley_3D.asset");
    }
    void CreateVoronoiPoints(int numCells)
    {
        var prng = new System.Random(seed);
        //make points
        var points = new Vector3[numCells * numCells * numCells];
        float cellSize = 1f / numCells;
        for (int x = 0; x < numCells; x++)
        {
            for (int y = 0; y < numCells; y++)
            {
                for (int z = 0; z < numCells; z++)
                {
                    Vector3 neighbour = new Vector3(x, y, z) * cellSize;
                    Vector3 point = new Vector3((float)Random.Range(0.01f, 0.99f), (float)Random.Range(0.01f, 0.99f), (float)Random.Range(0.01f, 0.99f)) * cellSize;

                    int i = x + numCells * (y + z * numCells);
                    points[i] = neighbour + point;
                }
            }
        }
        Debug.Log(points.Length.ToString());

        //copy points to buffer
        buffer = createBuffer(points, sizeof(float) * 3, "points");
    }
    ComputeBuffer createBuffer(System.Array data, int stride, string bufferName, int kernel = 0)
    {
        var buffer = new ComputeBuffer(data.Length, stride, ComputeBufferType.Structured);
        buffer.SetData(data);
        computeShader.SetBuffer(kernel, bufferName, buffer);
        return buffer;
    }

    private void GenerateNoise()
    {
        createTexture();

        CreateVoronoiPoints(8);

        computeShader.SetTexture(0, "Result", renderTexture);
        computeShader.SetBuffer(0, "points", buffer);

        computeShader.SetFloats("Resolution", resolution, resolution, resolution);

        computeShader.SetFloat("u_time", Time.time);
        int workGrps = Mathf.CeilToInt(resolution / (float)8);
        computeShader.Dispatch(0, workGrps, workGrps, workGrps);

        renderer = GetComponent<Renderer>();

        renderer.material.SetTexture("worley", renderTexture);

        buffer.Release();
    }

    void Update()
    {
        if (MakeSomeNoise)
        {
            GenerateNoise();
        }
        MakeSomeNoise = false;

    }
}
