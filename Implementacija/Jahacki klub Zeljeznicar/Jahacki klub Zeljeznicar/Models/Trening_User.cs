using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Jahacki_klub_Zeljeznicar.Models
{
    public class Trening_User
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Trening")]
        public int TreningId { get; set; }
        public Trening Trening { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }
    }
}
