using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Solfège
{
    public class Map
    {
        public const int TileWidth = 32;
        public const int TileHeight = 32;

        public const int MapTilesWide = 100;
        public const int MapTilesTall = 50;

        public int MapWidthPixels = MapTilesWide * TileWidth;
        public int MapHeightPixels = MapTilesTall * TileHeight;

        private int[,] tileMap;

        private List<Texture2D> tileTextures;

        public Map(ContentManager content, GraphicsDevice graphicsDevice)
        {
            tileTextures = new List<Texture2D>();

            for (int i = 0; i < 65; i++)
            {
                tileTextures.Add(content.Load<Texture2D>("sprites/white"));
            }

            GenerateMap();
        }

        private void GenerateMap()
        {
            tileMap = new int[MapTilesWide, MapTilesTall];

            for (int y = 0; y < MapTilesTall; y++)
            {
                for (int x = 0; x < MapTilesWide; x++)
                {
                    if (x == 0 || x == MapTilesWide - 1 ||
                        y == 0 || y == MapTilesTall - 1)
                    {
                        tileMap[x, y] = 0; // wall
                    }
                    else
                    {
                        tileMap[x, y] = 1; // blank white floor
                    }
                }
            }
        }

        public bool IsWall(int tileX, int tileY)
        {
            if (tileX < 0 || tileX >= MapTilesWide || tileY < 0 || tileY >= MapTilesTall)
                return true;

            return tileMap[tileX, tileY] == 0;
        }

        public void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            int startX = (int)(camera.Position.X / TileWidth);
            int startY = (int)(camera.Position.Y / TileHeight);
            int endX = startX + (1280 / TileWidth) + 2;
            int endY = startY + (720 / TileHeight) + 2;

            startX = Math.Max(0, startX);
            startY = Math.Max(0, startY);
            endX = Math.Min(MapTilesWide, endX);
            endY = Math.Min(MapTilesTall, endY);

            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    int tileIndex = tileMap[x, y];
                    if (tileIndex == 0) continue; // skip walls (invisible border)

                    Vector2 worldPos = new Vector2(x * TileWidth, y * TileHeight);
                    Vector2 screenPos = worldPos - camera.Position;

                    spriteBatch.Draw(tileTextures[tileIndex], screenPos, Color.White);
                }
            }
        }
    }
}