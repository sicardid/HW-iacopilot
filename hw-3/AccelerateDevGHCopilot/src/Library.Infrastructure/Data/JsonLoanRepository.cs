using Library.ApplicationCore;
using Library.ApplicationCore.Entities;

namespace Library.Infrastructure.Data;

public class JsonLoanRepository : ILoanRepository
{
    private readonly JsonData _jsonData;

    public JsonLoanRepository(JsonData jsonData)
    {
        _jsonData = jsonData;
    }

    public async Task<Loan?> GetLoan(int id)
    {
        await _jsonData.EnsureDataLoaded();
        if (_jsonData.Loans == null || _jsonData.Loans.Count == 0)
            return null;

        foreach (Loan loan in _jsonData.Loans)
        {
            if (loan.Id == id)
            {
                Loan populated = _jsonData.GetPopulatedLoan(loan);
                return populated;
            }
        }
        return null;
    }

    public async Task UpdateLoan(Loan loan)
    {
        if (_jsonData.Loans == null || _jsonData.Loans.Count == 0)
            return;

        Loan? existingLoan = null;
        foreach (Loan l in _jsonData.Loans)
        {
            if (l.Id == loan.Id)
            {
                existingLoan = l;
                break;
            }
        }

        if (existingLoan != null)
        {
            existingLoan.BookItemId = loan.BookItemId;
            existingLoan.PatronId = loan.PatronId;
            existingLoan.LoanDate = loan.LoanDate;
            existingLoan.DueDate = loan.DueDate;
            existingLoan.ReturnDate = loan.ReturnDate;

            await _jsonData.SaveLoans(_jsonData.Loans);
            await _jsonData.LoadData();
        }
    }
}