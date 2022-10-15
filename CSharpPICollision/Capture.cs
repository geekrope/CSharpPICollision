using System;
using System.IO;
using System.Text.Json;

namespace CSharpPICollision
{
    struct Capture
    {
        public SerializablePhysicalObject[][] Frames
        {
            get; set;
        }
    }

    [Serializable]
    class SerializablePhysicalObject
    {
        public double Position
        {
            get; set;
        }
    }

    class SerializableWall : SerializablePhysicalObject
    {
        public SerializableWall()
        {
        }
        public SerializableWall(Wall wall)
        {
            Position = wall.Position;
        }
    }

    class SerializableBlock : SerializablePhysicalObject
    {
        public double Size
        {
            get; set;
        }
        public SerializableBlock()
        {
        }
        public SerializableBlock(Block block)
        {
            Position = block.GetPosition();
            Size = block.Size;
        }
    }

    class CaptureSerializer
    {
        public static Capture Load(string fileName)
        {
            if (File.Exists(fileName))
            {
                using (var stream = new FileStream(fileName, FileMode.OpenOrCreate))
                {
                    return JsonSerializer.Deserialize<Capture>(stream, new JsonSerializerOptions(JsonSerializerDefaults.General));
                }
            }
            else
            {
                throw new FileNotFoundException("File not found", fileName);
            }
        }
        public static void Save(Capture data, string fileName)
        {
            using (var stream = new FileStream(fileName, FileMode.Create))
            {
                JsonSerializer.Serialize<Capture>(stream, data, new JsonSerializerOptions(JsonSerializerDefaults.General));
            }
        }
    }
}
