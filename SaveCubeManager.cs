using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;

namespace GameProject6
{
    public class SaveCubeManager
    {
        private static string GetFileName(CubeType cubeType)
        {
            string file = "saveUNKNOWN.json";
            if (cubeType == CubeType.Cube3x3)
            {
                file = "saveCube3x3.json";
            }
            else if (cubeType == CubeType.Cube2x2)
            {
                file = "saveCube2x2.json";
            }
            return file;
        }

        public static void Save(CubeType cubeType, SaveCubeState state)
        {
            string file = GetFileName(cubeType);
            var json = JsonSerializer.Serialize(state, new JsonSerializerOptions{WriteIndented = true});
            File.WriteAllText(file, json);
        }

        public static SaveCubeState Load(CubeType cubeType)
        {
            string file = GetFileName(cubeType);
            if (!File.Exists(file))
            {
                return null;
            }
            var json = File.ReadAllText(file);
            return JsonSerializer.Deserialize<SaveCubeState>(json);
        }

        public static bool HasSave(CubeType cubeType)
        {
            string file = GetFileName(cubeType);
            return File.Exists(file);
        }

        public static void Clear(CubeType cubeType)
        {
            string file = GetFileName(cubeType);
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
    }
}
