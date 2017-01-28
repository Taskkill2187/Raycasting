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
using System.Threading.Tasks;

namespace Raycasting
{
    public static class Master
    {
        public static bool[,] SolidityMap = new bool[(int)Values.WindowSize.X,(int)Values.WindowSize.Y];
        public static List<LightSource> LightSourceList = new List<LightSource>();
        public static List<Circle2> CircleList = new List<Circle2>();
        public static List<Rectangle2> RectangleList = new List<Rectangle2>();
        public static List<Texture2D> LightMapList = new List<Texture2D>();
        static GraphicsDevice GD;
        static int Timer;

        public static void Load(GraphicsDevice GD)
        {
            for (int i = 0; i < 13; i++)
                CircleList.Add(new Circle2(new Vector2(Values.RDM.Next((int)Values.WindowSize.X),
                    Values.RDM.Next((int)Values.WindowSize.Y)), Values.RDM.Next(10, 25), new Vector2(Values.RDM.Next(-5, 5), Values.RDM.Next(-5, 5))));

            for (int i = 0; i < 15; i++)
                RectangleList.Add(new Rectangle2(new Rectangle(Values.RDM.Next((int)Values.WindowSize.X), Values.RDM.Next((int)Values.WindowSize.Y),
                    Values.RDM.Next(10, 50), Values.RDM.Next(10, 50)), new Vector2(Values.RDM.Next(-5, 5), Values.RDM.Next(-5, 5)), i));
            
            Master.GD = GD;
        }
        public static bool IsOverlapped(Vector2 a)
        {
            for (int i = 0; i < CircleList.Count; i++)
            {
                if ((a - CircleList[i].Pos).LengthSquared() < CircleList[i].Radius * CircleList[i].Radius)
                    return true;
            }
            for (int i = 0; i < RectangleList.Count; i++)
            {
                if (RectangleList[i].Rect.Intersects(new Rectangle((int)a.X, (int)a.Y, 1, 1)))
                    return true;
            }
            return false;
        }
        public static int FindLineCircleIntersections(
            float cx, float cy, float radius,
            Vector2 point1, Vector2 point2,
            out Vector2 intersection1, out Vector2 intersection2)
        {
            float dx, dy, A, B, C, det, t;

            dx = point2.X - point1.X;
            dy = point2.Y - point1.Y;

            A = dx * dx + dy * dy;
            B = 2 * (dx * (point1.X - cx) + dy * (point1.Y - cy));
            C = (point1.X - cx) * (point1.X - cx) +
                (point1.Y - cy) * (point1.Y - cy) -
                radius * radius;

            det = B * B - 4 * A * C;
            if ((A <= 0.0000001) || (det < 0))
            {
                // No real solutions.
                intersection1 = new Vector2(float.NaN, float.NaN);
                intersection2 = new Vector2(float.NaN, float.NaN);
                return 0;
            }
            else if (det == 0)
            {
                // One solution.
                t = -B / (2 * A);
                intersection1 =
                    new Vector2(point1.X + t * dx, point1.Y + t * dy);
                intersection2 = new Vector2(float.NaN, float.NaN);
                return 1;
            }
            else
            {
                // Two solutions.
                t = (float)((-B + Math.Sqrt(det)) / (2 * A));
                intersection1 =
                    new Vector2(point1.X + t * dx, point1.Y + t * dy);
                t = (float)((-B - Math.Sqrt(det)) / (2 * A));
                intersection2 =
                    new Vector2(point1.X + t * dx, point1.Y + t * dy);
                return 2;
            }
        }

        // For per pixel ray-intersection search
        public static void LoadSolidityMap()
        {
            for (int x = 0; x < SolidityMap.GetLength(0); x++)
            {
                for (int y = 0; y < SolidityMap.GetLength(1); y++)
                {
                    SolidityMap[x, y] = IsOverlapped(new Vector2(x, y));
                }
            }
        }
        public static List<Point> BresenhamLine(int x0, int y0, int x1, int y1)
        {
            List<Point> result = new List<Point>();

            if (Math.Abs(x1 - x0) > Values.DiagonalWindowDistance) return result;

            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep)
            {
                Values.Swap(ref x0, ref y0);
                Values.Swap(ref x1, ref y1);
            }
            if (x0 > x1)
            {
                Values.Swap(ref x0, ref x1);
                Values.Swap(ref y0, ref y1);
            }

            int deltax = x1 - x0;
            int deltay = Math.Abs(y1 - y0);
            int error = 0;
            int ystep;
            int y = y0;
            if (y0 < y1) ystep = 1; else ystep = -1;
            for (int x = x0; x <= x1; x++)
            {
                if (steep) result.Add(new Point(y, x));
                else result.Add(new Point(x, y));
                error += deltay;
                if (2 * error >= deltax)
                {
                    y += ystep;
                    error -= deltax;
                }
            }

            return result;
        }
        public static bool IsSolid(int x, int y)
        {
            if (x < 0) return false;
            if (y < 0) return false;
            if (x > SolidityMap.GetLength(0) - 1) return false;
            if (y > SolidityMap.GetLength(1) - 1) return false;

            return SolidityMap[x, y];
        }

        public static void Update()
        {
            Timer++;

            // Logic
            for (int i = 0; i < CircleList.Count; i++)
                CircleList[i].Update();

            for (int i = 0; i < RectangleList.Count; i++)
                RectangleList[i].Update();

            if (Control.WasKeyJustPressed(Keys.N))
                LightSourceList.Add(new LightSource(Control.GetMouseVector(), 400,
                        Color.FromNonPremultiplied(Values.RDM.Next(255), Values.RDM.Next(255), Values.RDM.Next(255), 255), GD));

            if (Control.WasKeyJustPressed(Keys.X))
                LightSourceList.Clear();

            Task[] TaskList = new Task[LightSourceList.Count];
            int j = 0;
            foreach (LightSource Source in LightSourceList)
            { TaskList[j] = Task.Factory.StartNew(Source.Update); j++; }
            Task.WaitAll(TaskList);
        }
        public static void DrawObjects(SpriteBatch SB)
        {
            for (int i = 0; i < CircleList.Count; i++)
                CircleList[i].Draw(SB);

            for (int i = 0; i < RectangleList.Count; i++)
                RectangleList[i].Draw(SB);
        }
        public static void Draw(SpriteBatch SB, GraphicsDevice GD)
        {
            for (int i = 0; i < LightSourceList.Count; i++)
                LightMapList.Add(LightSourceList[i].DrawLightmap(SB));

            GD.SetRenderTarget(null);
            GD.Clear(Color.Black);
            SB.Begin();
            int RayCounter = 0;
            Assets.Blur.CurrentTechnique.Passes[0].Apply();
            for (int i = 0; i < LightSourceList.Count; i++)
            {
                RayCounter += LightSourceList[i].RayList.Count;
                if (!IsOverlapped(LightSourceList[i].Pos))
                    SB.Draw(LightMapList[i], new Rectangle(0, 0, (int)Values.WindowSize.X, (int)Values.WindowSize.Y), Color.White * 0.5f);
            }
            LightMapList.Clear();
            SB.DrawString(Assets.Font, "Total Rays: " + RayCounter.ToString(), new Vector2(12, 12), Color.White);
            FPSCounter.Draw(SB);
            SB.End();
        }
    }
}
