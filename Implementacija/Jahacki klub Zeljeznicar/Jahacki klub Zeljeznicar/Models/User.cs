using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Jahacki_klub_Zeljeznicar.Models
{
    public class User : IdentityUser
    {
        [Required(ErrorMessage = "Kategorija je obavezna")]
        [Display(Name = "Kategorija")]
        [EnumDataType(typeof(Kategorija))]
        public Kategorija Kategorija { get; set; }

        [Required(ErrorMessage = "Ime je obavezno")]
        [StringLength(50, ErrorMessage = "Ime ne može biti duže od 50 karaktera")]
        [Display(Name = "Ime")]
        public string Ime { get; set; }


        [Required(ErrorMessage = "Prezime je obavezno")]
        [StringLength(50, ErrorMessage = "Prezime ne može biti duže od 50 karaktera")]
        [Display(Name = "Prezime")]
        public string Prezime { get; set; }

        [Display(Name = "Nivo")]
        [EnumDataType(typeof(Nivo))]
        public Nivo? Nivo { get; set; }

        public ICollection<Clanarina> Clanarine { get; set; }
        public ICollection<Trening_User> TreningUsers { get; set; }
    }
}
