using System;
using System.Collections.Generic;

using OpenTK.Mathematics;

namespace EmberaEngine.Engine.Core
{
    public class Camera
    {
        public bool isDefault;
        public Vector3 position;
        public float nearClip = .1f, farClip = 1000f;
        public float fovy;
        public Vector3 front;
        public Vector3 up;
        public Vector3 right;

        private Matrix4 projection;
        private Matrix4 ViewMatrix;

        internal int rendererID;

        public Color4 ClearColor = Color4.Black;

        public void SetProjectionMatrix(Matrix4 projectionMatrix)
        {
            projection = projectionMatrix;
        }

        public Matrix4 GetProjectionMatrix()
        {
            return projection;
        }

        public void SetViewMatrix(Matrix4 viewMatrix)
        {
            ViewMatrix = viewMatrix;
        }

        public Matrix4 GetViewMatrix()
        {
            return ViewMatrix;
        }

        public void SetClearColor(Color4 color)
        {
            ClearColor = color;
        }

        public Color4 GetClearColor()
        {
            return ClearColor;
        }
    }
}