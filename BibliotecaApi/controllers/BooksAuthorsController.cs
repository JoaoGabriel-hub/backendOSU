using BibliotecaApi.Data;
using BibliotecaApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace BibliotecaApi.Controllers
{
    [ApiController]
    [Route("book-authors")]
    public class BookAuthorsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BookAuthorsController(AppDbContext context)
        {
            _context = context;
        }

        // POST /book-authors/link
        // Body: { "book_id": 1, "author_id": 2 }
        [HttpPost("link")]
        public async Task<IActionResult> Link([FromBody] BookAuthor body)
        {
            if (body.book_id <= 0 || body.author_id <= 0)
                return BadRequest("book_id and author_id must be > 0.");

            // verify if book exists
            var bookExists = await _context.Books.AnyAsync(b => b.id == body.book_id);
            if (!bookExists) return NotFound("Book not found.");

            // verify if author exists
            var authorExists = await _context.Authors.AnyAsync(a => a.id == body.author_id);
            if (!authorExists) return NotFound("Author not found.");

            // avoid duplicate (composite PK)
            var alreadyLinked = await _context.BookAuthors
                .AnyAsync(x => x.book_id == body.book_id && x.author_id == body.author_id);

            if (alreadyLinked)
                return Conflict("This author is already linked to this book.");

            _context.BookAuthors.Add(new BookAuthor
            {
                book_id = body.book_id,
                author_id = body.author_id
            });

            await _context.SaveChangesAsync();

            return Created("book-authors", body);
        }

        // POST /book-authors/link-batch
        // Body: [ { "book_id": 1, "author_id": 2 }, ... ]
        [HttpPost("link-batch")]
        public async Task<IActionResult> LinkBatch([FromBody] List<BookAuthor> body)
        {
            if (body == null || body.Count == 0)
                return BadRequest("List cannot be empty.");

            // validate ids > 0
            if (body.Any(x => x.book_id <= 0 || x.author_id <= 0))
                return BadRequest("All book_id and author_id must be > 0.");

            // remove duplicates within the request itself
            var distinct = body
                .GroupBy(x => new { x.book_id, x.author_id })
                .Select(g => g.First())
                .ToList();

            // (simple) try to insert only those that do not exist
            foreach (var item in distinct)
            {
                // check if book and author exist
                var bookExists = await _context.Books.AnyAsync(b => b.id == item.book_id);
                if (!bookExists) return NotFound($"Book not found: {item.book_id}");

                var authorExists = await _context.Authors.AnyAsync(a => a.id == item.author_id);
                if (!authorExists) return NotFound($"Author not found: {item.author_id}");

                var alreadyLinked = await _context.BookAuthors
                    .AnyAsync(x => x.book_id == item.book_id && x.author_id == item.author_id);

                if (!alreadyLinked)
                    _context.BookAuthors.Add(new BookAuthor { book_id = item.book_id, author_id = item.author_id });
            }

            await _context.SaveChangesAsync();
            return Created("book-authors", distinct);
        }

        // The API should be secured, requiring a login before use (for applicable endpoints) with JWT tokens
        [Authorize]
        // DELETE /book-authors/unlink?bookId=1&authorId=2
        [HttpDelete("unlink")]
        public async Task<IActionResult> Unlink([FromQuery] int bookId, [FromQuery] int authorId)
        {
            if (bookId <= 0 || authorId <= 0)
                return BadRequest("bookId and authorId must be > 0.");

            var link = await _context.BookAuthors
                .FirstOrDefaultAsync(x => x.book_id == bookId && x.author_id == authorId);

            if (link == null)
                return NotFound("Link not found.");

            _context.BookAuthors.Remove(link);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET /book-authors/by-book/1
        // list authors of the book (returns complete authors)
        [HttpGet("by-book/{bookId}")]
        public async Task<IActionResult> GetAuthorsByBook(int bookId)
        {
            if (bookId <= 0) return BadRequest("bookId must be > 0.");

            var bookExists = await _context.Books.AnyAsync(b => b.id == bookId);
            if (!bookExists) return NotFound("Book not found.");

            var authors = await _context.BookAuthors
                .Where(x => x.book_id == bookId)
                .Join(_context.Authors,
                      ba => ba.author_id,
                      a => a.id,
                      (ba, a) => a)
                .ToListAsync();

            return Ok(authors);
        }

        // (optional) GET /book-authors/by-author/2
        // list books of the author (returns complete books)
        [HttpGet("by-author/{authorId}")]
        public async Task<IActionResult> GetBooksByAuthor(int authorId)
        {
            if (authorId <= 0) return BadRequest("authorId must be > 0.");

            var authorExists = await _context.Authors.AnyAsync(a => a.id == authorId);
            if (!authorExists) return NotFound("Author not found.");

            var books = await _context.BookAuthors
                .Where(x => x.author_id == authorId)
                .Join(_context.Books,
                      ba => ba.book_id,
                      b => b.id,
                      (ba, b) => b)
                .ToListAsync();

            return Ok(books);
        }
    }
}
