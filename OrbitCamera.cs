using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameProject6
{
    public class OrbitCamera
    {
        public Vector3 Position { get; set; }
        public Vector3 Target { get; set; } = Vector3.Zero;
        public Matrix View { get; private set; }
        public Matrix Projection { get; private set; }

        private float yaw = 0f;
        private float pitch = 0f;
        private float distance = 0f;

        private MouseState prevMouse;

        // === Fields For Start Transition === //
        public Vector3 StartZoomPosition { get; set; }
        public Vector3 StartZoomTarget { get; set; }
        public bool playerCanControl = false;
        // =================================== //

        public OrbitCamera(GraphicsDevice graphicsDevice, float initialDistance)
        {
            distance = initialDistance;

            yaw = MathHelper.PiOver4;
            pitch = -MathHelper.PiOver4 / 1.5f;

            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45f), graphicsDevice.Viewport.AspectRatio, 0.1f, 100f);
            UpdateView();
        }

        public void Update()
        {
            if (playerCanControl == true)
            {
                return;
            }

            MouseState mouse = Mouse.GetState();

            if (mouse.RightButton == ButtonState.Pressed && prevMouse.RightButton == ButtonState.Pressed)
            {
                float changeX = mouse.X - prevMouse.X;
                float changeY = mouse.Y - prevMouse.Y;

                yaw -= changeX * 0.01f;
                pitch -= changeY * 0.01f;
                pitch = MathHelper.Clamp(pitch, -MathHelper.PiOver2 + 0.1f, MathHelper.PiOver2 - 0.1f);
            }

            int scroll = mouse.ScrollWheelValue - prevMouse.ScrollWheelValue;
            if (scroll != 0)
            {
                distance -= scroll * 0.005f;
                distance = MathHelper.Clamp(distance, 3f, 20f);
            }

            prevMouse = mouse;

            UpdateView();
        }

        private void UpdateView()
        {
            if (playerCanControl == false)
            {
                Vector3 offset = Vector3.Transform(new Vector3(0, 0, distance), Matrix.CreateRotationX(pitch) * Matrix.CreateRotationY(yaw));
                Position = Target + offset;
            }

            View = Matrix.CreateLookAt(Position, Target, Vector3.Up);
        }

        public void UpdateViewMatrix()
        {
            UpdateView();
        }
    }
}
