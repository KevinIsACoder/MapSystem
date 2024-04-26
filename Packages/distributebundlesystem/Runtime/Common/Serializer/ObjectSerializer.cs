using Runtime.AdvancedBundleSystem.Common.Util;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Runtime.AdvancedBundleSystem.Common.Serializer
{
    public static class ObjectSerializer
    {
        public static void Serialize<T>(T ToSerialized, string outputFile)
        {
            RuntimeUtils.CheckFileAndCreateDirWhenNeeded(outputFile);
            FileStream fs = File.OpenWrite(outputFile);
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fs, ToSerialized);
            }
            catch (SerializationException e)
            {
                LoggerInternal.LogErrorFormat("Failed to serialize: {0}", e.Message);
                throw;
            }
            finally
            {
                fs.Close();
            }
        } 
    }
}
