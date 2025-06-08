using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jahacki_klub_Zeljeznicar.Models
{
    public class Trening
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Naziv treninga je obavezan")]
        [StringLength(200, ErrorMessage = "Naziv ne može biti duži od 200 karaktera")]
        [Display(Name = "Naziv treninga")]
        public string Naziv { get; set; }

        [Required(ErrorMessage = "Nivo je obavezan")]
        [Display(Name = "Nivo")]
        [EnumDataType(typeof(Nivo))]
        public Nivo Nivo { get; set; }

        [Required(ErrorMessage = "Datum je obavezan")]
        [Display(Name = "Datum")]
        [DataType(DataType.Date)]
        public DateTime Datum { get; set; }

        [Required(ErrorMessage = "Maksimalni broj članova je obavezan")]
        [Range(1, 50, ErrorMessage = "Maksimalni broj članova mora biti između 1 i 50")]
        [Display(Name = "Maksimalni broj članova")]
        public int MaxBrClanova { get; set; }

        [Required]
        [ForeignKey("User")]
        [StringLength(450)]
        public string TrenerId { get; set; }
        public User Trener { get; set; }

        public ICollection<Trening_Konj> TreningKonji { get; set; } = new List<Trening_Konj>();
        public ICollection<Trening_User> TreningUsers { get; set; } = new List<Trening_User>();
    }
}
