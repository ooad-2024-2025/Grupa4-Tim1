using Jahacki_klub_Zeljeznicar.Models;
using System.ComponentModel.DataAnnotations;

namespace Jahacki_klub_Zeljeznicar.ViewModels
{
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Email je obavezan.")]
        [EmailAddress(ErrorMessage = "Neispravna email adresa.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Lozinka je obavezna.")]
        [StringLength(100, ErrorMessage = "Lozinka mora imati najmanje {2} karaktera.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Lozinka")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Ime je obavezno.")]
        [StringLength(50, ErrorMessage = "Ime ne može biti duže od 50 karaktera.")]
        [Display(Name = "Ime")]
        public string Ime { get; set; }

        [Required(ErrorMessage = "Prezime je obavezno.")]
        [StringLength(50, ErrorMessage = "Prezime ne može biti duže od 50 karaktera.")]
        [Display(Name = "Prezime")]
        public string Prezime { get; set; }

        [Required(ErrorMessage = "Kategorija je obavezna.")]
        [Display(Name = "Kategorija")]
        [EnumDataType(typeof(Kategorija), ErrorMessage = "Molimo izaberite validnu kategoriju.")]
        public Kategorija Kategorija { get; set; }

        [Display(Name = "Nivo")]
        [EnumDataType(typeof(Nivo), ErrorMessage = "Molimo izaberite validan nivo.")]
        public Nivo? Nivo { get; set; }
    }
}
