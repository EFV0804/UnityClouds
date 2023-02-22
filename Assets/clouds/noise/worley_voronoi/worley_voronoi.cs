using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class worley_voronoi : MonoBehaviour
{
    public Transform container;
    public Shader shader;
    public Material material;
    public float aplhaRange;

    public ComputeShader computeShader;
    private ComputeBuffer buffer;
    private RenderTexture renderTexture;
    private int resolution = 132;

    // UI
    public bool MakeSomeNoise;

    private void createTexture()
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
        AssetDatabase.CreateAsset(renderTexture, "Assets/clouds/cloudMaterial/worley_3D.asset");

    }

    private ComputeBuffer createBuffer(System.Array data, int stride)
    {
        var buffer = new ComputeBuffer(data.Length, stride, ComputeBufferType.Structured);
        buffer.SetData(data);
        return buffer;
    }

    private Vector3[] CreateVoronoiPoints(int numCells)
    {
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

        return points;
    }

    private void GenerateNoise()
    {

        Vector3[] points = CreateVoronoiPoints(8);
        buffer = createBuffer(points, sizeof(float) * 3);

        computeShader.SetTexture(0, "Result", renderTexture);
        computeShader.SetBuffer(0, "points", buffer);
        computeShader.SetFloats("Resolution", resolution, resolution, resolution);
        computeShader.SetFloat("u_time", Time.time);
        computeShader.SetBuffer(0, "points", buffer);

        int workGrps = Mathf.CeilToInt(resolution / (float)8);
        computeShader.Dispatch(0, workGrps, workGrps, workGrps);

        buffer.Release();
    }

    private void SetShader()
    {

        material.SetTexture("_Worley", renderTexture);
        material.SetVector("boxMin", container.position - container.localScale/2);
        material.SetVector("boxMax", container.position + container.localScale / 2);
        material.SetFloat("alpha_range", aplhaRange);
        //Debug.Log((container.position - container.localScale / 2) + " , " + (container.position + container.localScale / 2));
    }

    private void MakeSomeNoiseFunction()
    {
        createTexture();
        //SetShader();
        GenerateNoise();
    }
    void Update()
    {
        if (MakeSomeNoise)
        {
            MakeSomeNoiseFunction();
        }
        MakeSomeNoise = false;

    }

    [ImageEffectOpaque]
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //if(material == null)
        //{
        //    material = new Material(shader);
        //    Debug.LogError("Fuck it no material");
        //}

        SetShader();
        // Blit does the following:
        // - sets _MainTex property on material to the source texture
        // - sets the render target to the destination texture
        // - draws a full-screen quad
        // This copies the src texture to the dest texture, with whatever modifications the shader makes
        Graphics.Blit(source, destination, material);
    }
}
