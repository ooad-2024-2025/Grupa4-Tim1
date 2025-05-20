using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Jahacki_klub_Zeljeznicar.Models
{
    public class User : IdentityUser
    {
        [Required]
        public Kategorija Kategorija { get; set; }
        [Required]
        public string Ime { get; set; }
        [Required]
        public string Prezime { get; set; }
        public Nivo Nivo { get; set; }

        public ICollection<Clanarina> Clanarine { get; set; }
        public ICollection<Trening_User> TreningUsers { get; set; }
    }
}
