using Jahacki_klub_Zeljeznicar.Models;

namespace Jahacki_klub_Zeljeznicar.ViewModels
{
    public class TrailDetailsViewModel
    {
        public Trail Trail { get; set; }
        public int MaxHorses { get; set; }
        public string LoggedInUserId { get; set; }
    }

}
