using System.IO;

namespace WeComLoad.Open.Blazor.Utils;

public class JsonFileHelper
{

    public static T? ReadJson<T>(string path)
    {
        if (!File.Exists(path)) return default;
        using (StreamReader file = File.OpenText(path))
        {
            try
            {
                string json = file.ReadToEnd();
                var dtos = JsonConvert.DeserializeObject<T>(json);
                return dtos;
            }
            catch (Exception ex)
            {
                return default;
            }
        }

    }

    public static void WriteJson(string path, object data)
    {
        try
        {
            //判断文件是否存在
            if (!File.Exists(path))
            {
                File.Create(path);
            }
            var json = JsonConvert.SerializeObject(data);
            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        { 
            throw;
        }
    }
}
