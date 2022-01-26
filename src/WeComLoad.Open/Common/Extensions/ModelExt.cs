namespace WeComLoad.Open.Common.Extensions;

public static class ModelExt
{
    public static T MapFrom<T, TF>(this T to, TF from)
    {
        var typedTo = typeof(T);
        var typeFrom = from.GetType();
        foreach (var f in typeFrom.GetProperties())
        {
            try
            {
                foreach (var t in typedTo.GetProperties())
                {
                    if (t.Name == f.Name && t.PropertyType == f.PropertyType)
                    {
                        t.SetValue(to, f.GetValue(from, null), null);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        return to;
    }
}

