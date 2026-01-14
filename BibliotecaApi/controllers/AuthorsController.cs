using Microsoft.AspNetCore.Mvc;
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
        public IActionResult CreateAuthor([FromBody] Author author)
        {
            if (author == null || string.IsNullOrEmpty(author.name))
            {
                return BadRequest("Author name is required.");
            }

            _context.Authors.Add(author);
            _context.SaveChanges();

            return CreatedAtAction(
                nameof(CreateAuthor),
                new { id = author.id },
                author
            );
        }
    }
}
