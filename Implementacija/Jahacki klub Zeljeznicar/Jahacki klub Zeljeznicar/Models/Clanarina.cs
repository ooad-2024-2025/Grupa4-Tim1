using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Jahacki_klub_Zeljeznicar.Models
{
    public class Clanarina
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Početak članarine je obavezan")]
        [Display(Name = "Početak članarine")]
        [DataType(DataType.Date)]
        public DateTime PocetakClanarine { get; set; }

        [Required(ErrorMessage = "Istek članarine je obavezan")]
        [Display(Name = "Istek članarine")]
        [DataType(DataType.Date)]
        public DateTime IstekClanarine { get; set; }

        [Required]
        [ForeignKey("User")]
        [StringLength(450)]
        public string UserId { get; set; }

        public User User { get; set; }
    }
}
