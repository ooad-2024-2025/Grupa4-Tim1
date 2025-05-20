using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Jahacki_klub_Zeljeznicar.Models
{
    public class Clanarina
    {
        [Key]
        public int Id { get; set; }

        public DateTime PocetakClanarine { get; set; }
        public DateTime IstekClanarine { get; set; }

        // Navigation property
        [ForeignKey("User")]
        public string UserId { get; set; }

        public User User { get; set; }
    }
}
