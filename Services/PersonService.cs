using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using who_took_it_backend.Models;

namespace who_took_it_backend.Services;

public class PersonService
{
    private readonly HttpClient _http;
    private readonly string _table;

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public PersonService(IConfiguration config, HttpClient http)
    {
        var supabaseUrl = config["Supabase:Url"] ?? "";
        var supabaseServiceKey = config["Supabase:ServiceRoleKey"] ?? "";

        if (string.IsNullOrWhiteSpace(supabaseUrl) || string.IsNullOrWhiteSpace(supabaseServiceKey))
        {
            throw new InvalidOperationException(
                "Supabase is not configured. Set Supabase:Url and Supabase:ServiceRoleKey in appsettings.Development.json (and ensure it's not committed)."
            );
        }

        _http = http;
        _http.BaseAddress = new Uri(supabaseUrl);

        // Required by Supabase PostgREST
        _http.DefaultRequestHeaders.Remove("apikey");
        _http.DefaultRequestHeaders.Add("apikey", supabaseServiceKey);
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", supabaseServiceKey);

        // Return inserted/updated rows
        _http.DefaultRequestHeaders.Remove("Prefer");
        _http.DefaultRequestHeaders.Add("Prefer", "return=representation");

        // IMPORTANT: match your Supabase table name exactly
        _table = "Person";
    }

    public async Task<List<Person>> GetAllAsync()
    {
        var resp = await _http.GetAsync($"/rest/v1/{_table}?select=*");
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Person>>(json, JsonOptions) ?? new List<Person>();
    }

    public async Task<Person?> GetAsync(Guid id)
    {
        var resp = await _http.GetAsync($"/rest/v1/{_table}?id=eq.{id}&select=*&limit=1");
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync();
        var list = JsonSerializer.Deserialize<List<Person>>(json, JsonOptions);
        return list is null || list.Count == 0 ? null : list[0];
    }

    public async Task<Person> AddAsync(Person person)
    {
        // Minimal payload; let Supabase defaults generate id/created_at
        var payloadObj = new Dictionary<string, object?>();

        if (person.LastSeenAt is not null)
            payloadObj["last_seen_at"] = person.LastSeenAt;

        var payload = JsonSerializer.Serialize(payloadObj, JsonOptions);
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var resp = await _http.PostAsync($"/rest/v1/{_table}", content);
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync();
        var list = JsonSerializer.Deserialize<List<Person>>(json, JsonOptions);
        return list is not null && list.Count > 0 ? list[0] : person;
    }

    public async Task<Person?> UpdateAsync(Guid id, Person person)
    {
        var payloadObj = new Dictionary<string, object?>
        {
            ["last_seen_at"] = person.LastSeenAt
        };

        var payload = JsonSerializer.Serialize(payloadObj, JsonOptions);
        var req = new HttpRequestMessage(new HttpMethod("PATCH"), $"/rest/v1/{_table}?id=eq.{id}")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        var resp = await _http.SendAsync(req);
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync();
        var list = JsonSerializer.Deserialize<List<Person>>(json, JsonOptions);
        return list is not null && list.Count > 0 ? list[0] : null;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var req = new HttpRequestMessage(HttpMethod.Delete, $"/rest/v1/{_table}?id=eq.{id}");
        var resp = await _http.SendAsync(req);
        return resp.IsSuccessStatusCode;
    }
}