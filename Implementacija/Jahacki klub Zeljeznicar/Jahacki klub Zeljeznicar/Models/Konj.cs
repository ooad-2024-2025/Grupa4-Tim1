using System.ComponentModel.DataAnnotations;

namespace Jahacki_klub_Zeljeznicar.Models
{
    public class Konj
    {
        [Key]
        public int Id { get; set; }

        public string Ime { get; set; }
        public string Opis { get; set; }
        public Spol Spol { get; set; }
        public string Boja { get; set; }

        public ICollection<Trening_Konj> TreningKonji { get; set; }
        public ICollection<Trail_Konj> TrailKonji { get; set; }
    }
}
