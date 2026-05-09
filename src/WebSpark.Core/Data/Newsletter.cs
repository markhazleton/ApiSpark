using System.Text.Json.Serialization;

namespace WebSpark.Core.Data;

public class Newsletter : BaseEntity
{
    public int PostId { get; set; }
    public bool Success { get; set; }
    [JsonIgnore] public Post Post { get; set; } = null!;
}
