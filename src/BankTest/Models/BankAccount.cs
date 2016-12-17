using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankTest.Models
{
    public class BankAccount
    {
        [Key]
        [ForeignKey(nameof(ApplicationUser))]
        public int userId { get; set; }
        public decimal amount { get; set; }
        public virtual ApplicationUser user { get; set; }
    }
}
