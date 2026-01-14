using BibliotecaApi.Data;
using BibliotecaApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;


namespace BibliotecaApi.Controllers
{
    [ApiController]
    [Route("loans")]
    public class LoansController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LoansController(AppDbContext context)
        {
            _context = context;
        }

        // POST /loans
        // Body: { "user_id": 1, "book_id": 10 }
        // The API should be secured, requiring a login before use (for applicable endpoints) with JWT tokens
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateLoan([FromBody] JsonElement body)
        {
            if (body.ValueKind != JsonValueKind.Object)
                return BadRequest("Invalid JSON format.");

            var input = JsonSerializer.Deserialize<Loan>(body);

            if (input == null || input.user_id <= 0 || input.book_id <= 0)
                return BadRequest("user_id and book_id are required and must be > 0.");

            // validate user
            var userExists = await _context.Users.AnyAsync(u => u.Id == input.user_id);
            if (!userExists) return NotFound("User not found.");

            // validate book
            var bookExists = await _context.Books.AnyAsync(b => b.id == input.book_id);
            if (!bookExists) return NotFound("Book not found.");

            // generate random dates
            var rng = Random.Shared;

            // loan_date between today and 60 days ago
            var loanDaysBack = rng.Next(0, 61); // 0..60
            var loanDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-loanDaysBack));

            // return_date between loan_date and today
            var maxReturnDays = (DateTime.Today - loanDate.ToDateTime(TimeOnly.MinValue)).Days;
            var returnDaysAfter = rng.Next(0, Math.Max(1, maxReturnDays + 1));
            var returnDate = loanDate.AddDays(returnDaysAfter);

            var loan = new Loan
            {
                user_id = input.user_id,
                book_id = input.book_id,
                loan_date = loanDate,
                return_date = returnDate
            };

            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            return Created("loans", loan);
        }

        // GET /loans?id=1
        // GET /loans?page=1
        // There is at least one endpoint with pagination.
        [HttpGet]
        public IActionResult GetLoans([FromQuery] int? id, [FromQuery] int page = 1)
        {
            const int pageSize = 5;

            if (id.HasValue)
            {
                var loan = _context.Loans.Find(id.Value);
                if (loan == null) return NotFound("Loan not found.");
                return Ok(loan);
            }

            if (page < 1)
                return BadRequest("Page number must be greater than 0.");

            var totalLoans = _context.Loans.Count();
            var loans = _context.Loans
                .OrderByDescending(l => l.id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            if (!loans.Any())
                return NotFound("No loans found.");

            return Ok(new
            {
                page,
                pageSize,
                totalLoans,
                totalPages = (totalLoans + pageSize - 1) / pageSize,
                loans
            });
        }

        // DELETE /loans/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLoan(int id)
        {
            var loan = await _context.Loans.FirstOrDefaultAsync(l => l.id == id);
            if (loan == null) return NotFound("Loan not found.");

            _context.Loans.Remove(loan);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
