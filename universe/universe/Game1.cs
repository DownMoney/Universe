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
using Microsoft.Research.Kinect.Nui;
using Coding4Fun.Kinect.Wpf;
using System.Xml;

namespace universe
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    ///

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Cursor _cursor;
        float aspectRatio = 0;
        Texture2D background;
        planet[] planets = new planet[0];
        int scroll = Mouse.GetState().ScrollWheelValue;
        float zoom = 45;
        float scale = 1f;
        Model cursor;
        Vector3 mousePos = new Vector3(0f,0f, 0f);
        Vector3 cameraTarget = Vector3.Zero;
        int selected = 0;
        int needed = 0;
        SpriteFont font;
        Vector2 fontPos = new Vector2(0, 0);
        Runtime nui;
        DateTime time = new DateTime();
        #region Kinect

        float max = 0;
        float min = 99999;

        float xMax = 0;
        float xMin = 99999;
        bool close = false;
        TimeSpan diffTime = new TimeSpan();
        DateTime oldTime = new DateTime();
        DateTime moveTime = DateTime.Now;
        readonly Kinect.Toolbox.SwipeGestureDetector swipeGestureRecognizer = new Kinect.Toolbox.SwipeGestureDetector();

        private void SetupKinect()
        {
            if (Runtime.Kinects.Count == 0)
            {
                this.Window.Title = "No Kinect connected";
            }
            else
            {
                //use first Kinect
                nui = Runtime.Kinects[0];

                //Initialize to do skeletal tracking
                nui.Initialize(RuntimeOptions.UseSkeletalTracking);

                //add event to receive skeleton data
                nui.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(nui_SkeletonFrameReady);
                swipeGestureRecognizer.OnGestureDetected += new Action<string>(swipeGestureRecognizer_OnGestureDetected);
                //to experiment, toggle TransformSmooth between true & false
                // parameters used to smooth the skeleton data
                nui.SkeletonEngine.TransformSmooth = true;
                TransformSmoothParameters parameters = new TransformSmoothParameters();
                parameters.Smoothing = 0.7f;
                parameters.Correction = 0.3f;
                parameters.Prediction = 0.4f;
                parameters.JitterRadius = 1.0f;
                parameters.MaxDeviationRadius = 0.5f;
                nui.SkeletonEngine.SmoothParameters = parameters;

            }
        }

        void swipeGestureRecognizer_OnGestureDetected(string obj)
        {
            if ((obj.Contains("Right")) && (close))
            {
                if (needed > 0)
                {
                    needed--;
                    movePlanet(needed);
                }
            }

            if ((obj.Contains("Left")) && (close))
            {
                if (needed < planets.Length - 1)
                {
                    needed++;
                    movePlanet(needed);
                }
            }
            
        }

        void nui_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {

            SkeletonFrame allSkeletons = e.SkeletonFrame;

            //get the first tracked skeleton
            SkeletonData skeleton = (from s in allSkeletons.Skeletons
                                     where s.TrackingState == SkeletonTrackingState.Tracked
                                     select s).FirstOrDefault();


            if (skeleton != null)
            {
                //set position
                

                var leftScaled = skeleton.Joints[JointID.HandLeft].ScaleTo(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight, 0.5f, 0.5f);

                swipeGestureRecognizer.Add(skeleton.Joints[JointID.HandRight].Position, nui.SkeletonEngine);
                
                if (leftScaled.Position.Z<min)
                {
                    //this.Window.Title = "Closer";
                    min = leftScaled.Position.Z;
                }
                if (leftScaled.Position.Z>max)
                {
                    // this.Window.Title = leftScaled.Position.Z - min;
                    max = leftScaled.Position.Z;
                }


                if (leftScaled.Position.Z - min < 0.2f)
                    close = true;
                else
                {
                    if (max - leftScaled.Position.Z > 0.2f)
                        close = false;
                }

                var rightScaled = skeleton.Joints[JointID.HandRight].ScaleTo(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight, 0.5f, 0.5f);

            /*    diffTime = time.Subtract(oldTime);

                if (diffTime.Seconds == 0)
                {
                    xMax = rightScaled.Position.X;
                  
                }
                if (diffTime.Seconds >= 0.5)
                {
                    xMin = rightScaled.Position.X;
                    oldTime = time;
                }

                if ((Math.Abs(xMax-xMin)>500)&&(Math.Sign(xMax-xMin)==-1)&&(close)&&(DateTime.Now.Subtract(moveTime).Seconds>2))
                {
                    moveTime = DateTime.Now;
                    // this.Window.Title = "Left";
                     if (needed > 0)
                     {
                         needed--;
                         movePlanet(needed);
                     }
                    
                }
                if ((xMax - xMin > 500) && (close) && (DateTime.Now.Subtract(moveTime).Seconds > 2))
                {
                    moveTime = DateTime.Now;
                     //this.Window.Title = "Right";
                     if (needed < planets.Length - 1)
                     {
                         needed++;
                         movePlanet(needed);
                     }
                  
                }*/

                if ((leftScaled.Position.X == 0) && (rightScaled.Position.X == graphics.PreferredBackBufferWidth))
                {
                    if (zoom > 0.5)
                        zoom -= 0.5f;
                    if ((zoom == 0.5) && (scale < 2))
                        scale += 0.1f;

                }

                if (rightScaled.Position.X - leftScaled.Position.X < 25)
                {
                    if (zoom < 85.5)
                        zoom += 0.5f;
                    if ((zoom == 85.5) && (scale > 0.1))
                        scale -= 0.1f;
                }

                //this.Window.Title = close.ToString();
            }
        }
        #endregion

        #region MainGame

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
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
            _cursor = new Cursor(this);
            SetupKinect();
            Components.Add(_cursor);
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        /// 

        private void loadScene(string path)
        {
            XmlTextReader file = new XmlTextReader(path);
            while (file.Read())
            {
                if (file.Depth == 2)
                {
                    string name = file.ReadElementString("Name");
                    string desc = file.ReadElementString("Description");
                    string model = file.ReadElementString("Model");
                    Vector3 rotation = new Vector3((float)Convert.ToDouble(file.ReadElementString("Rotation_X")), (float)Convert.ToDouble(file.ReadElementString("Rotation_Y")), (float)Convert.ToDouble(file.ReadElementString("Rotation_Z")));
                    Vector3 position = new Vector3((float)Convert.ToDouble(file.ReadElementString("Position_X")), (float)Convert.ToDouble(file.ReadElementString("Position_Y")), (float)Convert.ToDouble(file.ReadElementString("Position_Z")));
                    string texture = file.ReadElementString("Texture");
                    double speed = Convert.ToDouble(file.ReadElementString("Speed"));
                    float radious = (float)Convert.ToDouble(file.ReadElementString("Radious"));
                    float size = (float)Convert.ToDouble(file.ReadElementString("Size"));
                    bool update = bool.Parse(file.ReadElementString("Update"));

                    addPlanet(name,model, rotation, position, texture, speed, radious, size, desc,update);
                }
            }
        }

        private void addPlanet(string name,string model, Vector3 rotation, Vector3 position, string texture, double speed, float radious, float scale, string desc, bool update=true)
        {
            Array.Resize<planet>(ref planets, planets.Length + 1);
            planets[planets.Length - 1] = new planet(name, speed,radious,update,scale,desc);
            planets[planets.Length - 1].myModel = Content.Load<Model>(model);
            planets[planets.Length - 1].rotation = rotation;
            planets[planets.Length - 1].position = position;
            planets[planets.Length - 1].texture = Content.Load<Texture2D>(texture);
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            // graphics.ToggleFullScreen();
            graphics.PreferredBackBufferHeight = 768;
            graphics.PreferredBackBufferWidth = 1280;
            graphics.ApplyChanges();

            background = Content.Load<Texture2D>("Textures\\background");
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Fonts\\Courier New");
            
           // addPlanet("Models\\Earth", Vector3.Zero, new Vector3(50f, -80f, -20f), "Textures\\Mercury", 8, 30f, 0.38f);
            //addPlanet("Models\\Earth", Vector3.Zero, new Vector3(50f, -80f, -20f), "Textures\\Venus", 10, 20f, 0.95f);
            //addPlanet("Models\\Earth", new Vector3(-1.2f, 0f, 0f), new Vector3(50f, -80f, -20f), "Textures\\EarthMap",15,30f,1f);
           // addPlanet("Models\\Earth", Vector3.Zero, new Vector3(0f, -10f, 0f), "Textures\\Sun", 0, 0, 2f, false);
            loadScene("solar.xml");

            cursor = Content.Load<Model>("Models\\Star");
            needed = planets.Length - 1;
            aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            if(nui!=null)
            nui.Uninitialize();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            time = DateTime.Now;
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Escape))
                this.Exit();
            if ((Mouse.GetState().ScrollWheelValue > scroll)&&(Mouse.GetState().RightButton == ButtonState.Released))
            {
                scroll = Mouse.GetState().ScrollWheelValue;
                mousePos = new Vector3(Mouse.GetState().X, Mouse.GetState().Y, 0f);
                if (zoom > 0.5)
                    zoom -= 0.5f;
                if ((zoom==0.5)&& (scale < 2))
                    scale += 0.1f;

               
            }
            if ((Mouse.GetState().ScrollWheelValue < scroll)&&(Mouse.GetState().RightButton == ButtonState.Released))
            {
                scroll = Mouse.GetState().ScrollWheelValue;
                mousePos = new Vector3(Mouse.GetState().X, Mouse.GetState().Y, 0f);
                if (zoom < 85.5)
                    zoom += 0.5f;
                if((zoom==85.5)&&(scale>0.1))
                    scale -= 0.1f;
                
            }

          /*  if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
               //cameraTarget=planets[selected].position+Vector3.Transform(cameraTarget, Matrix.CreateRotationY(planets[selected].rotation.Y));
                needed = selected;
                zoom = 45;
                scale = 1;
                rescale(needed);
            }*/

            if ((Mouse.GetState().RightButton == ButtonState.Pressed) && (Mouse.GetState().ScrollWheelValue < scroll))
            {
                scroll = Mouse.GetState().ScrollWheelValue;
                if (needed < planets.Length - 1)
                {
                    needed++;
                    movePlanet(needed);
                }
            }

            if ((Mouse.GetState().RightButton == ButtonState.Pressed) && (Mouse.GetState().ScrollWheelValue > scroll))
            {
                scroll = Mouse.GetState().ScrollWheelValue;
                if (needed > 0)
                { 
                    needed--;
                    movePlanet(needed);
                }
            }
            // TODO: Add your update logic here
            foreach (planet p in planets)
            {
                p.update(gameTime);
            }
            //this.Window.Title = "X: " + Mouse.GetState().X.ToString() + " Y:" + Mouse.GetState().Y.ToString();
            mousePos = new Vector3((70 * (Mouse.GetState().X- graphics.PreferredBackBufferWidth/2)/ graphics.PreferredBackBufferWidth) - 1, 0, (45 * (Mouse.GetState().Y- graphics.PreferredBackBufferHeight/2)/ graphics.PreferredBackBufferHeight) - 1);
           // planets[planets.Length - 1].update(gameTime);
            base.Update(gameTime);
        }
        private void movePlanet(int n)
        {
            zoom = 45;
            scale = 1;
            rescale(n);
        }
        private void rescale(int relative)
        {
            float rScale = planets[relative].scale;
            foreach (planet p in planets)
            {
                p.scale /= rScale;
            }
            planets[relative].scale = 1f;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// 

        // Set the position of the model in world space, and set the rotation.
        private static bool RayIntersectsModel(Ray ray, Model model,
             Matrix worldTransform, Matrix[] absoluteBoneTransforms)
        {
            // Each ModelMesh in a Model has a bounding sphere, so to check for an
            // intersection in the Model, we have to check every mesh.
            foreach (ModelMesh mesh in model.Meshes)
            {
                // the mesh's BoundingSphere is stored relative to the mesh itself.
                // (Mesh space). We want to get this BoundingSphere in terms of world
                // coordinates. To do this, we calculate a matrix that will transform
                // from coordinates from mesh space into world space....
                Matrix world = absoluteBoneTransforms[mesh.ParentBone.Index] * worldTransform;

                // ... and then transform the BoundingSphere using that matrix.
                BoundingSphere sphere = mesh.BoundingSphere.Transform(world);

                // now that the we have a sphere in world coordinates, we can just use
                // the BoundingSphere class's Intersects function. Intersects returns a
                // nullable float (float?). This value is the distance at which the ray
                // intersects the BoundingSphere, or null if there is no intersection.
                // so, if the value is not null, we have a collision.
                if (sphere.Intersects(ray) != null)
                {
                    return true;
                }
            }
            return false;
        }

        // Set the position of the camera in world space, for our view matrix.
        Vector3 cameraPosition = new Vector3(0.0f, 50.0f, 10.0f);

        protected override void Draw(GameTime gameTime)
        {
            string output = planets[needed].name + "\n" + planets[needed].description.Replace("\\n","\n");
            GraphicsDevice.Clear(Color.CornflowerBlue);
            Rectangle r = new Rectangle(0,0,graphics.GraphicsDevice.Viewport.Width,graphics.GraphicsDevice.Viewport.Height);
            spriteBatch.Begin();
            spriteBatch.Draw(background, r, Color.White);
            spriteBatch.DrawString(font,output , fontPos, Color.White,0f,Vector2.Zero,0.5f,SpriteEffects.None,0.5f);
            spriteBatch.End();
            // TODO: Add your drawing code here
            int i = 0;
            
            // Copy any parent transforms.
            foreach (planet p in planets)
            {
                Matrix[] transforms = new Matrix[p.myModel.Bones.Count];
                p.myModel.CopyAbsoluteBoneTransformsTo(transforms);
            
                // Draw the model. A model can have multiple meshes, so loop.
                foreach (ModelMesh mesh in p.myModel.Meshes)
                {                    
                    // This is where the mesh orientation is set, as well 
                    // as our camera and projection.
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        
                        effect.EnableDefaultLighting();
                        effect.Texture = p.texture;
                        effect.TextureEnabled = true;
                       
                        effect.World = transforms[mesh.ParentBone.Index] *
                            Matrix.CreateRotationX(p.rotation.X) * Matrix.CreateRotationZ(p.rotation.Z) * Matrix.CreateRotationY(p.rotation.Y)
                            * Matrix.CreateTranslation(p.position) * Matrix.CreateScale(p.scale) * Matrix.CreateScale(scale) * Matrix.CreateTranslation(-planets[needed].position) * Matrix.CreateRotationY(planets[needed].rotation.Y);
                        
                            effect.View = Matrix.CreateLookAt(cameraPosition,
                           cameraTarget, Vector3.Up);

                        effect.Projection = Matrix.CreatePerspectiveFieldOfView(
                            MathHelper.ToRadians(zoom), aspectRatio,
                            1.0f, 10000.0f) ;

                        if (RayIntersectsModel(_cursor.CalculateCursorRay(effect.Projection, effect.View), p.myModel, effect.World, transforms))
                        {
                            //this.Window.Title = p.name;
                            selected = i;
                        }
                      }
                    // Draw the mesh, using the effects set above.

                    mesh.Draw();
                   
                }
                i++;
            }

          /*  Matrix[] transforms2 = new Matrix[cursor.Bones.Count];
            cursor.CopyAbsoluteBoneTransformsTo(transforms2);

            foreach (ModelMesh mesh in cursor.Meshes)
            {
                // This is where the mesh orientation is set, as well 
                // as our camera and projection.
                
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();                   
                    effect.World = transforms2[mesh.ParentBone.Index] *
                       Matrix.CreateScale(0.0075f) * Matrix.CreateTranslation(mousePos);
                    effect.View = Matrix.CreateLookAt(cameraPosition,
                        Vector3.Zero, Vector3.Up);
                    effect.Projection = Matrix.CreatePerspectiveFieldOfView(
                        MathHelper.ToRadians(zoom), aspectRatio,
                        1.0f, 10000.0f);
                }
                // Draw the mesh, using the effects set above.
                mesh.Draw();
            }*/
            base.Draw(gameTime);
        }
    }
        #endregion
}
