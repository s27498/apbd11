using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JWT.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        [Column("id")]
        public int UserId { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("password_hash")]
        public string PasswordHash { get; set; }

        [Column("salt")]
        public string Salt { get; set; }
    }
}