namespace MyDailyJ.Api.Dtos;

// Mirrors com.example.poefn.model.JournalEntry in the Android app exactly
// (createdAt/updatedAt are strings there too), used for both request bodies
// and responses. Id/UserId/CreatedAt/UpdatedAt sent by the client on
// create/update are ignored server-side.
public class JournalEntryDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public string? UpdatedAt { get; set; }
}
