using Jahacki_klub_Zeljeznicar.Models;
using System.ComponentModel.DataAnnotations;

namespace Jahacki_klub_Zeljeznicar.ViewModels
{
    public class DashboardViewModel
    {
        [Required(ErrorMessage = "Korisnik je obavezan.")]
        public User CurrentUser { get; set; }

        [Required(ErrorMessage = "Kategorija korisnika je obavezna.")]
        [EnumDataType(typeof(Kategorija), ErrorMessage = "Molimo izaberite validnu kategoriju.")]
        public Kategorija UserCategory { get; set; }

        [EnumDataType(typeof(Nivo), ErrorMessage = "Molimo izaberite validan nivo.")]
        public Nivo? CurrentUserLevel { get; set; }

        // For Clan users - membership info  
        public Clanarina CurrentUserClanarina { get; set; }

        // For Guest users
        public List<Trail> UserTrails { get; set; } = new List<Trail>();

        // For Clan users
        public List<Trening> AvailableTrainings { get; set; } = new List<Trening>();
        public List<Trening> RegisteredTrainings { get; set; } = new List<Trening>();
        public List<int> RecommendedTrainingIds { get; set; } = new List<int>();

        // For Trener and Admin users
        public List<Trening> AllTrainings { get; set; } = new List<Trening>();
        public List<User> ClanMembers { get; set; } = new List<User>();

        // For Admin users only
        public List<Trail> AllTrails { get; set; } = new List<Trail>();
        public List<User> AllTrainers { get; set; } = new List<User>();
        public List<User> AllUsers { get; set; } = new List<User>();
        public List<Konj> AllHorses { get; set; } = new List<Konj>();
    }
}