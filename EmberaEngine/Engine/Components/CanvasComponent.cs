using EmberaEngine.Engine.Core;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using EmberaEngine.Engine.Rendering;

namespace EmberaEngine.Engine.Components
{
    public enum CanvasScaleMode
    {
        ScaleWithScreen,
        ConstantSize,
    }

    public class CanvasComponent : Component
    {
        public override string Type => nameof(CanvasComponent);

        public int ReferenceWidth = 800;
        public int ReferenceHeight = 600;

        public CanvasScaleMode ScaleMode = CanvasScaleMode.ScaleWithScreen;

        private int prevWidth;
        private int prevHeight;
        private float scaleWidth;
        private float scaleHeight;

        public float top, bottom, left, right;

        public Matrix4 OrthoGraphicProjection = Matrix4.Identity;
        public RenderCanvas canvas = new RenderCanvas();


        public override void OnStart()
        {


            OrthoGraphicProjection = Graphics.CreateOrthographic2D(ReferenceWidth, ReferenceHeight, -1f, 1f);

            canvas.Projection = OrthoGraphicProjection;

            CanvasManager.AddRenderCanvas(canvas);

            UIManager.AddCanvas(this);
        }

        public override void OnUpdate(float dt)
        {

            if (ScaleMode == CanvasScaleMode.ScaleWithScreen)
            {
                prevHeight = canvas.ScreenHeight;
                prevWidth = canvas.ScreenWidth;

                OrthoGraphicProjection = CalculateScaleWithScreen();
                canvas.Projection = OrthoGraphicProjection;
            }
        }

        static double MapValue(double value, double fromMin, double fromMax, double toMin, double toMax)
        {
            // First, normalize the value to the range [0, 1] within the source range
            double normalizedValue = ((value - fromMin) * (toMax - toMin)) / (fromMax - fromMin) + toMin;

            return normalizedValue;

        }

        public Vector2 GetLocalMousePos(Vector2 mousePos)
        {
            int mappedX, mappedY;
            if (ScaleMode != CanvasScaleMode.ScaleWithScreen)
            {
            //    mappedX = (int)MapValue(mousePos.X, 0, , 0, Screen.Size.X);
            //    mappedY = (int)MapValue(mousePos.Y, 0, bottom, 0, Screen.Size.Y);

            }

            //mappedX = (int)MapValue(mousePos.X, left, right, 0, Screen.Size.X);
            //mappedY = (int)MapValue(mousePos.Y, top, bottom, 0, Screen.Size.Y);

            Vector2 mappedXY = new Vector2(left + mousePos.X, bottom - mousePos.Y);//CalculateLocalMousePosition(mousePos);

            //Console.WriteLine(mappedXY);


            //Console.WriteLine(new Vector2(mappedX, mappedY));

            return mappedXY;
        }

        public Vector2 CalculateLocalMousePosition(Vector2 mouseScreenPosition)
        {
            // Convert screen position to canvas space
            float canvasX = (mouseScreenPosition.X - canvas.ScreenWidth / 2) / scaleWidth + ReferenceWidth / 2;
            float canvasY = (mouseScreenPosition.Y - canvas.ScreenHeight / 2) / scaleHeight + ReferenceHeight / 2;

            return new Vector2(canvasX, canvasY);
        }



        public Vector2 GetMouseScaleMultiplier()
        {
            return new Vector2(scaleWidth, scaleHeight);
        }

        public Matrix4 CalculateScaleWithScreen()
        {

            float deviceRatio = (float)canvas.ScreenWidth / canvas.ScreenHeight;
            float virtualRatio = (float)ReferenceWidth / ReferenceHeight;

            if (deviceRatio > virtualRatio)
            {
                // The window is wider than the desired aspect ratio
                scaleWidth = deviceRatio / virtualRatio;
                scaleHeight = 1.0f;
            }
            else
            {
                // The window is taller than the desired aspect ratio
                scaleWidth = 1.0f;
                scaleHeight = virtualRatio / deviceRatio;
            }

            left = -(scaleWidth * ReferenceWidth / 2) + ReferenceWidth / 2;
            right = scaleWidth * ReferenceWidth / 2 + ReferenceWidth / 2;
            top = -(scaleHeight * ReferenceHeight / 2) + ReferenceHeight / 2;
            bottom = scaleHeight * ReferenceHeight / 2 + ReferenceHeight / 2;

            Matrix4 projection = Graphics.CreateOrthographicCenter(left, right, top, bottom, 1f, -1f);

            //Matrix4 projection = Graphics.CreateOrthographicCenter(-(scaleWidth * ReferenceWidth) + ReferenceWidth, scaleWidth * ReferenceWidth + ReferenceWidth, -(scaleHeight * ReferenceHeight) + ReferenceHeight, scaleHeight * ReferenceHeight + ReferenceHeight, 1f, -1f);
            return projection;
        }

    }


}
