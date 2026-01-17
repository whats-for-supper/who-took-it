using Postgrest.Attributes;
using Postgrest.Models;

namespace who_took_it_backend.Models;

[Table("Embedding")]
public class Embedding : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; }

    [Column("person_id")]
    public Guid PersonId { get; set; }

    [Column("vector")]
    public string Vector { get; set; } = "[]";

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

}