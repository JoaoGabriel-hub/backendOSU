using System.ComponentModel.DataAnnotations.Schema;

namespace BibliotecaApi.Models
{
    [Table("authors")]
    public class Author
    {
        public int id { get; set; }
        public string name { get; set; }
        public string? nationality { get; set; }
        public DateOnly? birth_date { get; set; }
    }
}
