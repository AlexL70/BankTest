using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankTest.Models
{
    public class Transaction
    {
        public enum TransactionType
        {
            Deposit,
            Withdraw,
            Transfer
        }

        [Key]
        public int transactionId { get; set; }
        [ForeignKey(nameof(BankAccount))]
        public int accountId { get; set; }
        [ForeignKey(nameof(BankAccount))]
        public int correspondentAccountId { get; set; }
        public TransactionType type { get; set; }
        public decimal amount { get; set; }
        public DateTime utcTime { get; set; }

        public virtual BankAccount account { get; set; }
        public virtual BankAccount correspondentAccount { get; set; }
    }
}
