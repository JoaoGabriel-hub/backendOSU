using System.ComponentModel.DataAnnotations.Schema;

namespace BibliotecaApi.Models
{
    [Table("book_authors")]
    public class BookAuthor
    {
        [Column("book_id")]
        public int book_id { get; set; }

        [Column("author_id")]
        public int author_id { get; set; }
    }
}
