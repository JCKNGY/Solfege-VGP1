using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Solfège
{
    public class WaveManager
    {
        public List<Enemy> Enemies = new List<Enemy>();
        public int CurrentWave = 0;
        public bool WaveActive = false;

        private GraphicsDevice graphicsDevice;
        private Random random = new Random();

        // Spawn enemies this far away from the player
        private const float SpawnRadius = 400f;

        public WaveManager(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
        }

        public void StartNextWave(Vector2 playerPosition)
        {
            CurrentWave++;
            Enemies.Clear();

            // Each wave spawns more zombies
            int enemyCount = CurrentWave * 3;

            for (int i = 0; i < enemyCount; i++)
            {
                // Spawn in a ring around the player
                float angle = (float)(random.NextDouble() * Math.PI * 2);
                float dist = SpawnRadius + (float)(random.NextDouble() * 100f);
                Vector2 spawnPos = playerPosition + new Vector2(
                    (float)Math.Cos(angle) * dist,
                    (float)Math.Sin(angle) * dist
                );
                Enemies.Add(new Enemy(spawnPos, graphicsDevice));
            }

            WaveActive = true;
        }

        public void Update(GameTime gameTime, Vector2 playerPosition, Conductor conductor)
        {
            if (!WaveActive) return;

            bool anyAlive = false;
            foreach (Enemy enemy in Enemies)
            {
                enemy.Update(gameTime, playerPosition);

                // Check if enemy touches player and deal damage
                int dmg = enemy.CheckContactDamage(playerPosition, conductor.Size);
                if (dmg > 0)
                    conductor.TakeDamage(dmg);

                if (enemy.IsAlive)
                    anyAlive = true;
            }

            // Wave ends when all enemies are dead
            if (!anyAlive)
                WaveActive = false;
        }

        public void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            foreach (Enemy enemy in Enemies)
                enemy.Draw(spriteBatch, camera);
        }
    }
}