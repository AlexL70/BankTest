using BankTest.Data;
using BankTest.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BankTest.Services
{
    public class BankException : Exception
    {
        public BankException() : base() { }

        public BankException(string message) : base(message) { }

        public BankException(string message, Exception innerException) :
            base(message, innerException)
        { }
    }

    public class TransactionManager
    {
        private int _userId;
        private BankAccount _currAcc;
        private ApplicationDbContext _context;

        public TransactionManager(int userId, ApplicationDbContext context)
        {
            _userId = userId;
            _context = context;
        }

        private async Task AddTransaction(Transaction tr)
        {
            //  Update current account
            if (tr.type == Transaction.TransactionType.Withdraw
                || tr.type == Transaction.TransactionType.Transfer)
            {
                _currAcc.amount -= tr.amount;
            }
            else
            {
                _currAcc.amount += tr.amount;
            }
            //  Update correspondent amount
            if (tr.type == Transaction.TransactionType.Transfer)
            {
                var corrAcc = await _context.accounts.Where(a => _userId == tr.correspondentAccountId).SingleOrDefaultAsync();
                if (corrAcc == null)
                {
                    throw new BankException($"Erorr. Correspondent account not found (userId = {tr.correspondentAccountId}).");
                }
                corrAcc.amount += tr.amount;
            }
            //  Add transaction record
            tr.utcTime = DateTime.UtcNow;
            _context.transactions.Add(tr);
            //  Save to DB
            await _context.SaveChangesAsync();
        }

        public async Task Deposit(decimal sum)
        {
            CheckSign(sum);
            Transaction tr = new Transaction()
            {
                accountId = _userId,
                amount = sum,
                type = Transaction.TransactionType.Deposit,
                correspondentAccountId = _userId
            };
            await AddTransaction(tr);
        }

        public async Task Withdraw(decimal sum)
        {
            CheckSign(sum);
            await Reserve(sum);
            Transaction tr = new Transaction()
            {
                accountId = _userId,
                amount = sum,
                type = Transaction.TransactionType.Withdraw,
                correspondentAccountId = _userId
            };
            await AddTransaction(tr);
        }

        private async Task Reserve(decimal sum)
        {
            _currAcc = await _context.accounts.Include(a => a.user).Where(a => a.userId == _userId).SingleAsync();
            if (sum < _currAcc.amount)
            {
                throw new BankException($"Not enough money to withdraw ${sum}. User: {_currAcc.user.UserName}");
            }
        }

        private void CheckSign(decimal sum)
        {
            if (sum <= 0)
            {
                throw new BankException($"Bad sum: {sum}. Sum must be a positive value.");
            }
        }

        public async Task Transfer(decimal sum, int correspondent)
        {
            CheckSign(sum);
            await Reserve(sum);
            Transaction tr = new Transaction()
            {
                accountId = _userId,
                correspondentAccountId = correspondent,
                type = Transaction.TransactionType.Transfer,
                amount = sum
            };
            await AddTransaction(tr);
        }

        public async Task<IEnumerable<Transaction>> Statement()
        {
            var trs = await _context.transactions
                .Where(tr => tr.accountId == _userId || (
                    tr.type == Transaction.TransactionType.Withdraw &&
                    tr.correspondentAccountId == _userId
                )).Include(t => t.account).ToListAsync();
            return await Task.FromResult(trs);
        }

        public async Task<decimal> Balance()
        {
            return await _context.accounts.Where(a => a.userId == _userId)
                .Select(a => a.amount).SingleAsync();
        }
    }
}
