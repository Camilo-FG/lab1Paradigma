using System.ComponentModel.DataAnnotations;

namespace LibraryService.Domain.Entities
{
    public class Library
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Location { get; set; }
    }
}
