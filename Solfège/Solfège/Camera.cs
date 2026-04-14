using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Solfège
{
    public class Camera
    {
        public Vector2 Position { get; private set; }
        public float Zoom { get; set; } = 1f;
        public float Smoothing { get; set; } = 0.1f;

        private readonly int screenWidth;
        private readonly int screenHeight;
        private Rectangle? worldBounds;

        public Camera(int screenWidth, int screenHeight, Rectangle? worldBounds = null)
        {
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            this.worldBounds = worldBounds;
        }

    }

}
