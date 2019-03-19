using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using CustomUI.Utilities;
using UnityEngine.Rendering;
using System.IO;
using UnityEngine.SceneManagement;
using MusicVideoPlayer.Util;

namespace MusicVideoPlayer
{
    class CustomBloomPrePass : MonoBehaviour, CameraRenderCallbacksManager.ICameraRenderCallbacks
    {
        RenderTexture _bloomPrePassRenderTexture;

        Material _additiveMaterial;
        BloomPrePassParams bloomPrePassParams;
        KawaseBlurRenderer _kawaseBlurRenderer;

        Renderer _renderer;
        Mesh _mesh;
        
        private void Start()
        {
            var bloomPrePass = Resources.FindObjectsOfTypeAll<BloomPrePass>().First(x=>x.gameObject.activeInHierarchy);
            _bloomPrePassRenderTexture = bloomPrePass.GetPrivateField<RenderTexture>("_bloomPrePassRenderTexture");
            
            _mesh = GetComponent<MeshFilter>().mesh;
            _renderer = GetComponent<Renderer>();
            
            BloomPrePassRenderer bppr = Resources.FindObjectsOfTypeAll<BloomPrePassRenderer>().First();
            _kawaseBlurRenderer = bppr.GetPrivateField<KawaseBlurRenderer>("_kawaseBlurRenderer");
            _additiveMaterial = new Material(Shader.Find("Hidden/BlitAdd"));
            _additiveMaterial.SetFloat("_Alpha", 1f);

            bloomPrePassParams = Resources.FindObjectsOfTypeAll<BloomPrePassParams>().First();

            /*
            var screen = GameObject.CreatePrimitive(PrimitiveType.Quad);
            screen.transform.position = Vector3.up * 2f;
            screen.transform.parent = transform;
            var screenRenderer = screen.GetComponent<Renderer>();
            screenRenderer.material = new Material(Shader.Find("Custom/SimpleTexture"));
            screenRenderer.material.mainTexture = _bloomPrePassRenderTexture;
            */

            BSEvents.menuSceneLoaded += RefreshComponent;
            BSEvents.gameSceneLoaded += RefreshComponent;
            BSEvents.menuSceneLoadedFresh += RefreshComponent;
        }

        public void OnCameraPostRender(Camera camera)
        {
            // empty
        }

        public void OnCameraPreRender(Camera camera)
        {
            bool sRGBWrite = GL.sRGBWrite;
            GL.sRGBWrite = false;

            Matrix4x4 oldProjectionMatrix = camera.projectionMatrix;
            Matrix4x4 projectionMatrix;
            if (camera.stereoEnabled)
            {
                Matrix4x4 stereoProjectionMatrix = camera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
                Matrix4x4 stereoProjectionMatrix2 = camera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
                projectionMatrix = MatrixLerp(stereoProjectionMatrix, stereoProjectionMatrix2, 0.5f);
            }
            else
            {
                projectionMatrix = camera.projectionMatrix;
            }
            Vector2 textureToScreenRatio = new Vector2();
            textureToScreenRatio.x = Mathf.Clamp01(1f / (Mathf.Tan(bloomPrePassParams.fov.x * 0.5f * 0.0174532924f) * projectionMatrix.m00));
            textureToScreenRatio.y = Mathf.Clamp01(1f / (Mathf.Tan(bloomPrePassParams.fov.y * 0.5f * 0.0174532924f) * projectionMatrix.m11));
            projectionMatrix.m00 *= textureToScreenRatio.x;
            projectionMatrix.m02 *= textureToScreenRatio.x;
            projectionMatrix.m11 *= textureToScreenRatio.y;
            projectionMatrix.m12 *= textureToScreenRatio.y;

            RenderTexture temporary = RenderTexture.GetTemporary(bloomPrePassParams.textureWidth, bloomPrePassParams.textureHeight, 0, RenderTextureFormat.RGB111110Float, RenderTextureReadWrite.Linear);

            Graphics.SetRenderTarget(temporary);
            GL.Clear(false, true, Color.black);
            GL.PushMatrix();
            GL.LoadProjectionMatrix(projectionMatrix);
            _renderer.material.SetPass(0);
            Graphics.DrawMeshNow(_mesh, Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale));
            GL.PopMatrix();

            RenderTexture blur2 = RenderTexture.GetTemporary(bloomPrePassParams.textureWidth >> bloomPrePassParams.downsample, bloomPrePassParams.textureHeight >> bloomPrePassParams.downsample, 0, RenderTextureFormat.RGB111110Float, RenderTextureReadWrite.Linear);
            DoubleBlur(temporary, blur2, KawaseBlurRenderer.KernelSize.Kernel127, 0.07f, KawaseBlurRenderer.KernelSize.Kernel15, 0.03f, bloomPrePassParams.bloom2Alpha, bloomPrePassParams.downsample);

            Graphics.Blit(blur2, _bloomPrePassRenderTexture, _additiveMaterial);

            RenderTexture.ReleaseTemporary(temporary);
            RenderTexture.ReleaseTemporary(blur2);
            Shader.SetGlobalTexture("_BloomPrePassTexture", _bloomPrePassRenderTexture);
            GL.sRGBWrite = sRGBWrite;
        }

        public void RefreshComponent()
        {
            gameObject.AddComponent<CustomBloomPrePass>();
            CameraRenderCallbacksManager.UnregisterFromCameraCallbacks(this);
            BSEvents.menuSceneLoaded -= RefreshComponent;
            BSEvents.gameSceneLoaded -= RefreshComponent;
            Destroy(this);
        }

        private void OnWillRenderObject() {
           CameraRenderCallbacksManager.RegisterForCameraCallbacks(Camera.current, this);
        }

        public virtual void OnDisable()
        {
            CameraRenderCallbacksManager.UnregisterFromCameraCallbacks(this);
        }

        void OnDestroy()
        {
            CameraRenderCallbacksManager.UnregisterFromCameraCallbacks(this);
        }

        public virtual Matrix4x4 MatrixLerp(Matrix4x4 from, Matrix4x4 to, float t)
        {
            Matrix4x4 result = default(Matrix4x4);
            for (int i = 0; i < 16; i++)
            {
                result[i] = Mathf.Lerp(from[i], to[i], t);
            }
            return result;
        }

        private void DoubleBlur(RenderTexture src, RenderTexture dest, KawaseBlurRenderer.KernelSize kernelSize0, float boost0, KawaseBlurRenderer.KernelSize kernelSize1, float boost1, float secondBlurAlpha, int downsample)
        {
            int[] blurKernel = _kawaseBlurRenderer.GetBlurKernel(kernelSize0);
            int[] blurKernel2 = _kawaseBlurRenderer.GetBlurKernel(kernelSize1);
            int num = 0;
            while (num < blurKernel.Length && num < blurKernel2.Length && blurKernel[num] == blurKernel2[num])
            {
                num++;
            }
            int width = src.width >> downsample;
            int height = src.height >> downsample;
            RenderTextureDescriptor descriptor = src.descriptor;
            descriptor.depthBufferBits = 0;
            descriptor.width = width;
            descriptor.height = height;
            RenderTexture temporary = RenderTexture.GetTemporary(descriptor);
            _kawaseBlurRenderer.Blur(src, temporary, blurKernel, 0f, downsample, 0, num, 0f, 1f, false, KawaseBlurRenderer.WeightsType.None);
            _kawaseBlurRenderer.Blur(temporary, dest, blurKernel, boost0, 0, num, blurKernel.Length - num, 0f, 1f, false, KawaseBlurRenderer.WeightsType.None);
            _kawaseBlurRenderer.Blur(temporary, dest, blurKernel2, boost1, 0, num, blurKernel2.Length - num, 0f, secondBlurAlpha, true, KawaseBlurRenderer.WeightsType.None);
            RenderTexture.ReleaseTemporary(temporary);
        }
    }
}
