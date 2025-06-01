using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Jahacki_klub_Zeljeznicar.Models
{
    public class Trail_Konj
    {
        [Key]
        public int Id { get; set; }

        // Navigation properties
        [Required]
        [ForeignKey("Trail")]
        public int TrailId { get; set; }
        public Trail Trail { get; set; }


        [Required]
        [ForeignKey("Konj")]
        public int KonjId { get; set; }
        public Konj Konj { get; set; }
    }
}
