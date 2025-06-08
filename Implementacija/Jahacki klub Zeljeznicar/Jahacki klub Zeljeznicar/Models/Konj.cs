using System.ComponentModel.DataAnnotations;

namespace Jahacki_klub_Zeljeznicar.Models
{
    public class Konj
    {
        public Konj()
        {
            TreningKonji = new List<Trening_Konj>();
            TrailKonji = new List<Trail_Konj>();
        }
        
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Ime konja je obavezno")]
        [StringLength(100, ErrorMessage = "Ime konja ne može biti duže od 100 karaktera")]
        public string Ime { get; set; }

        [StringLength(500, ErrorMessage = "Opis ne može biti duži od 500 karaktera")]
        [Display(Name = "Opis")]
        public string? Opis { get; set; }

        [Required(ErrorMessage = "Spol konja je obavezan")]
        [Display(Name = "Spol")]
        [EnumDataType(typeof(Spol))]
        public Spol Spol { get; set; }

        [StringLength(50, ErrorMessage = "Boja ne može biti duža od 50 karaktera")]
        [Display(Name = "Boja")]
        public string? Boja { get; set; }

        public ICollection<Trening_Konj> TreningKonji { get; set; }
        public ICollection<Trail_Konj> TrailKonji { get; set; }
    }
}