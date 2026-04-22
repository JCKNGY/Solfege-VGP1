using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Solfège
{
    public class WaveManager
    {

        public int CurrentWave { get; private set; } = 0;
        public bool WaveActive { get; private set; } = false;
        public int TotalKills { get; private set; } = 0;
        public int CoinsEarned { get; private set; } = 0;


        private List<Enemy> enemies = new List<Enemy>();
        private List<EnemyProjectile> projectiles = new List<EnemyProjectile>();

        //Spawn control
        private int enemiesToSpawn = 0;
        private int spawnedThisWave = 0;
        private float spawnTimer = 0f;
        private float spawnInterval = 2.0f;


        private const int SpawnMargin = 100;
        private const int ScreenWidth = 1280;
        private const int ScreenHeight = 720;

        private List<Shockwave> shockwaves = new List<Shockwave>();


        private Texture2D pixel;
        private GraphicsDevice graphicsDevice;

        private Random rng = new Random();


        private List<DroppedCoin> coins = new List<DroppedCoin>();

        public WaveManager(GraphicsDevice gd)
        {
            graphicsDevice = gd;

            pixel = new Texture2D(gd, 1, 1);
            pixel.SetData(new[] { Color.White });
        }

        public void StartNextWave(Vector2 playerPosition)
        {
            CurrentWave++;
            spawnedThisWave = 0;
            spawnTimer = 1.0f; // short delay before first spawn

            // More enemies each wave, tighter interval at higher waves
            enemiesToSpawn = 6 + CurrentWave * 2;
            spawnInterval = Math.Max(0.5f, 2.0f - CurrentWave * 0.12f);

            WaveActive = true;
        }


        public void Update(GameTime gameTime, Vector2 playerPosition, Conductor conductor)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (WaveActive && spawnedThisWave < enemiesToSpawn)
            {
                spawnTimer -= elapsed;
                if (spawnTimer <= 0f)
                {
                    SpawnEnemy(playerPosition);
                    spawnTimer = spawnInterval;
                }
            }


            for (int i = 0; i < enemies.Count; i++)
            {
                if (!enemies[i].IsAlive) continue;
                for (int j = i + 1; j < enemies.Count; j++)
                {
                    if (!enemies[j].IsAlive) continue;

                    Vector2 diff = enemies[i].Position - enemies[j].Position;
                    float dist = diff.Length();
                    float minD = (enemies[i].Size.X + enemies[j].Size.X) / 2f + 4f;

                    if (dist < minD && dist > 0.01f)
                    {
                        Vector2 push = diff / dist * (minD - dist) * 3f;
                        enemies[i].SeparationForce += push;
                        enemies[j].SeparationForce -= push;
                    }
                }
            }

            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                Enemy e = enemies[i];

                EnemyProjectile proj = e.Update(gameTime, playerPosition);
                if (proj != null)
                    projectiles.Add(proj);

                if (e.JustSlammed)
                    shockwaves.Add(new Shockwave(e.Position, 80f, 0.5f));

                int contactDmg = e.CheckContactDamage(playerPosition, conductor.Size);
                if (contactDmg > 0)
                    conductor.TakeDamage(contactDmg);

                if (!e.IsAlive)
                {
                    SpawnCoinDrop(e.Position, CoinValueForType(e.Type));
                    TotalKills++;
                    enemies.RemoveAt(i);
                }
            }

            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                EnemyProjectile p = projectiles[i];
                p.Update(gameTime);

                int dmg = p.CheckHit(playerPosition, conductor.Size);
                if (dmg > 0)
                    conductor.TakeDamage(dmg);

                if (!p.IsAlive)
                    projectiles.RemoveAt(i);
            }

            for (int i = shockwaves.Count - 1; i >= 0; i--)
            {
                shockwaves[i].Update(elapsed);

                // Check shockwave hits player
                if (shockwaves[i].CheckHit(playerPosition, conductor.Size))
                {
                    // Slam damage already applied via contact
                }

                if (!shockwaves[i].IsAlive)
                    shockwaves.RemoveAt(i);
            }

            for (int i = coins.Count - 1; i >= 0; i--)
            {
                coins[i].Update(elapsed, playerPosition);

                if (coins[i].Collected)
                {
                    CoinsEarned += coins[i].Value;
                    coins.RemoveAt(i);
                }
                else if (coins[i].Expired)
                {
                    coins.RemoveAt(i);
                }
            }

            if (WaveActive && spawnedThisWave >= enemiesToSpawn && enemies.Count == 0)
            {
                WaveActive = false;

            }
        }

        public void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            // Coins
            foreach (DroppedCoin c in coins)
                c.Draw(spriteBatch, camera, pixel);

            // Enemies
            foreach (Enemy e in enemies)
                e.Draw(spriteBatch, camera);

            // Projectiles
            foreach (EnemyProjectile p in projectiles)
                p.Draw(spriteBatch, camera, pixel);

            // Shockwaves
            foreach (Shockwave s in shockwaves)
                s.Draw(spriteBatch, camera, pixel);

            // Off-screen enemy arrows (like Blooket / survivors-style)
            DrawOffscreenIndicators(spriteBatch, camera);
        }


        private void SpawnEnemy(Vector2 playerPos)
        {
            Vector2 spawnPos = ChooseSpawnPosition(playerPos);
            EnemyType type = ChooseEnemyType();

            enemies.Add(new Enemy(spawnPos, graphicsDevice, type, CurrentWave));
            spawnedThisWave++;
        }

        private Vector2 ChooseSpawnPosition(Vector2 playerPos)
        {
            int side = rng.Next(4);
            int spread = 400;

            float sx, sy;

            float halfW = ScreenWidth / 2f + SpawnMargin;
            float halfH = ScreenHeight / 2f + SpawnMargin;

            switch (side)
            {
                case 0: // top
                    sx = playerPos.X + (float)(rng.NextDouble() - 0.5) * spread * 2;
                    sy = playerPos.Y - halfH;
                    break;
                case 1: // bottom
                    sx = playerPos.X + (float)(rng.NextDouble() - 0.5) * spread * 2;
                    sy = playerPos.Y + halfH;
                    break;
                case 2: // left
                    sx = playerPos.X - halfW;
                    sy = playerPos.Y + (float)(rng.NextDouble() - 0.5) * spread * 2;
                    break;
                default: // right
                    sx = playerPos.X + halfW;
                    sy = playerPos.Y + (float)(rng.NextDouble() - 0.5) * spread * 2;
                    break;
            }

            // Clamp inside world bounds
            sx = Math.Max(0, Math.Min(Map.MapTilesWide * Map.TileWidth - 32, sx));
            sy = Math.Max(0, Math.Min(Map.MapTilesTall * Map.TileHeight - 48, sy));

            return new Vector2(sx, sy);
        }

        private EnemyType ChooseEnemyType()
        {
            List<EnemyType> pool = new List<EnemyType> { EnemyType.Melee };

            if (CurrentWave >= 2)
            {
                pool.Add(EnemyType.Projectile);
                pool.Add(EnemyType.Projectile);
            }
            if (CurrentWave >= 3)
            {
                pool.Add(EnemyType.Slam);
            }

            return pool[rng.Next(pool.Count)];
        }


        private void DrawOffscreenIndicators(SpriteBatch spriteBatch, Camera camera)
        {
            const int Pad = 24;

            foreach (Enemy e in enemies)
            {
                Vector2 screenPos = e.Position - camera.Position;
                bool onScreen = screenPos.X > -e.Size.X && screenPos.X < ScreenWidth + e.Size.X &&
                                screenPos.Y > -e.Size.Y && screenPos.Y < ScreenHeight + e.Size.Y;
                if (onScreen) continue;


                float ax = Math.Max(Pad, Math.Min(ScreenWidth - Pad, screenPos.X));
                float ay = Math.Max(Pad, Math.Min(ScreenHeight - Pad, screenPos.Y));


                Color arrowColor = e.Type == EnemyType.Melee ? Color.Red :
                                   e.Type == EnemyType.Projectile ? Color.Orange :
                                                                       Color.Purple;


                float angle = (float)Math.Atan2(screenPos.Y - ay, screenPos.X - ax);
                DrawArrow(spriteBatch, new Vector2(ax, ay), angle, arrowColor);
            }
        }

        private void DrawArrow(SpriteBatch spriteBatch, Vector2 pos, float angle, Color color)
        {

            const int size = 12;
            spriteBatch.Draw(
                pixel,
                new Rectangle((int)pos.X - size / 2, (int)pos.Y - size / 2, size, size),
                null,
                color,
                angle,
                new Vector2(0.5f, 0.5f),
                SpriteEffects.None,
                0f
            );
        }


        private void SpawnCoinDrop(Vector2 pos, int value)
        {

            int count = Math.Max(1, value / 2);
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = new Vector2(
                    (float)(rng.NextDouble() - 0.5) * 40,
                    (float)(rng.NextDouble() - 0.5) * 40);
                coins.Add(new DroppedCoin(pos + offset, value));
            }
        }

        private int CoinValueForType(EnemyType t)
        {
            switch (t)
            {
                case EnemyType.Melee: return 3;
                case EnemyType.Projectile: return 4;
                case EnemyType.Slam: return 6;
                default: return 3;
            }
        }


        public List<Enemy> Enemies => enemies;
    }


    public class Shockwave
    {
        public bool IsAlive = true;

        private Vector2 center;
        private float currentRadius;
        private float maxRadius;
        private float lifetime;
        private float timeAlive;

        public Shockwave(Vector2 origin, float radius, float life)
        {
            center = origin;
            maxRadius = radius;
            lifetime = life;
            currentRadius = 0f;
        }

        public void Update(float elapsed)
        {
            timeAlive += elapsed;
            currentRadius = maxRadius * (timeAlive / lifetime);
            if (timeAlive >= lifetime) IsAlive = false;
        }

        public bool CheckHit(Vector2 playerPos, Vector2 playerSize)
        {
            Vector2 playerCenter = playerPos + playerSize / 2f;
            float dist = Vector2.Distance(center, playerCenter);
            return Math.Abs(dist - currentRadius) < 20f;
        }

        public void Draw(SpriteBatch spriteBatch, Camera camera, Texture2D pixel)
        {
            if (!IsAlive) return;

            float alpha = 1f - (timeAlive / lifetime);
            Color ringColor = Color.Purple * alpha;

            Vector2 screenCenter = center - camera.Position;
            int steps = 32;

            for (int i = 0; i < steps; i++)
            {
                float angle = (float)(Math.PI * 2 / steps * i);
                float px = screenCenter.X + (float)Math.Cos(angle) * currentRadius;
                float py = screenCenter.Y + (float)Math.Sin(angle) * currentRadius;

                spriteBatch.Draw(pixel,
                    new Rectangle((int)px - 3, (int)py - 3, 6, 6),
                    null,
                    ringColor,
                    angle,
                    Vector2.Zero,
                    SpriteEffects.None,
                    0f);
            }
        }
    }


    public class DroppedCoin
    {
        public bool Collected = false;
        public bool Expired = false;
        public int Value;

        public Vector2 Position;

        private float lifetime = 10f;
        private float timeAlive = 0f;
        private const float PickupRadius = 60f;
        private const float AttractRadius = 90f;
        private const float AttractSpeed = 300f;

        public DroppedCoin(Vector2 pos, int value)
        {
            Position = pos;
            Value = value;
        }

        public void Update(float elapsed, Vector2 playerPos)
        {
            timeAlive += elapsed;
            if (timeAlive >= lifetime) { Expired = true; return; }


            Vector2 dir = playerPos - Position;
            float dist = dir.Length();

            if (dist < AttractRadius)
            {
                dir.Normalize();
                Position += dir * AttractSpeed * elapsed;
            }

            if (dist < 20f) Collected = true;
        }

        public void Draw(SpriteBatch spriteBatch, Camera camera, Texture2D pixel)
        {
            if (Collected || Expired) return;

            Vector2 screenPos = Position - camera.Position;


            spriteBatch.Draw(pixel,
                new Rectangle((int)screenPos.X - 6, (int)screenPos.Y - 6, 12, 12),
                Color.Gold);

            spriteBatch.Draw(pixel,
                new Rectangle((int)screenPos.X - 3, (int)screenPos.Y - 3, 6, 6),
                Color.DarkGoldenrod);
        }
    }
}
