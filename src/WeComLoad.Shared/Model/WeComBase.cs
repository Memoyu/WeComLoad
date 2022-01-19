namespace WeComLoad.Shared.Model;

public class WeComBase<T>
{
    [JsonProperty("data")]
    public T Data { get; set; }
}
