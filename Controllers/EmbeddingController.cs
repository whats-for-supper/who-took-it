using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using who_took_it_backend.Models;
using who_took_it_backend.Services;

namespace who_took_it_backend.Controllers;

[ApiController]
[Route("api/embeddings")]
public class EmbeddingController : ControllerBase
{
    private readonly EmbeddingService _embeddingService;

    public EmbeddingController(EmbeddingService embeddingService)
    {
        _embeddingService = embeddingService;
    }

    // GET /api/embeddings
    [HttpGet]
    public async Task<ActionResult<List<Embedding>>> GetAll()
    {
        return Ok(await _embeddingService.GetAllAsync());
    }

    // GET /api/embeddings/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Embedding>> GetById(Guid id)
    {
        var embedding = await _embeddingService.GetAsync(id);
        if (embedding is null)
            return NotFound();

        return Ok(embedding);
    }

    // POST /api/embeddings
    [HttpPost]
    public async Task<ActionResult<Embedding>> Create([FromBody] Embedding embedding)
    {
        var created = await _embeddingService.AddAsync(embedding);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT /api/embeddings/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Embedding embedding)
    {
        var existing = await _embeddingService.GetAsync(id);
        if (existing is null)
            return NotFound();

        // Ensure path id is the source of truth
        embedding.Id = id;

        await _embeddingService.UpdateAsync(id, embedding);
        return NoContent();
    }

    // DELETE /api/embeddings/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _embeddingService.GetAsync(id);
        if (existing is null)
            return NotFound();

        await _embeddingService.DeleteAsync(id);
        return NoContent();
    }
}