using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BibliotecaApi.Models
{
    [Table("books")]
    public class Book
    {
        [Key]
        [Column("id")]
        public int id { get; set; }

        [Required]
        [Column("title")]
        [MaxLength(200)]
        public string title { get; set; } = string.Empty;

        [Column("isbn")]
        [MaxLength(20)]
        public string? isbn { get; set; }

        [Column("publication_year")]
        public int? publication_year { get; set; }

        [Column("available")]
        public bool available { get; set; } = true;
    }
}
