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
using System.Diagnostics;

namespace Raycasting
{
    public interface Element2D { void Update(); void Draw(SpriteBatch SB); }

    public class Ray2 : Element2D
    {
        public Vector2 Pos, Dir, DirFrac;
        Color Col;

        public Ray2(Vector2 Pos, Vector2 Dir, Color Col)
        {
            this.Pos = Pos;
            this.Dir = Dir;
            Dir.Normalize();
            Dir *= Values.DiagonalWindowDistance;

            if (Dir.X == 0)
                Dir.X = 0.000001f;

            if (Dir.Y == 0)
                Dir.Y = 0.000001f;
            
            DirFrac = new Vector2(1/Dir.X, 1/Dir.Y);

            if (Dir.X == 0.000001f)
                Dir.X = 0;

            if (Dir.Y == 0.000001f)
                Dir.Y = 0;

            this.Col = Col;
        }

        public void CheckIntersection(Rectangle2 R)
        {
            float L = Dir.Length();
            Dir.Normalize();
            // Box Axis Intersections
            float t0x = (R.Rect.X -                 Pos.X) * DirFrac.X;
            float t1x = (R.Rect.X + R.Rect.Width -  Pos.X) * DirFrac.X;
            float t0y = (R.Rect.Y -                 Pos.Y) * DirFrac.Y;
            float t1y = (R.Rect.Y + R.Rect.Height - Pos.Y) * DirFrac.Y;

            float tmin = Math.Max(Math.Min(t0x, t1x), Math.Min(t0y, t1y));
            float tmax = Math.Min(Math.Max(t0x, t1x), Math.Max(t0y, t1y));
            float t = Values.DiagonalWindowDistance;

            if (tmax < 0)
            {
                Dir *= L;
                return;
            }

            if (tmin > tmax)
            {
                Dir *= L;
                return;
            }

            t = tmin;
            if (t > 0)
            {
                if (t * Values.DiagonalWindowDistance < L)
                    Dir *= t * Values.DiagonalWindowDistance;
                else
                    Dir *= L;
            }
        }
        public void CheckIntersection(Circle2 C)
        {
            //Vector2 HelpingVector = new Vector2(-(Pos.Y - (Dir + Pos).Y), Pos.X - (Dir + Pos).X);
            //Vector2 Intersection = Values.IntersectionPoint(Pos, Dir + Pos, C.Pos, HelpingVector);
            //if (Vector2.Dot(Dir, Pos - C.Pos) < 0 && (Intersection - C.Pos).Length() < C.Radius && (Intersection - Pos).Length() < Dir.Length())
            //{
            //    Dir.Normalize();
            //    Dir *= (Pos - C.Pos).Length();
            //}
            Vector2 I1, I2;
            int a = Master.FindLineCircleIntersections(C.Pos.X, C.Pos.Y, C.Radius, Pos, Pos+Dir, out I1, out I2);
            if (a > 0 && Vector2.Dot(Pos - C.Pos, Dir) < 0 && (Pos - I2).LengthSquared() < Dir.LengthSquared())
            {
                Dir.Normalize();
                float b = (Pos - I2).Length();
                if (b > 0)
                    Dir *= b;
            }
        }
        public void PixelIntersection()
        {
            List<Point> rayLine = Master.BresenhamLine((int)Pos.X, (int)Pos.Y, (int)(Pos+Dir).X, (int)(Pos+Dir).Y);
            if (rayLine.Count > 0)
            {
                for (int i = 0; i < rayLine.Count; i++)
                {
                    Point rayPoint = rayLine[i];
                    if (Master.IsSolid(rayPoint.X, rayPoint.Y) && (new Vector2(rayPoint.X, rayPoint.Y) - Pos).Length() < Dir.Length())
                    {
                        Dir.Normalize();
                        Dir *= (new Vector2(rayPoint.X, rayPoint.Y) - Pos).Length();
                        break;
                    }
                }
            }
        }

        public void Update()
        {
            Dir.Normalize();
            Dir *= Values.DiagonalWindowDistance * 3;

            for (int i = 0; i < Master.RectangleList.Count; i++)
                CheckIntersection(Master.RectangleList[i]);

            for (int i = 0; i < Master.CircleList.Count; i++)
                CheckIntersection(Master.CircleList[i]);
        }
        public void Draw(SpriteBatch SB) { Assets.DrawLine(Pos, Dir + Pos, 1, Col, SB); }
    }

    public class Circle2 : Element2D
    {
        public Vector2 Pos, Vel;
        public float Radius;

        public Circle2(Vector2 Pos, float Radius, Vector2 Vel)
        {
            this.Pos = Pos;
            this.Radius = Radius;
            this.Vel = Vel;
        }

        public void Update()
        {
            Pos += Vel;

            if (Pos.X < Radius)
            { Vel.X *= -1; Pos.X = Radius; }

            if (Pos.X > Values.WindowSize.X - Radius)
            { Vel.X *= -1; Pos.X = Values.WindowSize.X - Radius; }

            if (Pos.Y < Radius)
            { Vel.Y *= -1; Pos.Y = Radius; }

            if (Pos.Y > Values.WindowSize.Y - Radius)
            { Vel.Y *= -1; Pos.Y = Values.WindowSize.Y - Radius; }
        }
        public void Draw(SpriteBatch SB) { Assets.DrawCircle(Pos, Radius+0.5f, Color.White, SB); }
    }
    public class Rectangle2 : Element2D
    {
        public Rectangle Rect;
        public Vector2 Vel;
        public int i;

        public Rectangle2(Rectangle Rect, Vector2 Vel0, int i)
        {
            this.Rect = Rect;
            Vel = Vel0;
            this.i = i;
        }

        public void Update()
        {
            Rect = new Rectangle(Rect.X + (int)Vel.X, Rect.Y + (int)Vel.Y, Rect.Width, Rect.Height);

            if (Rect.X < 0)
            { Vel.X *= -1; Rect.X = 0; }

            if (Rect.X + Rect.Width > (int)Values.WindowSize.X)
            { Vel.X *= -1; Rect.X = (int)Values.WindowSize.X - Rect.Width; }

            if (Rect.Y < 0)
            { Vel.Y *= -1; Rect.Y = 0; }

            if (Rect.Y + Rect.Height > (int)Values.WindowSize.Y)
            { Vel.Y *= -1; Rect.Y = (int)Values.WindowSize.Y - Rect.Height; }
        }
        public void Draw(SpriteBatch SB) { SB.Draw(Assets.White, Rect, Color.White); }
    }
}
