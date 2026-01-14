using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using BibliotecaApi.Data;
using BibliotecaApi.Models;

namespace BibliotecaApi.Controllers
{
    [ApiController]
    [Route("authors")]
    public class AuthorsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthorsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        // http://localhost:5205/authors
        public IActionResult CreateAuthor([FromBody] JsonElement body)
        {
            // 🔹 Caso 1: veio um ARRAY de autores
            if (body.ValueKind == JsonValueKind.Array)
            {
                var authors = JsonSerializer.Deserialize<List<Author>>(body);

                if (authors == null || !authors.Any())
                    return BadRequest("Author list cannot be empty.");

                if (authors.Any(a => string.IsNullOrWhiteSpace(a.name)))
                    return BadRequest("All authors must have a name.");

                _context.Authors.AddRange(authors);
                _context.SaveChanges();

                return Created("authors", authors);
            }

            // 🔹 Caso 2: veio UM autor
            if (body.ValueKind == JsonValueKind.Object)
            {
                var author = JsonSerializer.Deserialize<Author>(body);

                if (author == null || string.IsNullOrWhiteSpace(author.name))
                    return BadRequest("Author name is required.");

                _context.Authors.Add(author);
                _context.SaveChanges();

                return Created("authors", author);
            }

            return BadRequest("Invalid JSON format.");
        }

        [HttpGet]
        // http://localhost:5205/authors?page=1&pageSize=5
        public IActionResult GetAuthors([FromQuery] int page = 1)
        {
            const int pageSize = 5;

            if (page < 1)
                return BadRequest("Page number must be greater than 0.");

            var totalAuthors = _context.Authors.Count();
            var authors = _context.Authors
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            if (!authors.Any())
                return NotFound("No authors found.");

            var response = new
            {
                page,
                pageSize,
                totalAuthors,
                totalPages = (totalAuthors + pageSize - 1) / pageSize,
                authors
            };

            return Ok(response);
        }

        [HttpPut("{id}")]
        // http://localhost:5205/authors/1
        public IActionResult UpdateAuthor(int id, [FromBody] JsonElement body)
        {
            if (body.ValueKind != JsonValueKind.Object)
                return BadRequest("Invalid JSON format.");

            var author = JsonSerializer.Deserialize<Author>(body);

            if (author == null || string.IsNullOrWhiteSpace(author.name))
                return BadRequest("Author name is required.");

            var existingAuthor = _context.Authors.Find(id);

            if (existingAuthor == null)
                return NotFound("Author not found.");

            existingAuthor.name = author.name;
            _context.SaveChanges();

            return Ok(existingAuthor);
        }

        [HttpDelete("{id}")]
        // http://localhost:5205/authors/1
        public IActionResult DeleteAuthor(int id)
        {
            var author = _context.Authors.Find(id);

            if (author == null)
            return NotFound("Author not found.");

            _context.Authors.Remove(author);
            _context.SaveChanges();

            return NoContent();
        }
    }
}

