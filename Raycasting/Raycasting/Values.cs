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

namespace Raycasting
{
    public static class Values
    {
        public static Random RDM = new Random();
        public static Vector2 WindowSize = new Vector2(1360, 768);
        public static float DiagonalWindowDistance = WindowSize.Length();

        public static void Swap<T>(ref T a, ref T b) { T temp; temp = a; a = b; b = temp; }
        public static float DistanceFromLineToPoint(Vector2 Line1, Vector2 Line2, Vector2 Point)
        {
            Vector2 HelpingVector = new Vector2(-(Line1.Y - Line2.Y), Line1.X - Line2.X);
            Vector2 Intersection = IntersectionPoint(Line1, Line2, Point, HelpingVector);
            return (Point - Intersection).Length();
        }
        public static Vector2 IntersectionPoint(Vector2 A1, Vector2 A2, Vector2 B1, Vector2 B2)
        {
            // Nutze hier die Cramerschen Regel:
            return new Vector2( ((B2.X - B1.X) * (A2.X * A1.Y - A1.X * A2.Y) - (A2.X - A1.X) * (B2.X * B1.Y - B1.X * B2.Y)) / 
                                ((B2.Y - B1.Y) * (A2.X - A1.X) - (A2.Y - A1.Y) * (B2.X - B1.X)),

                                ((A1.Y - A2.Y) * (B2.X * B1.Y - B1.X * B2.Y) - (B1.Y - B2.Y) * (A2.X * A1.Y - A1.X * A2.Y)) / 
                                ((B2.Y - B1.Y) * (A2.X - A1.X) - (A2.Y - A1.Y) * (B2.X - B1.X)));
        }
    }
}
