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
    public class LightSource
    {
        float Intensity;
        public Vector2 Pos;
        bool IsOverlapped;
        internal List<Ray2> RayList = new List<Ray2>();
        RenderTarget2D LightMap;
        RenderTarget2D LightMapwEffects;
        GraphicsDevice GD;

        public LightSource(Vector2 Pos, float Intensity, Color Col, GraphicsDevice GD)
        {
            // Create the Rendertargets
            LightMap = new RenderTarget2D(GD, (int)Values.WindowSize.X, (int)Values.WindowSize.Y, false,
                GD.DisplayMode.Format, DepthFormat.Depth24);
            LightMapwEffects = new RenderTarget2D(GD, (int)Values.WindowSize.X, (int)Values.WindowSize.Y, false,
                GD.DisplayMode.Format, DepthFormat.Depth24);

            int RayCount = (int)(Intensity / 0.16f);

            // Fill the RayList
            for (int i = 0; i < RayCount; i++)
            {
                double BaseAngle = Math.PI * 2 / RayCount;
                RayList.Add(new Ray2(Pos, new Vector2((float)Math.Cos(BaseAngle * i - Math.PI), (float)Math.Sin(BaseAngle * i - Math.PI)), Col));
            }

            this.Pos = Pos;
            this.Intensity = Intensity;
            this.GD = GD;
        }

        public void Update()
        {
            IsOverlapped = false;

            if (!Master.IsOverlapped(Pos))
            {
                for (int i = 0; i < RayList.Count; i++)
                { RayList[i].Pos = Pos; RayList[i].Update(); }
            }
            else
            {
                IsOverlapped = true;
                for (int i = 0; i < RayList.Count; i++)
                { RayList[i].Pos = Pos; RayList[i].Dir.Normalize(); RayList[i].Dir *= 0.001f; }
            }
        }

        public Texture2D DrawLightmap(SpriteBatch SB)
        {
            GD.SetRenderTarget(LightMap);
            GD.Clear(Color.Black);
            SB.Begin();
            for (int i = 0; i < RayList.Count; i++)
                RayList[i].Draw(SB);
            Master.DrawObjects(SB);
            SB.End();

            GD.SetRenderTarget(LightMapwEffects);
            GD.Clear(Color.FromNonPremultiplied(0, 0, 0, 0));
            SB.Begin(SpriteSortMode.Immediate, BlendState.Additive);
            Assets.LightSource2DShader.Parameters["X"].SetValue(Pos.X);
            Assets.LightSource2DShader.Parameters["Y"].SetValue(Pos.Y);
            Assets.LightSource2DShader.Parameters["Intensity"].SetValue(Intensity);
            Assets.LightSource2DShader.CurrentTechnique.Passes[0].Apply();
            SB.Draw(LightMap, new Rectangle(0, 0, (int)Values.WindowSize.X, (int)Values.WindowSize.Y), Color.White);
            SB.End();

            return LightMapwEffects;
        }
    }
}
