using Jahacki_klub_Zeljeznicar.Models;

namespace Jahacki_klub_Zeljeznicar.ViewModels
{
    public class EditUserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public Kategorija Kategorija { get; set; }
        public Nivo? Nivo { get; set; }
        public string? NewPassword { get; set; } // Optional password update
    }
}
