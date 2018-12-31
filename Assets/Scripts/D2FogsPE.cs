//#define DEBUG_RENDER

using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

[ExecuteInEditMode]
public class D2FogsPE : MonoBehaviour
{
    public Color Color = new Color(1f, 1f, 1f, 1f);
    public float Size = 1f;
    public float HorizontalSpeed = 0.2f;
    public float VerticalSpeed = 0f;
    public bool gotHit = false;

    [Range(0.0f, 5)]
    public float Density = 2f;

    public float lastGotHit = 0.0f;

    private Shader currentShader;
    private Shader damageShader;
    private Material _material;
    private Material damageMaterial;
    //private float redAmount = 5.0f;

    public float RedMultiplier = 5.0f;

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (currentShader == null)
        {
            currentShader = Shader.Find("UB/PostEffects/D2Fogs");
            damageShader = Shader.Find("Custom/DamageShader");

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
            _material = new Material(currentShader);
            damageMaterial = new Material(damageShader);
        }

        if (_material)
        {
            DestroyImmediate(_material);
            _material = null;
        }
        if (currentShader || damageShader)
        {
            _material = new Material(currentShader);
            _material.hideFlags = HideFlags.HideAndDontSave;

            if (_material.HasProperty("_Color"))
            {
                _material.SetColor("_Color", Color);
            }
            if (_material.HasProperty("_Size"))
            {
                _material.SetFloat("_Size", Size);
            }
            if (_material.HasProperty("_Speed"))
            {
                _material.SetFloat("_Speed", HorizontalSpeed);
            }
            if (_material.HasProperty("_VSpeed"))
            {
                _material.SetFloat("_VSpeed", VerticalSpeed);
            }
            if (_material.HasProperty("_Density"))
            {
                _material.SetFloat("_Density", Density);
            }
            if (damageMaterial.HasProperty("_RedMultiplier"))
            {
                //damageMaterial.SetFloat("_RedMultiplier", 5.0f);
                damageMaterial.SetFloat("_RedMultiplier", RedMultiplier);
            }
        }

        if ((currentShader != null && _material != null) || (damageShader != null && damageMaterial != null))
        {
            //if (gotHit) /*(Time.time >= coolDown + 5.0f)*/
            //if(lastGotHit <= Time.time)
            if(gotHit)
            {
                damageMaterial = new Material(damageShader);
                damageMaterial.SetFloat("_RedMultiplier", RedMultiplier);
                Graphics.Blit(source, destination, damageMaterial);

                //coolDown = Time.time;
                //Debug.Log("DAMAGE YEAH!");
            }
            else
            {
                //gotHit = false;
                Graphics.Blit(source, destination, _material);
            }
        }
    }
}