using Jahacki_klub_Zeljeznicar.Models;
using System.ComponentModel.DataAnnotations;

namespace Jahacki_klub_Zeljeznicar.ViewModels
{
    public class TrailDetailsViewModel
    {
        [Required(ErrorMessage = "Trail je obavezan.")]
        public Trail Trail { get; set; }

        [Required(ErrorMessage = "Maksimalan broj konja je obavezan.")]
        [Range(1, 20, ErrorMessage = "Maksimalan broj konja mora biti između 1 i 20.")]
        [Display(Name = "Maksimalan broj konja")]
        public int MaxHorses { get; set; }

        [Required(ErrorMessage = "ID prijavljenog korisnika je obavezan.")]
        public string LoggedInUserId { get; set; }
    }

}
