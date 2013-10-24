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

namespace universe
{
    class planet
    {
        public Model myModel;
        public Vector3 position=Vector3.Zero;
        public Vector3 rotation = Vector3.Zero;
        public Texture2D texture;
        private double speed = 100;
        public float radious = 15f;
        public bool _update = true;
        public float scale = 1f;
        public string name = "";
        public string description = "";
        Curve3D c = new Curve3D();

        struct Curve3D
        {
           public Curve x;
            public Curve y;
            public Curve z;
        }

        public planet(string _name,double s, float r, bool u, float sc, string desc)
        {
            description = desc;
            name = _name;
            scale = sc;
            speed = s;
            radious = r;
            _update = u;
            c.x = new Curve();
            c.y = new Curve();
            c.z = new Curve();
            c.z.PostLoop = CurveLoopType.Oscillate;
            c.x.PostLoop = CurveLoopType.Oscillate;
            c.y.PostLoop = CurveLoopType.Oscillate;
            addKey(new Vector3(0f, 0f, 0f), 0f);
            addKey(new Vector3(0f, -8f, 0f), 10f);
            addKey(new Vector3(0f, -10f, 0f), 20f);
        }

        private void addKey(Vector3 v, float time)
        {
            c.z.Keys.Add(new CurveKey(v.Z, time));
            c.x.Keys.Add(new CurveKey(v.X, time));
            c.y.Keys.Add(new CurveKey(v.Y, time));
        }

        private Vector3 getPoint(float time)
        {
            return new Vector3(c.x.Evaluate(time), c.y.Evaluate(time), c.z.Evaluate(time));
        }

        public void rotate(Vector3 _rotation)
        {
            rotation += _rotation;
        }

        public void move(Vector3 _position)
        {
            position += _position;
        }

        private void _move(Vector3 _position)
        {
            position = _position;
        }

        

        public void update(GameTime g)
        {
            
            rotate(new Vector3(0.0f, 0.00f, 0.03f));
            if (_update)
            {
                float angle = (float)(g.TotalGameTime.TotalSeconds * speed);

                float x = radious * (float)Math.Cos((double)MathHelper.ToRadians(angle));
                float y = radious * (float)Math.Sin((double)MathHelper.ToRadians(angle));
                _move(new Vector3(x, y, 0));
            }
        }
    }
}
