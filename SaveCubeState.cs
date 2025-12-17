using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;

namespace GameProject6
{
    [Serializable]
    public struct ColorSaveFormat
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public byte A { get; set; }

        public ColorSaveFormat(Color c)
        {
            R = c.R;
            G = c.G;
            B = c.B;
            A = c.A;
        }

        public Color ToColor()
        {
            return new Color(R, G, B, A);
        }
    }

    [Serializable]
    public class SaveCubeState
    {
        public Point3[][][] CubeletPositions {  get; set; }
        public ColorSaveFormat[][][][] CubeletColors {  get; set; }

        public double SolvingTime { get; set; }
        public CubeType CubeType { get; set; }
    }
}
