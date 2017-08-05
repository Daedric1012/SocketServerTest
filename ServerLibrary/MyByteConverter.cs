
namespace ServerLibrary
{
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    public class MyByteConverter
    {
        // Convert a byte array to an Object
        public static object ByteArrayToObject(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                if (memStream.Length != 0)
                {
                    var obj = bf.Deserialize(memStream);
                    return obj;
                }

                return null;
            }
        }

        // convert object to byte array
        public static byte[] ObjectToByteArray(object obj)
        {
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }
}