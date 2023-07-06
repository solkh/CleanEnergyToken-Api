using CleanEnergyToken_Api.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Security.AccessControl;
using System.Xml.Linq;

namespace CleanEnergyToken_Api.Entities
{
    public class NotificationMessage
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; }

        public int NotificationId { get; set; }

        public virtual Notification Notification { get; set; }
    }
    public class NotificationMessageDto
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Text { get; set; }

        public string Url { get; set; }

        public DateTime? ViewDate { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
