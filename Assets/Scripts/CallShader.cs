using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallShader : MonoBehaviour
{

    private Shader currentShader;
    private Material currentMaterial;

    // Use this for initialization
    void Start()
    {
        currentShader = Shader.Find("Custom/Foggy");

        if (!SystemInfo.supportsImageEffects)
        {
            enabled = false;
            return;
        }

        if (!currentShader || !currentShader.isSupported)
        {
            enabled = false;
            return;
        }
        ;
        currentMaterial = new Material(currentShader);
    }

    public void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (currentShader != null)
        {
            Graphics.Blit(source, destination, currentMaterial);
        }

        else
            Graphics.Blit(source, destination);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
