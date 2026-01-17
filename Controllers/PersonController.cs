using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using who_took_it_backend.Models;
using who_took_it_backend.Services;

namespace who_took_it_backend.Controllers;

[ApiController]
[Route("api/persons")]
public class PersonController : ControllerBase
{
    private readonly PersonService _personService;

    public PersonController(PersonService personService)
    {
        _personService = personService;
    }

    // GET /api/persons
    [HttpGet]
    public async Task<ActionResult<List<Person>>> GetAll()
    {
        return Ok(await _personService.GetAllAsync());
    }

    // GET /api/persons/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Person>> GetById(Guid id)
    {
        var person = await _personService.GetAsync(id);
        if (person is null)
            return NotFound();

        return Ok(person);
    }

    // POST /api/persons
    [HttpPost]
    public async Task<ActionResult<Person>> Create([FromBody] Person person)
    {
        var created = await _personService.AddAsync(person);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT /api/persons/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Person person)
    {
        var existing = await _personService.GetAsync(id);
        if (existing is null)
            return NotFound();

        person.Id = id;

        await _personService.UpdateAsync(id, person);
        return NoContent();
    }

    // DELETE /api/persons/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _personService.GetAsync(id);
        if (existing is null)
            return NotFound();

        await _personService.DeleteAsync(id);
        return NoContent();
    }
}