using Jahacki_klub_Zeljeznicar.Models;

namespace Jahacki_klub_Zeljeznicar.ViewModels
{
    public class CreateUserViewModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public Kategorija Kategorija { get; set; }
        public Nivo? Nivo { get; set; }
    }
}
