using Postgrest.Attributes;
using Postgrest.Models;
using System;
using System.Collections.Generic;
namespace who_took_it_backend.Models;

[Table("Person")]
public class Person : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [Column("last_seen_at")]
    public DateTimeOffset? LastSeenAt { get; set; }
}