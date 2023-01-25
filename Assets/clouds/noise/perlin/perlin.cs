using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class perlin : MonoBehaviour
{
    public ComputeShader computeShader;
    public RenderTexture renderTexture;
    

    // Start is called before the first frame update
    void Start()
    {
        renderTexture = new RenderTexture(256, 256, 24);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();

        computeShader.SetTexture(0,"Result", renderTexture);
        //computeShader.SetFloat("Resolution", renderTexture.width);
        computeShader.SetFloats("Resolution", renderTexture.width, renderTexture.height);
        computeShader.SetFloat("u_time", Time.time);
        computeShader.Dispatch(0, renderTexture.width / 8, renderTexture.height / 8, 1);


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
