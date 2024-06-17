using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JWT.Models
{
    [Table("Refresh_Tokens")]
    public class RefreshToken
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("token")]
        public string Token { get; set; }

        [Column("expiry_date")]
        public DateTime ExpiryDate { get; set; }

        [ForeignKey("UserId")]
        [Column("FK_user_id")]
        public int UserId { get; set; }
        public User User { get; set; }
    }
}