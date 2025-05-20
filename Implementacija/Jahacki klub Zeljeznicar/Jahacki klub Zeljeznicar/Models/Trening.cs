using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jahacki_klub_Zeljeznicar.Models
{
    public class Trening
    {
        [Key]
        public int Id { get; set; }

        public string Naziv { get; set; }
        public Nivo Nivo { get; set; }
        public DateTime Datum { get; set; }
        public int MaxBrClanova { get; set; }

        [ForeignKey("User")]
        public string TrenerId { get; set; }
        public User Trener { get; set; }

        public ICollection<Trening_Konj> TreningKonji { get; set; }
        public ICollection<Trening_User> TreningUsers { get; set; }
    }
}
