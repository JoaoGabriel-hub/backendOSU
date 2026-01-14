using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BibliotecaApi.Models
{
    [Table("loans")]
    public class Loan
    {
        [Key]
        [Column("id")]
        public int id { get; set; }

        [Required]
        [Column("user_id")]
        public int user_id { get; set; }

        [Required]
        [Column("book_id")]
        public int book_id { get; set; }

        [Required]
        [Column("loan_date")]
        public DateOnly loan_date { get; set; }

        [Column("return_date")]
        public DateOnly? return_date { get; set; }
    }
}
