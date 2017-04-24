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
        Nullable<Rectangle> sourceBackground = null;
        Vector2 originBackground;

        Texture2D ship;
        Vector2 shipPosition;
        Vector2 shipSpeed = new Vector2(0.0f, 0.0f);
        int shipHeight;
        int shipWidth;

        Texture2D rocket;
        List<Vector3> rocketPosition = new List<Vector3>();
        List<Vector3> rocketSpeed = new List<Vector3>();
        int rocketHeight;
        int rocketWidth;
        int rocketMax = 99;

        Texture2D enemy;
        List<Vector2> enemyPosition = new List<Vector2>();
        List<Vector2> enemySpeed = new List<Vector2>();
        int enemyHeight;
        int enemyWidth;
        int totalEnemies = 4;

        SoundEffect soundEffect;
        SoundEffect music;

        SoundEffectInstance soundInstance;
        SoundEffectInstance musicInstance;

        //Sound effect for Galaga Game Music
        SoundEffect galagaMusic;
        SoundEffectInstance galagaMusicInstance;

        //Sound effect for firing rockets
        SoundEffect rocketFire;
        SoundEffectInstance rocketFireInstance;

        //Sound effect for rocket hits
        SoundEffect rocketHit;
        SoundEffectInstance rocketHitInstance;

        bool fireRocket = false;
        bool hitTarget = false;

        bool collide = false;

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures

            spriteBatch = new SpriteBatch(GraphicsDevice);

            background = Content.Load<Texture2D>("background");
            mainframe = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            originBackground = new Vector2(mainframe.X, mainframe.Y);

            scoreFont = Content.Load<SpriteFont>("SpriteFont1");
            levelFont = Content.Load<SpriteFont>("SpriteFont1");
            endFont = Content.Load<SpriteFont>("SpriteFont2");

            ship = Content.Load<Texture2D>("Galaga_ship");
            rocket = Content.Load<Texture2D>("rocket");
            enemy = Content.Load<Texture2D>("Enemy_Ship");

            soundEffect = Content.Load<SoundEffect>("Swords_Collide-Sound");
            music = Content.Load<SoundEffect>("siren");

            soundInstance = soundEffect.CreateInstance();
            musicInstance = music.CreateInstance();

            //Initialize Galaga Music
            galagaMusic = Content.Load<SoundEffect>("Galaga");
            galagaMusicInstance = galagaMusic.CreateInstance();
            //Initialize Rocket Fire Sound Effect
            rocketFire = Content.Load<SoundEffect>("RocketSound");
            rocketFireInstance = rocketFire.CreateInstance();
            //Initialize Rocket Hit Sound Effect
            rocketHit = Content.Load<SoundEffect>("RocketHit");
            rocketHitInstance = rocketHit.CreateInstance();

            //musicInstance.IsLooped = true;

            shipHeight = ship.Bounds.Height;
            shipWidth = ship.Bounds.Width;

            shipPosition.X = 10;
            shipPosition.Y = graphics.GraphicsDevice.Viewport.Height/2-shipHeight;

            rocketHeight = rocket.Bounds.Height;
            rocketWidth = rocket.Bounds.Width;

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
            //Play rocket fire sound
            if (fireRocket)
            {
                rocketFireInstance.Play();
                fireRocket = false;
            }
            //Play rocket hit target sound
            if (hitTarget)
            {
                rocketHitInstance.Play();
                hitTarget = false;
            }
            
           
            if (galagaMusicInstance.State == SoundState.Stopped)
                galagaMusicInstance.Play();

            // The time since Update was called last.
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Move the sprite around
            UpdateShip(gameTime, ref shipPosition, ref shipSpeed);
            UpdateRocket(gameTime, ref rocketPosition, ref rocketSpeed);
            UpdateEnemies(gameTime, ref enemyPosition, ref enemySpeed);
            CheckForCollision();

            base.Update(gameTime);
        }

        void UpdateShip(GameTime gameTime, ref Vector2 shipPosition, ref Vector2 shipSpeed)
        {

            // Move the sprite by speed, scaled by elapsed time 
            KeyboardState newState = Keyboard.GetState();

            if (newState.IsKeyDown(Keys.Up) && oldState.IsKeyUp(Keys.Down) && oldState.IsKeyUp(Keys.Left) && oldState.IsKeyUp(Keys.Right))
            {
                shipSpeed.Y = -200.0f;
            }

            else if (newState.IsKeyDown(Keys.Down) && oldState.IsKeyUp(Keys.Up) && oldState.IsKeyUp(Keys.Left) && oldState.IsKeyUp(Keys.Right))
            {
                shipSpeed.Y = 200.0f;
            }
            else if (newState.IsKeyDown(Keys.Left) && oldState.IsKeyUp(Keys.Down) && oldState.IsKeyUp(Keys.Up) && oldState.IsKeyUp(Keys.Right))
            {
                shipSpeed.X = -200.0f;
            }
            else if (newState.IsKeyDown(Keys.Right) && oldState.IsKeyUp(Keys.Down) && oldState.IsKeyUp(Keys.Left) && oldState.IsKeyUp(Keys.Up))
            {
                shipSpeed.X = 200.0f;
            }
            else if (newState.IsKeyDown(Keys.Down) && newState.IsKeyDown(Keys.Left) && oldState.IsKeyUp(Keys.Up) && oldState.IsKeyUp(Keys.Right))
            {
                shipSpeed.Y = 120.0f;
                shipSpeed.X = -120.0f;
            }

            else if (newState.IsKeyDown(Keys.Down) && newState.IsKeyDown(Keys.Right) && oldState.IsKeyUp(Keys.Up) && oldState.IsKeyUp(Keys.Left))
            {
                shipSpeed.Y = 120.0f;
                shipSpeed.X = 120.0f;
            }
            else if (newState.IsKeyDown(Keys.Up) && newState.IsKeyDown(Keys.Left) && oldState.IsKeyUp(Keys.Down) && oldState.IsKeyUp(Keys.Right))
            {
                shipSpeed.Y = -120.0f;
                shipSpeed.X = -120.0f;
            }
            else if (newState.IsKeyDown(Keys.Up) && newState.IsKeyDown(Keys.Right) && oldState.IsKeyUp(Keys.Down) && oldState.IsKeyUp(Keys.Left))
            {
                shipSpeed.Y = -120.0f;
                shipSpeed.X = 120.0f;
            }
            else
            {
                shipSpeed.Y = 0.0f;
                shipSpeed.X = 0.0f;
            }

            shipPosition += shipSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            int MaxX = graphics.GraphicsDevice.Viewport.Width - ship.Width;
            int MinX = 0;
            int MaxY = graphics.GraphicsDevice.Viewport.Height - ship.Height;
            int MinY = 0;

            // Check for bounce 

            if (shipPosition.X > MaxX)
            {

                shipSpeed.X = 0;
                shipPosition.X = MaxX;
            }

            else if (shipPosition.X < MinX)
            {

                shipSpeed.X = 0;
                shipPosition.X = MinX;
            }

            else if (shipPosition.Y > MaxY)
            {
                shipSpeed.Y = 0;
                shipPosition.Y = MaxY;
            }
            else if (shipPosition.Y < MinY)
            {
                shipSpeed.Y = 0;
                shipPosition.Y = MinY;
            }

        }

        void UpdateRocket(GameTime gameTime, ref List<Vector3> rocketPosition, ref List<Vector3> rocketSpeed)
        {
            int MaxX = graphics.GraphicsDevice.Viewport.Width;
            int MinX = 0;
            int MaxY = graphics.GraphicsDevice.Viewport.Height;
            int MinY = 0;


            // Move the sprite by speed, scaled by elapsed time 
            KeyboardState newState = Keyboard.GetState();

            if (newState.IsKeyDown(Keys.Space) && oldState.IsKeyUp(Keys.Space))
            {
                //Stop any current rocket fire sound
                if (rocketFireInstance.State == SoundState.Playing)
                    rocketFireInstance.Stop();
                //Tells update to play rocket sound(much faster than playing in keypress!)
                fireRocket = true;
            

                if (rocketPosition.Count < rocketMax)
                {
                    rocketPosition.Add(new Vector3(shipPosition.X + shipWidth/2 - rocketWidth/2, shipPosition.Y, 0));
                    rocketSpeed.Add(new Vector3(0, -500.0f, 0));
                }
            }


            for (int i = 0; i < rocketPosition.Count; i++)
            {
                rocketPosition[i] += rocketSpeed[i] * (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (rocketPosition[i].Y > MaxY)
                {
                    rocketPosition.RemoveAt(i);
                    rocketSpeed.RemoveAt(i);
                }
                else if (rocketPosition[i].Y < MinY)
                {
                    rocketPosition.RemoveAt(i);
                    rocketSpeed.RemoveAt(i);
                }
            }
            oldState = newState;
        }

        void UpdateEnemies(GameTime gameTime, ref List<Vector2> enemyPosition, ref List<Vector2> enemySpeed)
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
                        if (enemyPosition[j].X < shipPosition.X)
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
                        if (enemyPosition[j].X < shipPosition.X)
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
            if (rocketFireInstance.State == SoundState.Playing)
                rocketFireInstance.Stop();
            //Tells update to play rocket sound(much faster than playing in keypress!)
            fireRocket = true;

            rocketPosition.Add(new Vector3(enemyPosition.X + enemyWidth / 2 - rocketWidth / 2, enemyPosition.Y + (1.5f*enemyHeight), -1));
            rocketSpeed.Add(new Vector3(0, 250.0f, 0));
        }


        void CheckForCollision()
        {
            List<BoundingBox> enemyBoxes = new List<BoundingBox>();
            List<BoundingBox> rocketBoxes = new List<BoundingBox>();
            List<BoundingBox> enemyrocketBoxes = new List<BoundingBox>();
            List<BoundingBox> spaceCraftBoxes = new List<BoundingBox>();
            bool round_cleared = false;

            for (int i = 0; i < totalEnemies; i++)
            {
                enemyBoxes.Add(new BoundingBox(new Vector3(enemyPosition[i].X - (enemyWidth / 2), enemyPosition[i].Y - (enemyHeight / 2), 0), new Vector3(enemyPosition[i].X + (enemyWidth / 2), enemyPosition[i].Y + (enemyHeight / 2), 0)));
            }
            int userRockets = 0;
            int enemyRockets = 0;
            for (int j = 0; j < rocketPosition.Count; j++)
            {
                if (rocketPosition[j].Z == 0)
                {
                    rocketBoxes.Add(new BoundingBox(new Vector3(rocketPosition[j].X - (rocketWidth / 2), rocketPosition[j].Y - (rocketHeight / 2), 0), new Vector3(rocketPosition[j].X + (rocketWidth / 2), rocketPosition[j].Y + (rocketHeight / 2), 0)));
                    userRockets++;
                }
                else
                {
                    enemyrocketBoxes.Add(new BoundingBox(new Vector3(rocketPosition[j].X - (rocketWidth / 2), rocketPosition[j].Y - (rocketHeight / 2), 0), new Vector3(rocketPosition[j].X + (rocketWidth / 2), rocketPosition[j].Y + (rocketHeight / 2), 0)));
                    enemyRockets++;
                }
            }

            for (int i = 0; i < totalEnemies; i++)
            {
                for (int j = 0; j < userRockets; j++)
                {
                    if (!round_cleared && enemyBoxes[i].Intersects(rocketBoxes[j]))
                    {
                        if (rocketHitInstance.State == SoundState.Playing)
                            rocketHitInstance.Stop();
                        hitTarget = true;
                        //rocketHitInstance.Play();

                        enemyPosition.RemoveAt(i);
                        enemySpeed.RemoveAt(i);
                        rocketPosition.RemoveAt(j);
                        rocketSpeed.RemoveAt(j);
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
            BoundingBox shipBox = new BoundingBox(new Vector3(shipPosition.X - (shipWidth / 2), shipPosition.Y - (shipHeight / 2), 0), new Vector3(shipPosition.X + (shipWidth / 2), shipPosition.Y + (shipHeight / 2), 0));
            for (int i = 0; i < enemyRockets; i++)
            {
                if (shipBox.Intersects(enemyrocketBoxes[i]))
                {
                    // TODO END OF GAME!!
                    endgame = true;
                }
            }
            if (!round_cleared)
            {
                for (int i = 0; i < totalEnemies; i++)
                {
                    if (shipBox.Intersects(enemyBoxes[i]))
                    {
                        // TODO END OF GAME!!
                        endgame = true;
                    }
                }
            }
                


            //BoundingBox bb1 = new BoundingBox(new Vector3(spritePosition1.X - (sprite1Width / 2), spritePosition1.Y - (sprite1Height / 2), 0), new Vector3(spritePosition1.X + (sprite1Width / 2), spritePosition1.Y + (sprite1Height / 2), 0));

            //BoundingBox bb2 = new BoundingBox(new Vector3(spritePosition2.X - (sprite2Width / 2), spritePosition2.Y - (sprite2Height / 2), 0), new Vector3(spritePosition2.X + (sprite2Width / 2), spritePosition2.Y + (sprite2Height / 2), 0));

            //if (bb1.Intersects(bb2))
            //{
            //    spriteSpeed1.X *= -1;
            //    spriteSpeed1.Y *= -1;

            //    spriteSpeed2.X *= -1;
              //  spriteSpeed2.Y *= -1;

                //score++;

                //collide = true;

                //musicInstance.Stop();
                
                //soundInstance.Play();

            //}
            //else
            //{
              //  if(musicInstance.State != SoundState.Playing && soundInstance.State != SoundState.Playing && collide)
                //{
                  //  musicInstance.Play();
                    //collide = false;
               // }
            //}
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
            
            // Draw the sprite
           
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            
            spriteBatch.Draw(background, mainframe, sourceBackground,  Color.White, 0,originBackground,SpriteEffects.None, 1);
            //spriteBatch.Draw(background,mainframe,Color.White);
            
            
            spriteBatch.DrawString(scoreFont, "SCORE " + score.ToString(), new Vector2(10, 10), Color.White);

            spriteBatch.DrawString(levelFont, "LEVEL " + level.ToString(), new Vector2(510, 10), Color.White);

            if (endgame)
            {
                spriteBatch.DrawString(endFont, "GAME OVER", new Vector2(30, 250), Color.White);
            }

            spriteBatch.Draw(ship, shipPosition, Color.White);


            for (int i = 0; i < rocketPosition.Count; i++)
            {
                Vector2 temp = new Vector2(rocketPosition[i].X, rocketPosition[i].Y);
                spriteBatch.Draw(rocket, temp, Color.White);
            }

            for (int i = 0; i < totalEnemies; i++)
            {
                spriteBatch.Draw(enemy, enemyPosition[i], Color.White);
            }

            if (level == 4)
            {
                enemy = Content.Load<Texture2D>("Enemy_Ship2");
            }
            if (level == 7)
            {
                enemy = Content.Load<Texture2D>("Enemy_Ship3");
            }
            
            spriteBatch.End();

            //spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            //spriteBatch.Draw(texture2, spritePosition2, null, Color.White, 2*RotationAngle, origin2, 1.0f, SpriteEffects.None, 0f);
            //spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
