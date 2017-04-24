using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using ScrollingBackgroundSpace;


namespace Step8
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        KeyboardState oldState;
        bool endgame = false;
        bool win = false;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            // change back to 1000!
            graphics.PreferredBackBufferHeight = 740;
            graphics.PreferredBackBufferWidth = 1280;
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
            oldState = Keyboard.GetState();

        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        /// 

        SpriteFont scoreFont;
        int score = 0;

        SpriteFont levelFont;
        int level = 1;
        int next_enemy_shot = 0;

        SpriteFont endFont;

        Texture2D background;
        Rectangle mainframe;
        Vector2 originBackground;

        Texture2D character;
        Vector2 characterPosition;
        Vector2 characterSpeed = new Vector2(0.0f, 0.0f);
        int characterHeight;
        int characterWidth;

        Texture2D bullet;
        Texture2D enemyBullet;
        List<Vector3> bulletPosition = new List<Vector3>();
        List<Vector3> bulletSpeed = new List<Vector3>();
        List<Vector3> enemyBulletPosition = new List<Vector3>();
        List<Vector3> enemyBulletSpeed = new List<Vector3>();
        int bulletHeight;
        int bulletWidth;
        int bulletMax = 99;
        int enemyBulletHeight;
        int enemyBulletWidth;
        int enemyBulletMax = 99;

        Texture2D enemy;
        List<Vector2> enemyPosition = new List<Vector2>();
        List<Vector2> enemySpeed = new List<Vector2>();
        int enemyHeight;
        int enemyWidth;
        int totalEnemies = 4;

        //Sound effect for Game Music
        SoundEffect gameMusic;
        SoundEffectInstance gameMusicInstance;

        //Sound effect for firing bullets
        SoundEffect bulletFire;
        SoundEffectInstance bulletFireInstance;

        //Sound effect for bullet hits
        SoundEffect bulletHit;
        SoundEffectInstance bulletHitInstance;

        bool firebullet = false;
        bool hitTarget = false;

        private ScrollingBackground myBackground;

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures

            spriteBatch = new SpriteBatch(GraphicsDevice);
            myBackground = new ScrollingBackground();
            background = Content.Load<Texture2D>("background1");
            myBackground.Load(GraphicsDevice, background);

            mainframe = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            originBackground = new Vector2(mainframe.X, mainframe.Y);

            scoreFont = Content.Load<SpriteFont>("SpriteFont1");
            levelFont = Content.Load<SpriteFont>("SpriteFont1");
            endFont = Content.Load<SpriteFont>("SpriteFont2");

            character = Content.Load<Texture2D>("blossom");
            bullet = Content.Load<Texture2D>("redslash");

            enemy = Content.Load<Texture2D>("mojo1");
            enemyBullet = Content.Load<Texture2D>("wave1");
 
            //Initialize Game Music
            gameMusic = Content.Load<SoundEffect>("game");
            gameMusicInstance = gameMusic.CreateInstance();
            //Initialize bullet Fire Sound Effect
            bulletFire = Content.Load<SoundEffect>("RocketSound");
            bulletFireInstance = bulletFire.CreateInstance();
            //Initialize bullet Hit Sound Effect
            bulletHit = Content.Load<SoundEffect>("EnemyExplosion");
            bulletHitInstance = bulletHit.CreateInstance();

            characterHeight = character.Bounds.Height;
            characterWidth = character.Bounds.Width;

            characterPosition.X = 10;
            characterPosition.Y = graphics.GraphicsDevice.Viewport.Height/2-characterHeight;

            bulletHeight = bullet.Bounds.Height;
            bulletWidth = bullet.Bounds.Width;

            enemyBulletHeight = enemyBullet.Bounds.Height;
            enemyBulletWidth = enemyBullet.Bounds.Width;

            enemyHeight = enemy.Bounds.Height;
            enemyWidth = enemy.Bounds.Width;

            int enemySpacing = graphics.GraphicsDevice.Viewport.Width / totalEnemies;

            for (int i = 0; i < totalEnemies; i++)
            {
                enemyPosition.Add(new Vector2(0, enemySpacing * i / 2));
                enemySpeed.Add(new Vector2(50.0f, 0));
            }

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allow the game to exit

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            //Play bullet fire sound
            if (firebullet)
            {
                bulletFireInstance.Play();
                firebullet = false;
            }
            //Play bullet hit target sound
            if (hitTarget)
            {
                bulletHitInstance.Play();
                hitTarget = false;
            }
            
           
            if (gameMusicInstance.State == SoundState.Stopped)
                gameMusicInstance.Play();

            // The time since Update was called last.
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            myBackground.Update(elapsed * -10);
            // Move the sprite around
            Updatecharacter(gameTime);
            Updatebullet(gameTime);
            UpdateEnemies(gameTime);
            CheckForCollision();

            base.Update(gameTime);
        }

        //Qichao Wang Update
        void Updatecharacter(GameTime gameTime)
        {

            // Move the sprite by speed, scaled by elapsed time 
            KeyboardState newState = Keyboard.GetState();

            if (newState.IsKeyDown(Keys.Up) && oldState.IsKeyUp(Keys.Down) && oldState.IsKeyUp(Keys.Left) && oldState.IsKeyUp(Keys.Right))
            {
                characterSpeed.Y = -200.0f;
            }

            else if (newState.IsKeyDown(Keys.Down) && oldState.IsKeyUp(Keys.Up) && oldState.IsKeyUp(Keys.Left) && oldState.IsKeyUp(Keys.Right))
            {
                characterSpeed.Y = 200.0f;
            }
            else if (newState.IsKeyDown(Keys.Left) && oldState.IsKeyUp(Keys.Down) && oldState.IsKeyUp(Keys.Up) && oldState.IsKeyUp(Keys.Right))
            {
                characterSpeed.X = -200.0f;
            }
            else if (newState.IsKeyDown(Keys.Right) && oldState.IsKeyUp(Keys.Down) && oldState.IsKeyUp(Keys.Left) && oldState.IsKeyUp(Keys.Up))
            {
                characterSpeed.X = 200.0f;
            }
            else if (newState.IsKeyDown(Keys.Down) && newState.IsKeyDown(Keys.Left) && oldState.IsKeyUp(Keys.Up) && oldState.IsKeyUp(Keys.Right))
            {
                characterSpeed.Y = 120.0f;
                characterSpeed.X = -120.0f;
            }

            else if (newState.IsKeyDown(Keys.Down) && newState.IsKeyDown(Keys.Right) && oldState.IsKeyUp(Keys.Up) && oldState.IsKeyUp(Keys.Left))
            {
                characterSpeed.Y = 120.0f;
                characterSpeed.X = 120.0f;
            }
            else if (newState.IsKeyDown(Keys.Up) && newState.IsKeyDown(Keys.Left) && oldState.IsKeyUp(Keys.Down) && oldState.IsKeyUp(Keys.Right))
            {
                characterSpeed.Y = -120.0f;
                characterSpeed.X = -120.0f;
            }
            else if (newState.IsKeyDown(Keys.Up) && newState.IsKeyDown(Keys.Right) && oldState.IsKeyUp(Keys.Down) && oldState.IsKeyUp(Keys.Left))
            {
                characterSpeed.Y = -120.0f;
                characterSpeed.X = 120.0f;
            }
            else
            {
                characterSpeed.Y = 0.0f;
                characterSpeed.X = 0.0f;
            }

            characterPosition += characterSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            int MaxX = graphics.GraphicsDevice.Viewport.Width - character.Width;
            int MinX = 0;
            int MaxY = graphics.GraphicsDevice.Viewport.Height - character.Height;
            int MinY = 0;

            // Check for bounce 

            if (characterPosition.X > MaxX)
            {

                characterSpeed.X = 0;
                characterPosition.X = MaxX;
            }

            else if (characterPosition.X < MinX)
            {

                characterSpeed.X = 0;
                characterPosition.X = MinX;
            }

            else if (characterPosition.Y > MaxY)
            {
                characterSpeed.Y = 0;
                characterPosition.Y = MaxY;
            }
            else if (characterPosition.Y < MinY)
            {
                characterSpeed.Y = 0;
                characterPosition.Y = MinY;
            }

        }

        void Updatebullet(GameTime gameTime)
        {
            int MaxX = graphics.GraphicsDevice.Viewport.Width;

            KeyboardState newState = Keyboard.GetState();

            if (newState.IsKeyDown(Keys.Space) && oldState.IsKeyUp(Keys.Space))
            {
                //Stop any current bullet fire sound
                if (bulletFireInstance.State == SoundState.Playing)
                    bulletFireInstance.Stop();
                //Tells update to play bullet sound(much faster than playing in keypress!)
                firebullet = true;
            

                if (bulletPosition.Count < bulletMax)
                {
                    bulletPosition.Add(new Vector3(characterPosition.X + characterWidth, characterPosition.Y + bulletHeight, 0));
                    bulletSpeed.Add(new Vector3(500.0f, 0, 0));
                }
            }


            for (int i = 0; i < bulletPosition.Count; i++)
            {
                bulletPosition[i] += bulletSpeed[i] * (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (bulletPosition[i].X > MaxX)
                {
                    bulletPosition.RemoveAt(i);
                    bulletSpeed.RemoveAt(i);
                }
               
            }
            oldState = newState;
        }

        void UpdateEnemies(GameTime gameTime)
        {
            int MaxY = graphics.GraphicsDevice.Viewport.Height;
            int MinY = 0;
            Vector2 enemyShiftX = new Vector2(50.0f, 0.0f);

            for (int i = 0; i < totalEnemies; i++)
            {
                enemyPosition[i] += enemySpeed[i] * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (enemyPosition[i].Y > MaxY - enemyHeight)
                {
                    for (int j = 0; j < totalEnemies; j++)
                    {
                        enemySpeed[j] *= -1;
                        if (enemyPosition[j].X < characterPosition.X)
                        {
                            enemyPosition[j] -= enemyShiftX;
                        }
                        else
                        {
                            endgame = true;
                        }
                    } 
                }
                else if (enemyPosition[i].Y < MinY)
                {
                    for (int j = 0; j < totalEnemies; j++)
                    {
                        enemySpeed[j] *= -1;
                        if (enemyPosition[j].X < characterPosition.X)
                        {
                            enemyPosition[j] += enemyShiftX;
                        }
                        else
                        {
                            endgame = true;
                        }
                    }
                }
            }
            if ( next_enemy_shot > 400 / level )
            {
                Random rnd = new Random();
                AddEnemyShot(enemyPosition[rnd.Next(0, totalEnemies)]);
                next_enemy_shot = 0;
            }
            next_enemy_shot++;
        }

        void AddEnemyShot(Vector2 enemyPosition)
        {
            if (bulletFireInstance.State == SoundState.Playing)
                bulletFireInstance.Stop();
            //Tells update to play bullet sound(much faster than playing in keypress!)
            firebullet = true;

            enemyBulletPosition.Add(new Vector3(enemyPosition.X + enemyWidth / 2 - bulletWidth / 2, enemyPosition.Y + (1.5f*enemyHeight), -1));
            enemyBulletSpeed.Add(new Vector3(0, 250.0f, 0));
        }


        void CheckForCollision()
        {
            List<BoundingBox> enemyBoxes = new List<BoundingBox>();
            List<BoundingBox> bulletBoxes = new List<BoundingBox>();
            List<BoundingBox> enemybulletBoxes = new List<BoundingBox>();
            List<BoundingBox> spaceCraftBoxes = new List<BoundingBox>();
            bool round_cleared = false;

            for (int i = 0; i < totalEnemies; i++)
            {
                enemyBoxes.Add(new BoundingBox(new Vector3(enemyPosition[i].X - (enemyWidth / 2), enemyPosition[i].Y - (enemyHeight / 2), 0), new Vector3(enemyPosition[i].X + (enemyWidth / 2), enemyPosition[i].Y + (enemyHeight / 2), 0)));
            }
            int userbullets = 0;
            int enemybullets = 0;
            for (int j = 0; j < bulletPosition.Count; j++)
            {
                if (bulletPosition[j].Z == 0)
                {
                    bulletBoxes.Add(new BoundingBox(new Vector3(bulletPosition[j].X - (bulletWidth / 2), bulletPosition[j].Y - (bulletHeight / 2), 0), new Vector3(bulletPosition[j].X + (bulletWidth / 2), bulletPosition[j].Y + (bulletHeight / 2), 0)));
                    userbullets++;
                }
                else
                {
                    enemybulletBoxes.Add(new BoundingBox(new Vector3(bulletPosition[j].X - (bulletWidth / 2), bulletPosition[j].Y - (bulletHeight / 2), 0), new Vector3(bulletPosition[j].X + (bulletWidth / 2), bulletPosition[j].Y + (bulletHeight / 2), 0)));
                    enemybullets++;
                }
            }

            for (int i = 0; i < totalEnemies; i++)
            {
                for (int j = 0; j < userbullets; j++)
                {
                    if (!round_cleared && enemyBoxes[i].Intersects(bulletBoxes[j]))
                    {
                        if (bulletHitInstance.State == SoundState.Playing)
                            bulletHitInstance.Stop();
                        hitTarget = true;
                        bulletHitInstance.Play();

                        enemyPosition.RemoveAt(i);
                        enemySpeed.RemoveAt(i);
                        bulletPosition.RemoveAt(j);
                        bulletSpeed.RemoveAt(j);
                        totalEnemies--;

                        // defeated all the enemies
                        if (!enemyPosition.Any())
                        {
                            round_cleared = true;
                            // go to the next level
                            level++;
                            // add more enemies once you beat the level
                            totalEnemies = 4;

                            int enemySpacing = graphics.GraphicsDevice.Viewport.Width / totalEnemies;

                            for (int k = 0; k < totalEnemies; k++)
                            {
                                enemyPosition.Add(new Vector2(enemySpacing * k, 0));
                                enemySpeed.Add(new Vector2(-50.0f * (level / 1.8f), 0));
                            }
                        }

                        score++;
                    }
                }
            }
            BoundingBox characterBox = new BoundingBox(new Vector3(characterPosition.X - (characterWidth / 2), characterPosition.Y - (characterHeight / 2), 0), new Vector3(characterPosition.X + (characterWidth / 2), characterPosition.Y + (characterHeight / 2), 0));
            for (int i = 0; i < enemybullets; i++)
            {
                if (characterBox.Intersects(enemybulletBoxes[i]))
                {
                    // TODO END OF GAME!!
                    endgame = true;
                }
            }
            if (!round_cleared)
            {
                for (int i = 0; i < totalEnemies; i++)
                {
                    if (characterBox.Intersects(enemyBoxes[i]))
                    {
                        // TODO END OF GAME!!
                        endgame = true;
                    }
                }
            }
              
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
            
            // Draw the sprite
           
            spriteBatch.Begin();

            //spriteBatch.Draw(background, mainframe, sourceBackground,  Color.White, 0,originBackground,SpriteEffects.None, 1);
            //spriteBatch.Draw(background,mainframe,Color.White);
            myBackground.Draw(spriteBatch);
            
            spriteBatch.DrawString(scoreFont, "SCORE " + score.ToString(), new Vector2(10, 10), Color.Red);

            spriteBatch.DrawString(levelFont, "LEVEL " + level.ToString(), new Vector2(GraphicsDevice.Viewport.Width-200, 10), Color.Red);

            if (endgame)
            {
                if (win)
                    spriteBatch.DrawString(endFont, "You Win", new Vector2(GraphicsDevice.Viewport.Width/4+30, 300), Color.Red);
                else
                    spriteBatch.DrawString(endFont, "GAME OVER", new Vector2(GraphicsDevice.Viewport.Width / 4 + 30, 300), Color.Red);
            }

            spriteBatch.Draw(character, characterPosition, Color.White);


            for (int i = 0; i < bulletPosition.Count; i++)
            {
                Vector2 temp = new Vector2(bulletPosition[i].X, bulletPosition[i].Y);
                spriteBatch.Draw(bullet, temp, Color.White);
            }

            for (int i = 0; i < totalEnemies; i++)
            {
                spriteBatch.Draw(enemy, enemyPosition[i], Color.White);
            }

            if (level == 4)
            {
                enemy = Content.Load<Texture2D>("Enemy_character2");
            }
            if (level == 7)
            {
                enemy = Content.Load<Texture2D>("Enemy_character3");
            }
            
            spriteBatch.End();

            //spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            //spriteBatch.Draw(texture2, spritePosition2, null, Color.White, 2*RotationAngle, origin2, 1.0f, SpriteEffects.None, 0f);
            //spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
