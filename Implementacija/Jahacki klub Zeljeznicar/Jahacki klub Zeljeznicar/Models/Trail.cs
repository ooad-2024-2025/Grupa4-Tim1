using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Jahacki_klub_Zeljeznicar.Models
{
    public class Trail
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Naziv trail-a je obavezan")]
        [StringLength(200, ErrorMessage = "Naziv ne može biti duži od 200 karaktera")]
        [Display(Name = "Naziv")]
        public string Naziv { get; set; }

        [StringLength(1000, ErrorMessage = "Opis ne može biti duži od 1000 karaktera")]
        [Display(Name = "Opis")]
        public string Opis { get; set; }

        [Required(ErrorMessage = "Datum je obavezan")]
        [Display(Name = "Datum")]
        [DataType(DataType.Date)]
        public DateTime Datum { get; set; }

        // Navigation property
        [Required]
        [ForeignKey("User")]
        [StringLength(450)]
        public string RezervatorId { get; set; }
        public User Rezervator { get; set; }

        public ICollection<Trail_Konj> TrailKonji { get; set; }
    }
}
