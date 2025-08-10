using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Library.ApplicationCore.Entities;
using Library.Infrastructure.Data;

public class JsonLoanRepositoryTest
{
	private List<Loan> GetSampleLoans()
	{
		return new List<Loan>
		{
			new Loan { Id = 1, BookItemId = 10, PatronId = 100, LoanDate = DateTime.Today, DueDate = DateTime.Today.AddDays(14), ReturnDate = null },
			new Loan { Id = 2, BookItemId = 11, PatronId = 101, LoanDate = DateTime.Today, DueDate = DateTime.Today.AddDays(7), ReturnDate = null }
		};
	}

	[Fact]
	public async Task GetLoan_ReturnsCorrectLoan_WhenIdExists()
	{
		var loans = GetSampleLoans();
		var mockJsonData = new Mock<JsonData>();
		mockJsonData.Setup(j => j.EnsureDataLoaded()).Returns(Task.CompletedTask).Verifiable();
		mockJsonData.SetupGet(j => j.Loans).Returns(loans);
		mockJsonData.Setup(j => j.GetPopulatedLoan(It.IsAny<Loan>())).Returns<Loan>(l => l);

		var repo = new JsonLoanRepository(mockJsonData.Object);

		var loan = await repo.GetLoan(1);

		Assert.NotNull(loan);
		Assert.Equal(1, loan!.Id);
		mockJsonData.Verify(j => j.EnsureDataLoaded(), Times.Once);
		mockJsonData.Verify(j => j.GetPopulatedLoan(It.Is<Loan>(l => l.Id == 1)), Times.Once);
	}

	[Fact]
	public async Task GetLoan_ReturnsNull_WhenIdDoesNotExist()
	{
		var loans = GetSampleLoans();
		var mockJsonData = new Mock<JsonData>();
		mockJsonData.Setup(j => j.EnsureDataLoaded()).Returns(Task.CompletedTask).Verifiable();
		mockJsonData.SetupGet(j => j.Loans).Returns(loans);

		var repo = new JsonLoanRepository(mockJsonData.Object);

		var loan = await repo.GetLoan(999);

		Assert.Null(loan);
		mockJsonData.Verify(j => j.EnsureDataLoaded(), Times.Once);
	}

	[Fact]
	public async Task GetLoan_ReturnsNull_WhenLoansListIsNull()
	{
		var mockJsonData = new Mock<JsonData>();
		mockJsonData.Setup(j => j.EnsureDataLoaded()).Returns(Task.CompletedTask);
		mockJsonData.SetupGet(j => j.Loans).Returns((List<Loan>?)null);

		var repo = new JsonLoanRepository(mockJsonData.Object);

		var loan = await repo.GetLoan(1);

		Assert.Null(loan);
	}

	[Fact]
	public async Task UpdateLoan_UpdatesExistingLoan_AndPersists()
	{
		var loans = GetSampleLoans();
		var mockJsonData = new Mock<JsonData>();
		mockJsonData.SetupGet(j => j.Loans).Returns(loans);
		mockJsonData.Setup(j => j.SaveLoans(It.IsAny<List<Loan>>())).Returns(Task.CompletedTask).Verifiable();
		mockJsonData.Setup(j => j.LoadData()).Returns(Task.CompletedTask).Verifiable();

		var repo = new JsonLoanRepository(mockJsonData.Object);

		var updatedLoan = new Loan
		{
			Id = 1,
			BookItemId = 20,
			PatronId = 200,
			LoanDate = DateTime.Today.AddDays(-1),
			DueDate = DateTime.Today.AddDays(10),
			ReturnDate = DateTime.Today
		};

		await repo.UpdateLoan(updatedLoan);

		var loan = loans.Find(l => l.Id == 1);
		Assert.NotNull(loan);
		Assert.Equal(20, loan!.BookItemId);
		Assert.Equal(200, loan.PatronId);
		Assert.Equal(DateTime.Today.AddDays(-1), loan.LoanDate);
		Assert.Equal(DateTime.Today.AddDays(10), loan.DueDate);
		Assert.Equal(DateTime.Today, loan.ReturnDate);

		mockJsonData.Verify(j => j.SaveLoans(It.IsAny<List<Loan>>()), Times.Once);
		mockJsonData.Verify(j => j.LoadData(), Times.Once);
	}

	[Fact]
	public async Task UpdateLoan_DoesNothing_WhenLoanDoesNotExist()
	{
		var loans = GetSampleLoans();
		var mockJsonData = new Mock<JsonData>();
		mockJsonData.SetupGet(j => j.Loans).Returns(loans);

		var repo = new JsonLoanRepository(mockJsonData.Object);

		var nonExistentLoan = new Loan { Id = 999, BookItemId = 1, PatronId = 1, LoanDate = DateTime.Now, DueDate = DateTime.Now, ReturnDate = null };

		await repo.UpdateLoan(nonExistentLoan);

		mockJsonData.Verify(j => j.SaveLoans(It.IsAny<List<Loan>>()), Times.Never);
		mockJsonData.Verify(j => j.LoadData(), Times.Never);
	}

	[Fact]
	public async Task UpdateLoan_DoesNothing_WhenLoansListIsNull()
	{
		var mockJsonData = new Mock<JsonData>();
		mockJsonData.SetupGet(j => j.Loans).Returns((List<Loan>?)null);

		var repo = new JsonLoanRepository(mockJsonData.Object);

		var updatedLoan = new Loan { Id = 1, BookItemId = 1, PatronId = 1, LoanDate = DateTime.Now, DueDate = DateTime.Now, ReturnDate = null };

		await repo.UpdateLoan(updatedLoan);

		mockJsonData.Verify(j => j.SaveLoans(It.IsAny<List<Loan>>()), Times.Never);
		mockJsonData.Verify(j => j.LoadData(), Times.Never);
	}

	[Fact]
	public async Task UpdateLoan_UpdatesOnlySpecifiedLoan()
	{
		var loans = GetSampleLoans();
		var mockJsonData = new Mock<JsonData>();
		mockJsonData.SetupGet(j => j.Loans).Returns(loans);
		mockJsonData.Setup(j => j.SaveLoans(It.IsAny<List<Loan>>())).Returns(Task.CompletedTask);
		mockJsonData.Setup(j => j.LoadData()).Returns(Task.CompletedTask);

		var repo = new JsonLoanRepository(mockJsonData.Object);

		var updatedLoan = new Loan
		{
			Id = 2,
			BookItemId = 99,
			PatronId = 199,
			LoanDate = DateTime.Today.AddDays(-2),
			DueDate = DateTime.Today.AddDays(5),
			ReturnDate = DateTime.Today.AddDays(1)
		};

		await repo.UpdateLoan(updatedLoan);

		var loan1 = loans.Find(l => l.Id == 1);
		var loan2 = loans.Find(l => l.Id == 2);

		Assert.NotNull(loan1);
		Assert.Equal(10, loan1!.BookItemId); // unchanged
		Assert.NotNull(loan2);
		Assert.Equal(99, loan2!.BookItemId); // updated
		Assert.Equal(199, loan2.PatronId);
		Assert.Equal(DateTime.Today.AddDays(-2), loan2.LoanDate);
		Assert.Equal(DateTime.Today.AddDays(5), loan2.DueDate);
		Assert.Equal(DateTime.Today.AddDays(1), loan2.ReturnDate);
	}

	[Fact]
	public async Task GetLoan_CallsGetPopulatedLoan_OnlyWhenFound()
	{
		var loans = GetSampleLoans();
		var mockJsonData = new Mock<JsonData>();
		mockJsonData.Setup(j => j.EnsureDataLoaded()).Returns(Task.CompletedTask);
		mockJsonData.SetupGet(j => j.Loans).Returns(loans);
		mockJsonData.Setup(j => j.GetPopulatedLoan(It.IsAny<Loan>())).Returns<Loan>(l => l);

		var repo = new JsonLoanRepository(mockJsonData.Object);

		var loan = await repo.GetLoan(2);

		Assert.NotNull(loan);
		Assert.Equal(2, loan!.Id);
		mockJsonData.Verify(j => j.GetPopulatedLoan(It.Is<Loan>(l => l.Id == 2)), Times.Once);
		mockJsonData.Verify(j => j.GetPopulatedLoan(It.Is<Loan>(l => l.Id == 1)), Times.Never);
	}
}
