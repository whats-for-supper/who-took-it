using who_took_it_backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers (so /api/... controllers work)
builder.Services.AddControllers();

// OpenAPI (new .NET template)
builder.Services.AddOpenApi();

// CORS (allow your frontend to call this API from the browser)
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// Supabase-backed services using HttpClient + IConfiguration
builder.Services.AddHttpClient<PersonService>();
builder.Services.AddHttpClient<EmbeddingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.MapControllers();

app.Run();
