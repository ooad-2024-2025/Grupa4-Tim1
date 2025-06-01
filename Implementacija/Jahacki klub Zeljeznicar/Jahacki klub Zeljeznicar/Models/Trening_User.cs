using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Jahacki_klub_Zeljeznicar.Models
{
    public class Trening_User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Trening")]
        public int TreningId { get; set; }
        public Trening Trening { get; set; }

        [Required]
        [ForeignKey("User")]
        [StringLength(450)]
        public string UserId { get; set; }
        public User User { get; set; }
    }
}
