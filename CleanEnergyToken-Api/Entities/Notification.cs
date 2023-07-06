using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Security.AccessControl;
using System.Xml.Linq;

namespace CleanEnergyToken_Api.Entities
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Text { get; set; }
        public string? Url { get; set; }
        public string? Image { get; set; }
        public DateTime CreatedDate { get; set; }

        [InverseProperty("Notification")]
        public virtual ICollection<NotificationMessage> NotificationMessages { get; set; }


        public Dictionary<string, string> Payload => new ()
        {
            ["Id"] = Id.ToString("D"),
            ["Title"] = Title,
            ["Text"] = Text,
            ["Url"] = Url,
            ["ImageUrl"] = Image,
            ["CreatedDate"] = CreatedDate.ToString("O")
        };
    }
}
