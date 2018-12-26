using Haushaltsbuch.Objects;

namespace Haushaltsbuch.Interfaces
{
	public interface ITransactionDataAccessObject
	{
		void DeleteTransaction(Transaction transaction);
		Transaction[] GetTransactions(Filter filter);
		string[] GetCategoriesFromAllTransactions();
		void WriteTransaction(Transaction transaction);
		string[] GetDescriptionsFromOutgoingTransactions();
		string[] GetCategoriesFromOutgoingTransactions();
		string[] GetDescriptionsFromIncomingTransactions();
		string[] GetCategoriesFromIncomingTransactions();
	}
}