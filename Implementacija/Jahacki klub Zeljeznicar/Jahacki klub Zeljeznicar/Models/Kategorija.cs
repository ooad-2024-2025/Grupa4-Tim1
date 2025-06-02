using System.ComponentModel.DataAnnotations;

namespace Jahacki_klub_Zeljeznicar.Models
{
    public enum Kategorija
    {
        [Display(Name = "Guest")]
        Guest,

        [Display(Name = "Član")]
        Clan,

        [Display(Name = "Trener")]
        Trener,

        [Display(Name = "Admin")]
        Admin
    }
}
