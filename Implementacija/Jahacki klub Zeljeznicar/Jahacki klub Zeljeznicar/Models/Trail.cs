using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Jahacki_klub_Zeljeznicar.Models
{
    public class Trail
    {
        [Key]
        public int Id { get; set; }

        public string Naziv { get; set; }
        public string Opis { get; set; }
        public DateTime Datum { get; set; }

        // Navigation property
        [ForeignKey("User")]
        public string RezervatorId { get; set; }

        public User Rezervator { get; set; }

        public ICollection<Trail_Konj> TrailKonji { get; set; }
    }
}
