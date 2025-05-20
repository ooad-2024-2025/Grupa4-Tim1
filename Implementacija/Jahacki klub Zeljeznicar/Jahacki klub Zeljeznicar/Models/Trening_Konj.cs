using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Jahacki_klub_Zeljeznicar.Models
{
    public class Trening_Konj
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Trening")]
        public int TreningId { get; set; }
        public Trening Trening { get; set; }

        [ForeignKey("Konj")]
        public int KonjId { get; set; }
        public Konj Konj { get; set; }
    }
}
