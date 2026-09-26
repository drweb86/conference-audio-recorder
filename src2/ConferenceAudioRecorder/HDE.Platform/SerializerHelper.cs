using System.IO;
using System.Xml.Serialization;

namespace HDE.Platform.Serialization
{
    public static class SerializerHelper
    {
        public static TData Load<TData>(string file)
            where TData : new()
        {
            using (var stream = File.OpenRead(file))
            {
                return (TData)new XmlSerializer(typeof(TData)).Deserialize(stream);
            }
        }

        public static void Save<TData>(TData data, string file)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }

            using (var stream = File.OpenWrite(file))
            {
                new XmlSerializer(typeof(TData)).Serialize(stream, data);
            }
        }
    }
}
