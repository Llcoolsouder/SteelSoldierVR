using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallShader2 : MonoBehaviour {

    public Material material;

    public void Start() {
        Camera cameraSource = GetComponent<Camera>();
        cameraSource.depthTextureMode = DepthTextureMode.Depth;
    }

    public void OnRenderImage(RenderTexture source, RenderTexture destination) {
        Graphics.Blit(source, destination, material);
    }

}
