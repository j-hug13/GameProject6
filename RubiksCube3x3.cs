using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameProject6
{
    public struct Point3
    {
        public int X, Y, Z;
        public Point3(int x, int y, int z) { X = x; Y = y; Z = z; }
    }

    public class RubiksCube3x3
    {
        // === Fields and stuff for Building Cube === //
        public BasicEffect Effect => effect;
        public Matrix World { get; set; } = Matrix.Identity;

        private GraphicsDevice graphicsDevice;
        private BasicEffect effect;
        private Cubelet[,,] cubelets = new Cubelet[3, 3, 3];
        private class Cubelet
        {
            public Point3 GridPos;
            public Color[] FaceColors;
            public Vector3[] FaceNormals = new Vector3[]
            {
                Vector3.UnitY,      // Top
                -Vector3.UnitY,     // Bottom
                -Vector3.UnitX,     // Left
                Vector3.UnitX,      // Right
                Vector3.UnitZ,      // Front
                -Vector3.UnitZ,     // Back
            };
            public VertexBuffer Buffer;
        }
        // =========================================== //

        // === Fields for Layer Rotation === //
        public enum CubeAxis { X, Y, Z, None }
        public bool IsRotating => isRotating;
        public float rotationSpeed = MathHelper.Pi * 2.0f;

        private bool isRotating = false;
        private CubeAxis currentAxis;
        private int currentLayer;
        private float rotationAngle = 0f;
        private int rotationDirection = 1;
        // ================================= //

        // === Fields for Layer Selection === //
        public bool IsLayerSelected { get; set; } = false;
        public CubeAxis SelectedAxis { get; private set; }
        public int SelectedLayer { get; private set; }
        public struct HitInfo
        {
            public bool Hit;
            public float Distance;
            public Point3 GridPos;
            public Vector3 HitNormal;
        }
        // ================================= //

        // === Fields for Cube Scrambiling === //
        public struct CubeMove
        {
            public CubeAxis Axis;
            public int Layer;
            public int Direction;
        }

        private readonly Queue<CubeMove> scrambleQueue = new Queue<CubeMove>();
        private readonly Random r = new Random();
        private const int ScrambleMoves = 1;
        private CubeAxis lastAxis = CubeAxis.None;
        private int lastLayer;
        private const float ScrambleSpeed = MathHelper.Pi * 10.0f;
        // =================================== //

        public RubiksCube3x3(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            effect = new BasicEffect(graphicsDevice)
            {
                VertexColorEnabled = true,
                LightingEnabled = false
            };

            BuildCube();
        }

        #region Create The Cube
        private void BuildCube()
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        var colors = InitFaceColors(x, y, z);
                        Vector3[] initialNormals = new Vector3[]
                        {
                            Vector3.UnitY,      // Top
                            -Vector3.UnitY,     // Bottom
                            -Vector3.UnitX,     // Left
                            Vector3.UnitX,      // Right
                            Vector3.UnitZ,      // Front
                            -Vector3.UnitZ,     // Back
                        };
                        var verts = CreateRubiksCubelet(new Point3(x, y, z), 1f, colors, initialNormals);

                        cubelets[x + 1, y + 1, z + 1] = new Cubelet
                        {
                            GridPos = new Point3(x, y, z),
                            FaceColors = colors,
                            FaceNormals = initialNormals,
                            Buffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionColor), verts.Length, BufferUsage.WriteOnly)
                        };

                        cubelets[x + 1, y + 1, z + 1].Buffer.SetData(verts);
                    }
                }
            }
        }

        private Color[] InitFaceColors(int x, int y, int z)
        {
            return new Color[]
            {
                y ==  1 ? Color.Red    : Color.Black,
                y == -1 ? Color.Orange : Color.Black,
                x == -1 ? Color.Blue   : Color.Black,
                x ==  1 ? Color.Green  : Color.Black,
                z ==  1 ? Color.White  : Color.Black,
                z == -1 ? Color.Yellow : Color.Black
            };
        }

        private VertexPositionColor[] CreateRubiksCubelet(Point3 position, float size, Color[] faceColors, Vector3[] faceNormals)
        {
            Color baseColor = Color.Black;
            List<VertexPositionColor> verts = new List<VertexPositionColor>();

            verts.AddRange(CreateColoredCube(size * 0.9f, baseColor));

            float stickerOffset = size / 2f * 0.95f;
            float stickerSize = size * 0.85f;

            for (int i = 0; i < 6; i++)
            {
                Color color = faceColors[i];
                Vector3 normal = faceNormals[i];

                if (color == Color.Black)
                {
                    continue;
                }

                Vector3 up = Vector3.Up;
                Vector3 right = Vector3.Right;

                if (Math.Abs(Vector3.Dot(normal, Vector3.Up)) > 0.9f)
                {
                    up = Vector3.Forward;
                    right = Vector3.Right;
                }
                else if (Math.Abs(Vector3.Dot(normal, Vector3.Forward)) > 0.9f)
                {
                    up = Vector3.Up;
                    right = Vector3.Right;
                }
                else if (Math.Abs(Vector3.Dot(normal, Vector3.Right)) > 0.9f)
                {
                    up = Vector3.Up;
                    right = Vector3.Forward;
                }

                Vector3 center = normal * stickerOffset;

                Vector3 a = center + (-right - up) * stickerSize * 0.5f;
                Vector3 b = center + (-right + up) * stickerSize * 0.5f;
                Vector3 c = center + (right + up) * stickerSize * 0.5f;
                Vector3 d = center + (right - up) * stickerSize * 0.5f;

                verts.Add(new VertexPositionColor(a, color));
                verts.Add(new VertexPositionColor(b, color));
                verts.Add(new VertexPositionColor(c, color));
                verts.Add(new VertexPositionColor(a, color));
                verts.Add(new VertexPositionColor(c, color));
                verts.Add(new VertexPositionColor(d, color));
            }

            return verts.ToArray();
        }

        private VertexPositionColor[] CreateColoredCube(float size, Color color)
        {
            float half = size / 2f;
            Vector3[] corners = new Vector3[]
            {
                new Vector3(-half, -half, -half),
                new Vector3(-half,  half, -half),
                new Vector3( half,  half, -half),
                new Vector3( half, -half, -half),
                new Vector3(-half, -half,  half),
                new Vector3(-half,  half,  half),
                new Vector3( half,  half,  half),
                new Vector3( half, -half,  half),
            };

            int[,] faces = new int[,]
            {
                {0,1,2,3}, {5,4,7,6}, {4,5,1,0}, {3,2,6,7}, {1,5,6,2}, {4,0,3,7}
            };

            List<VertexPositionColor> v = new List<VertexPositionColor>();

            for (int i = 0; i < 6; i++)
            {
                int a = faces[i, 0], b = faces[i, 1], c = faces[i, 2], d = faces[i, 3];
                v.Add(new VertexPositionColor(corners[a], color));
                v.Add(new VertexPositionColor(corners[b], color));
                v.Add(new VertexPositionColor(corners[c], color));
                v.Add(new VertexPositionColor(corners[a], color));
                v.Add(new VertexPositionColor(corners[c], color));
                v.Add(new VertexPositionColor(corners[d], color));
            }

            return v.ToArray();
        }
        #endregion

        #region Select and Rotate a Layer
        private List<Cubelet> GetLayer(CubeAxis axis, int layerIndex)
        {
            List<Cubelet> layer = new List<Cubelet>();
            foreach (var c in cubelets)
            {
                if (axis == CubeAxis.X && c.GridPos.X == layerIndex)
                {
                    layer.Add(c);
                }
                if (axis == CubeAxis.Y && c.GridPos.Y == layerIndex)
                {
                    layer.Add(c);
                }
                if (axis == CubeAxis.Z && c.GridPos.Z == layerIndex)
                {
                    layer.Add(c);
                }
            }
            return layer;
        }

        private void RotateFaceColors(Cubelet c, Matrix rotation)
        {
            Vector3[] targetAxes = new Vector3[]
            {
                Vector3.UnitY, -Vector3.UnitY, -Vector3.UnitX, Vector3.UnitX, Vector3.UnitZ, -Vector3.UnitZ
            };

            Color[] newColors = new Color[6];

            for (int i = 0; i < 6; i++)
            {
                Vector3 originalNormal = targetAxes[i];
                Vector3 newNormal = Vector3.Transform(originalNormal, rotation);

                int newFaceIndex = -1;
                float maxDotProduct = -1f;

                for (int j = 0; j < targetAxes.Length; j++)
                {
                    float dot = Vector3.Dot(newNormal, targetAxes[j]);
                    if (dot > maxDotProduct && dot > 0.99f)
                    {
                        maxDotProduct = dot;
                        newFaceIndex = j;
                    }
                }

                if (newFaceIndex != -1)
                {
                    newColors[newFaceIndex] = c.FaceColors[i];
                }
                else
                {
                    newColors[i] = c.FaceColors[i];
                }
            }

            c.FaceColors = newColors;
        }

        public void StartRotation(CubeAxis axis, int layer, int direction)
        {
            if (isRotating == true)
            {
                return;
            }

            isRotating = true;
            currentAxis = axis;
            currentLayer = layer;
            rotationDirection = direction;
            rotationAngle = 0f;
        }

        private void ApplyRotationToLayer(CubeAxis axis, int layer, int d)
        {
            var cubeletsInLayer = GetLayer(axis, layer);

            Matrix r = axis switch
            {
                CubeAxis.X => Matrix.CreateRotationX(MathHelper.PiOver2 * d),
                CubeAxis.Y => Matrix.CreateRotationY(MathHelper.PiOver2 * d),
                CubeAxis.Z => Matrix.CreateRotationZ(MathHelper.PiOver2 * d),
                _ => Matrix.Identity
            };

            foreach (var c in cubeletsInLayer)
            {
                Vector3 pos = Vector3.Transform(new Vector3(c.GridPos.X, c.GridPos.Y, c.GridPos.Z), r);
                c.GridPos = new Point3((int)Math.Round(pos.X), (int)Math.Round(pos.Y), (int)Math.Round(pos.Z));

                RotateFaceColors(c, r);

                UpdateCubeletBuffer(c);
            }

            RebuildCubeArray();
        }

        private void RebuildCubeArray()
        {
            var newCubelets = new Cubelet[3, 3, 3];
            foreach (var c in cubelets)
            {
                newCubelets[c.GridPos.X + 1, c.GridPos.Y + 1, c.GridPos.Z + 1] = c;
            }
            cubelets = newCubelets;
        }

        private void UpdateCubeletBuffer(Cubelet c)
        {
            var verts = CreateRubiksCubelet(new Point3(0, 0, 0), 1f, c.FaceColors, c.FaceNormals);
            c.Buffer.SetData(verts);
        }

        public HitInfo Intersect(Ray ray)
        {
            HitInfo closestHit = new HitInfo 
            { 
                Hit = false, 
                Distance = float.MaxValue 
            };
            float size = 1f;

            foreach (var c in cubelets)
            {
                Vector3 cubeletPos = new Vector3(c.GridPos.X, c.GridPos.Y, c.GridPos.Z);

                Vector3 min = cubeletPos - new Vector3(size / 2f);
                Vector3 max = cubeletPos + new Vector3(size / 2f);
                BoundingBox box = new BoundingBox(min, max);

                float? distance = ray.Intersects(box);

                if (distance != null && distance.Value < closestHit.Distance)
                {
                    Vector3 hitPoint = ray.Position + ray.Direction * distance.Value;

                    Vector3 normal = Vector3.Normalize(hitPoint - cubeletPos);

                    Vector3 hitNormal = SnapToAxis(normal);

                    closestHit = new HitInfo
                    {
                        Hit = true,
                        Distance = distance.Value,
                        GridPos = c.GridPos,
                        HitNormal = hitNormal
                    };
                }
            }
            return closestHit;
        }

        private Vector3 SnapToAxis(Vector3 v)
        {
            float absX = Math.Abs(v.X);
            float absY = Math.Abs(v.Y);
            float absZ = Math.Abs(v.Z);

            if (absX > absY && absX > absZ)
            {
                return v.X > 0 ? Vector3.UnitX : -Vector3.UnitX;
            }
            if (absY > absX && absY > absZ)
            {
                return v.Y > 0 ? Vector3.UnitY : -Vector3.UnitY;
            }

            return v.Z > 0 ? Vector3.UnitZ : -Vector3.UnitZ;
        }

        public void SelectLayer(Point3 gridPos, Vector3 hitNormal)
        {
            if (isRotating == true)
            {
                return;
            }

            // Check for top and bottom
            if (Math.Abs(hitNormal.Y) > 0.9f)
            {
                SelectedAxis = CubeAxis.Y;
                SelectedLayer = gridPos.Y;
            }
            // Check for left and right
            else if (Math.Abs(hitNormal.X) > 0.9f)
            {
                SelectedAxis = CubeAxis.X;
                SelectedLayer = gridPos.X;
            }
            // Check for front and back
            else if (Math.Abs(hitNormal.Z) > 0.9f)
            {
                SelectedAxis = CubeAxis.Z;
                SelectedLayer = gridPos.Z;
            }

            IsLayerSelected = true;
        }

        // FINALLY WORKS, DO NOT FUCK WITH THIS FUTURE JOHNNY
        public int GetAdjustedDirection(int keyboardDirection, Vector3 cameraPosition)
        {
            Vector3 layerVector = SelectedAxis switch
            {
                CubeAxis.X => Vector3.UnitX * SelectedLayer,
                CubeAxis.Y => Vector3.UnitY * SelectedLayer,
                CubeAxis.Z => Vector3.UnitZ * SelectedLayer,
                _ => Vector3.Zero
            };

            Vector3 vectorToCamera = Vector3.Normalize(cameraPosition - Vector3.Zero);

            int axisBaseFlip = 1;
            if (SelectedAxis == CubeAxis.Y)
            {
                axisBaseFlip = Math.Sign(SelectedLayer);
            }

            float alignment = Vector3.Dot(layerVector, vectorToCamera);

            int perspectiveFlip = 1;
            if (alignment > 0)
            {
                perspectiveFlip = -1;
            }

            int negativeSideFlip = 1;
            if (SelectedLayer < 0)
            {
                if (SelectedAxis == CubeAxis.X || SelectedAxis == CubeAxis.Z)
                {
                    negativeSideFlip = -1;
                }
            }

            return keyboardDirection * axisBaseFlip * perspectiveFlip * negativeSideFlip;
        }
        #endregion

        #region Scramble The Cube
        public void Scramble()
        {
            if (IsRotating == true)
            {
                return;
            }

            scrambleQueue.Clear();
            rotationSpeed = ScrambleSpeed;

            rotationAngle = 0;

            for(int i = 0; i < ScrambleMoves; i++)
            {
                CubeAxis nextAxis;
                int nextLayer;
                int nextDirection;

                do {
                    nextAxis = (CubeAxis)r.Next(3);
                } while (nextAxis == lastAxis);

                // either -1 or 1
                nextLayer = r.Next(2) * 2 - 1;
                nextDirection = r.Next(2) * 2 - 1;

                CubeMove nextMove = new CubeMove
                {
                    Axis = nextAxis,
                    Layer = nextLayer,
                    Direction = nextDirection
                };
                scrambleQueue.Enqueue(nextMove);

                lastAxis = nextAxis;
                lastLayer = nextLayer;
            }

            ProcessScrambleQueue();
        }

        private void ProcessScrambleQueue()
        {
            if (scrambleQueue.Count == 0)
            {
                rotationSpeed = GameScene.NormalRotationSpeed;
                return;
            }

            CubeMove nextMove = scrambleQueue.Dequeue();

            StartRotation(nextMove.Axis, nextMove.Layer, nextMove.Direction);
        }
        #endregion

        #region Check If Cube is Solved
        public bool IsSolved()
        {
            int[,] faces = new int[,]
            {
                { 1, 0, 1 },    // Top face
                { -1, 1, 1 },   // Bottom face
                { -1, 2, 0 },   // Left face
                { 1, 3, 0 },    // Right face
                { 1, 4, 2 },    // Front face
                { -1, 5, 2 }    // Back face
            };

            for(int i=0; i<faces.GetLength(0); i++)
            {
                int layerIndex = faces[i, 0];
                int normalIndex = faces[i, 1];
                CubeAxis axis = (CubeAxis)faces[i, 2];

                Point3 center = new Point3(axis == CubeAxis.X ? layerIndex : 0, axis == CubeAxis.Y ? layerIndex : 0, axis == CubeAxis.Z ? layerIndex : 0);
                Cubelet centerCube = cubelets[center.X + 1, center.Y + 1, center.Z + 1];
                Color targetColor = centerCube.FaceColors[normalIndex];

                List<Cubelet> cubeletsInLayer = GetLayer(axis, layerIndex);
                foreach (Cubelet c in cubeletsInLayer)
                {
                    Color currentColor = c.FaceColors[normalIndex];
                    if (currentColor != targetColor)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        #endregion

        public void Update(GameTime gameTime)
        {
            if (isRotating == false)
            {
                return;
            }

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            rotationAngle += rotationSpeed * dt * rotationDirection;

            if (Math.Abs(rotationAngle) >= MathHelper.PiOver2)
            {
                ApplyRotationToLayer(currentAxis, currentLayer, rotationDirection);

                rotationAngle = 0f;
                isRotating = false;

                ProcessScrambleQueue();
            }
        }

        public void Draw(BasicEffect effect, Matrix world)
        {
            const float SelectedLayerScale = 1.07f;

            foreach (var c in cubelets)
            {
                Vector3 cubeletPos = new Vector3(c.GridPos.X, c.GridPos.Y, c.GridPos.Z);
                Matrix cubeWorld = Matrix.CreateTranslation(cubeletPos);

                if (isRotating == false && IsLayerSelected == true)
                {
                    bool inSelectedLayer =
                        (SelectedAxis == CubeAxis.X && c.GridPos.X == SelectedLayer) ||
                        (SelectedAxis == CubeAxis.Y && c.GridPos.Y == SelectedLayer) ||
                        (SelectedAxis == CubeAxis.Z && c.GridPos.Z == SelectedLayer);

                    if (inSelectedLayer == true)
                    {
                        cubeWorld *= Matrix.CreateScale(SelectedLayerScale);
                    }
                }

                if (isRotating == true)
                {
                    bool inLayer =
                        (currentAxis == CubeAxis.X && c.GridPos.X == currentLayer) ||
                        (currentAxis == CubeAxis.Y && c.GridPos.Y == currentLayer) ||
                        (currentAxis == CubeAxis.Z && c.GridPos.Z == currentLayer);

                    if (inLayer == true)
                    {
                        Vector3 center = currentAxis switch
                        {
                            CubeAxis.X => new Vector3(currentLayer, 0, 0),
                            CubeAxis.Y => new Vector3(0, currentLayer, 0),
                            CubeAxis.Z => new Vector3(0, 0, currentLayer),
                            _ => Vector3.Zero
                        };

                        Matrix selectedAxis = currentAxis switch
                        {
                            CubeAxis.X => Matrix.CreateRotationX(rotationAngle),
                            CubeAxis.Y => Matrix.CreateRotationY(rotationAngle),
                            CubeAxis.Z => Matrix.CreateRotationZ(rotationAngle),
                            _ => Matrix.Identity
                        };
                        cubeWorld = Matrix.CreateTranslation(cubeletPos - center) * selectedAxis * Matrix.CreateTranslation(center);
                    }
                    else
                    {
                        cubeWorld = Matrix.CreateTranslation(cubeletPos);
                    }
                }

                effect.World = cubeWorld * world;
                effect.CurrentTechnique.Passes[0].Apply();

                graphicsDevice.SetVertexBuffer(c.Buffer);
                graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, c.Buffer.VertexCount / 3);
            }
        }
    }
}
