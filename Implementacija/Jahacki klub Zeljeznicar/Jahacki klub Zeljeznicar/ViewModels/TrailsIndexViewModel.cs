using Jahacki_klub_Zeljeznicar.Models;

namespace Jahacki_klub_Zeljeznicar.ViewModels
{
    public class TrailsIndexViewModel
    {
        public List<Trail> RegisteredTrails { get; set; } = new List<Trail>();
        public List<Trail> AvailableTrails { get; set; } = new List<Trail>();
        public string LoggedInUserId { get; set; }

        public bool HasRegisteredTrails => RegisteredTrails != null && RegisteredTrails.Any();
        public bool HasAvailableTrails => AvailableTrails != null && AvailableTrails.Any();
    }
}
