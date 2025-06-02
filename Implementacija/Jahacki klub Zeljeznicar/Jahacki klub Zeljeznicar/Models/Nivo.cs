using System.ComponentModel.DataAnnotations;

namespace Jahacki_klub_Zeljeznicar.Models
{
    public enum Nivo
    {
        [Display(Name = "Početnik")]
        Pocetnik,

        [Display(Name = "Srednji")]
        Srednji,

        [Display(Name = "Napredni")]
        Napredni
    }
}
