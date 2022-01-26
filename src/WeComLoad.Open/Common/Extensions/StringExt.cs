namespace WeComLoad.Open.Common.Extensions;

public static class StringExt
{
    public static string GetRangStr(this string[] inputs, int index)
    {
        try
        {
            return inputs[index];
        }
        catch (Exception ex)
        {
            return string.Empty;
        }
    }
}
