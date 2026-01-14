using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using BibliotecaApi.Data;
using BibliotecaApi.Models;

namespace BibliotecaApi.Controllers
{
    [ApiController]
    [Route("books")]
    public class BooksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BooksController(AppDbContext context)
        {
            _context = context;
        }

        // POST /books
        [HttpPost]
        public IActionResult CreateBook([FromBody] JsonElement body)
        {
            // Caso 1: veio um ARRAY de livros
            if (body.ValueKind == JsonValueKind.Array)
            {
                var books = JsonSerializer.Deserialize<List<Book>>(body);

                if (books == null || !books.Any())
                    return BadRequest("Book list cannot be empty.");

                if (books.Any(b => string.IsNullOrWhiteSpace(b.title)))
                    return BadRequest("All books must have a title.");

                _context.Books.AddRange(books);
                _context.SaveChanges();

                return Created("books", books);
            }

            // Caso 2: veio UM livro
            if (body.ValueKind == JsonValueKind.Object)
            {
                var book = JsonSerializer.Deserialize<Book>(body);

                if (book == null || string.IsNullOrWhiteSpace(book.title))
                    return BadRequest("Book title is required.");

                _context.Books.Add(book);
                _context.SaveChanges();

                return Created("books", book);
            }

            return BadRequest("Invalid JSON format.");
        }

        // GET /books?id=1
        // GET /books?page=1
        [HttpGet]
        public IActionResult GetBooks([FromQuery] int? id, [FromQuery] int page = 1)
        {
            const int pageSize = 5;

            if (id.HasValue)
            {
                var book = _context.Books.Find(id.Value);

                if (book == null)
                    return NotFound("Book not found.");

                return Ok(book);
            }

            if (page < 1)
                return BadRequest("Page number must be greater than 0.");

            var totalBooks = _context.Books.Count();
            var books = _context.Books
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            if (!books.Any())
                return NotFound("No books found.");

            return Ok(new
            {
                page,
                pageSize,
                totalBooks,
                totalPages = (totalBooks + pageSize - 1) / pageSize,
                books
            });
        }

        // PUT /books/1
        [HttpPut("{id}")]
        public IActionResult UpdateBook(int id, [FromBody] JsonElement body)
        {
            if (body.ValueKind != JsonValueKind.Object)
                return BadRequest("Invalid JSON format.");

            var updatedBook = JsonSerializer.Deserialize<Book>(body);

            if (updatedBook == null || string.IsNullOrWhiteSpace(updatedBook.title))
                return BadRequest("Book title is required.");

            var existingBook = _context.Books.Find(id);

            if (existingBook == null)
                return NotFound("Book not found.");

            existingBook.title = updatedBook.title;
            existingBook.isbn = updatedBook.isbn;
            existingBook.publication_year = updatedBook.publication_year;
            existingBook.available = updatedBook.available;

            _context.SaveChanges();

            return Ok(existingBook);
        }

        // DELETE /books/1
        [HttpDelete("{id}")]
        public IActionResult DeleteBook(int id)
        {
            var book = _context.Books.Find(id);

            if (book == null)
                return NotFound("Book not found.");

            _context.Books.Remove(book);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
