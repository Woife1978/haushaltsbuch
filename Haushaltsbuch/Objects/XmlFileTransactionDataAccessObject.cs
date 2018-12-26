using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using Haushaltsbuch.Interfaces;

namespace Haushaltsbuch.Objects
{
	public class XmlFileTransactionDataAccessObject : ITransactionDataAccessObject
	{
		private readonly string fileName;

		public XmlFileTransactionDataAccessObject(string fileName)
		{
			this.fileName = fileName;
			CreateXmlDocument();
		}


		/// <summary>
		/// Schreibt Eintrag in XML-Datei.
		/// </summary>
		/// <param name="transaction">Eintrag, der in XML-Datei geschrieben werden soll.</param>
		public void WriteTransaction(Transaction transaction)
		{
			if (transaction == null)
			{
				return;
			}

			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(fileName);

			XmlNode transactionsNode = xmlDocument.SelectSingleNode("transactions");

			XmlNode transactionNode = xmlDocument.CreateElement("transaction");

			transactionsNode?.AppendChild(transactionNode);

			XmlNode typeNode = xmlDocument.CreateElement("type");
			typeNode.InnerText = transaction.TransactionType == Helper.TransactionType.Outgoing
				? "outgoing"
				: "incoming";
			transactionNode.AppendChild(typeNode);

			XmlNode dayNode = xmlDocument.CreateElement("day");
			dayNode.InnerText = transaction.Day;
			transactionNode.AppendChild(dayNode);

			XmlNode monthNode = xmlDocument.CreateElement("month");
			monthNode.InnerText = transaction.Month;
			transactionNode.AppendChild(monthNode);

			XmlNode yearNode = xmlDocument.CreateElement("year");
			yearNode.InnerText = transaction.Year;
			transactionNode.AppendChild(yearNode);

			XmlNode descriptionNode = xmlDocument.CreateElement("description");
			descriptionNode.InnerText = transaction.Description;
			transactionNode.AppendChild(descriptionNode);

			XmlNode amountNode = xmlDocument.CreateElement("amount");
			amountNode.InnerText = transaction.Amount;
			transactionNode.AppendChild(amountNode);

			XmlNode categoryNode = xmlDocument.CreateElement("category");
			categoryNode.InnerText = transaction.Category;
			transactionNode.AppendChild(categoryNode);

			xmlDocument.Save(fileName);
		}



		/// <summary>
		/// Liest Kategorien von Ausgaben aus XML-Datei.
		/// </summary>
		/// <returns>Kategorien der Ausgaben.</returns>
		public string[] GetCategoriesFromOutgoingTransactions()
		{
			return ReadCategoriesFromOutgoingTransactions(fileName);
		}

		/// <summary>
		/// Liest Beschreibungen von Einnahmen aus XML-Datei.
		/// </summary>
		/// <returns>Beschreibungen der Einnahmen.</returns>
		public string[] GetDescriptionsFromIncomingTransactions()
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(fileName);

			const string xpath = "transactions/transaction[type='incoming']/description";
			XmlNodeList descriptionNodesList = xmlDocument.SelectNodes(xpath);

			string[] descriptionsList = (from XmlNode descriptionNode in descriptionNodesList
				where !string.IsNullOrEmpty(descriptionNode.InnerText)
				orderby descriptionNode.InnerText
				select descriptionNode.InnerText).Distinct().ToArray();

			return descriptionsList;
		}




		/// <summary>
		/// Entfernt Eintrag aus XML-Datei.
		/// </summary>
		/// <param name="transaction">Eintrag, der aus XML-Datei entfernt werden soll.</param>
		public void DeleteTransaction(Transaction transaction)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(fileName);

			XmlNode[] selectedTransactionNodes =
				(from XmlNode transactionNode in xmlDocument.SelectNodes("transactions/transaction")
				 let transactionType =
						string.Equals(transactionNode.SelectSingleNode("type")?.InnerText, "outgoing")
							? Helper.TransactionType.Outgoing
							: Helper.TransactionType.Incoming
				 where
						transactionType == transaction.TransactionType &&
						string.Equals(transactionNode.SelectSingleNode("day")?.InnerText, transaction.Day) &&
						string.Equals(transactionNode.SelectSingleNode("month")?.InnerText, transaction.Month) &&
						string.Equals(transactionNode.SelectSingleNode("year")?.InnerText, transaction.Year) &&
						string.Equals(
							transactionNode.SelectSingleNode("description")?.InnerText,
							transaction.Description) &&
						string.Equals(transactionNode.SelectSingleNode("amount")?.InnerText, transaction.Amount) &&
						string.Equals(transactionNode.SelectSingleNode("category")?.InnerText, transaction.Category)
				 select transactionNode).ToArray();

			if (selectedTransactionNodes.Length == 0)
			{
				return;
			}

			// Entfernt immer ersten gefundenen Eintrag.
			XmlNode parentNode = selectedTransactionNodes[0].ParentNode;

			parentNode?.RemoveChild(selectedTransactionNodes[0]);

			xmlDocument.Save(fileName);
		}


		/// <summary>
		/// Liest Einträge aus XML-Datei.
		/// </summary>
		/// <param name="filter">Filter, der auf Einträge angewendet wird.</param>
		/// <returns>Einträge aus XML-Datei.</returns>
		public Transaction[] GetTransactions(Filter filter)
		{
			IEnumerable<XmlNode> filteredTransactions = FilterTransactions(filter);

			Transaction[] selectedTransactions = (from XmlNode transactionNode in filteredTransactions
												  let typeNode = transactionNode.SelectSingleNode("type")
												  let dayNode = transactionNode.SelectSingleNode("day")
												  let monthNode = transactionNode.SelectSingleNode("month")
												  let yearNode = transactionNode.SelectSingleNode("year")
												  let descriptionNode = transactionNode.SelectSingleNode("description")
												  let amountNode = transactionNode.SelectSingleNode("amount")
												  let categoryNode = transactionNode.SelectSingleNode("category")
												  select
					new Transaction(
						new DateTime(
							Convert.ToInt32(yearNode.InnerText, CultureInfo.InvariantCulture),
							Convert.ToInt32(monthNode.InnerText, CultureInfo.InvariantCulture),
							Convert.ToInt32(dayNode.InnerText, CultureInfo.InvariantCulture)))
					{
						Amount = amountNode.InnerText,
						Category = categoryNode.InnerText,
						Description = descriptionNode.InnerText,
						TransactionType =
							string.Equals(typeNode.InnerText, "outgoing")
								? Helper.TransactionType.Outgoing
								: Helper.TransactionType.Incoming
					}).OrderBy(t => t.Date).ToArray();

			return selectedTransactions;
		}



		/// <summary>
		/// Filtert Einträge in XML-Datei.
		/// </summary>
		/// <param name="filter">Filter, der auf Einträge angewendet wird.</param>
		/// <returns>Gefilterte Einträge.</returns>
		private IEnumerable<XmlNode> FilterTransactions(Filter filter)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(fileName);

			XmlNode[] filteredTransactions =
				(from XmlNode transactionNode in xmlDocument.SelectNodes("transactions/transaction")
				 let yearNode = transactionNode.SelectSingleNode("year")
				 where string.Equals(yearNode.InnerText, filter.Year)
				 select transactionNode).ToArray();

			if (!string.Equals(filter.Month, "00"))
			{
				filteredTransactions = ApplyMonth(filter, filteredTransactions);
			}

			if (!string.IsNullOrEmpty(filter.Category))
			{
				filteredTransactions = ApplyCategory(filter, filteredTransactions);
			}

			if (!string.IsNullOrEmpty(filter.SearchTerm))
			{
				filteredTransactions = ApplySearchTerm(filter, filteredTransactions);
			}

			return filteredTransactions;
		}


		/// <summary>
		/// Wendet Monat als Filter an.
		/// </summary>
		/// <param name="filter">Filter, der auf Einträge angewendet wird.</param>
		/// <param name="filteredTransactions">Bereits gefilterte Einträge.</param>
		/// <returns>Gefiltertete Einträge.</returns>
		private static XmlNode[] ApplyMonth(Filter filter, XmlNode[] filteredTransactions)
		{
			filteredTransactions = (from XmlNode transactionNode in filteredTransactions
									let monthNode = transactionNode.SelectSingleNode("month")
									where string.Equals(monthNode.InnerText, filter.Month)
									select transactionNode).ToArray();
			return filteredTransactions;
		}

		/// <summary>
		/// Wendet Kategorie als Filter an.
		/// </summary>
		/// <param name="filter">Filter, der auf Einträge angewendet wird.</param>
		/// <param name="filteredTransactions">Bereits gefilterte Einträge.</param>
		/// <returns>Gefilterte Einträge.</returns>
		private static XmlNode[] ApplyCategory(Filter filter, XmlNode[] filteredTransactions)
		{
			filteredTransactions = (from XmlNode transactionNode in filteredTransactions
									let categoryNode = transactionNode.SelectSingleNode("category")
									where string.Equals(categoryNode.InnerText, filter.Category)
									select transactionNode).ToArray();
			return filteredTransactions;
		}

		/// <summary>
		/// Wendet Suchbegriff als Filter an.
		/// </summary>
		/// <param name="filter">Filter, der auf Einträge angewendet wird.</param>
		/// <param name="filteredTransactions">Bereits gefilterte Einträge.</param>
		/// <returns>Gefilterte Einträge.</returns>
		private static XmlNode[] ApplySearchTerm(Filter filter, XmlNode[] filteredTransactions)
		{
			filteredTransactions = (from XmlNode transactionNode in filteredTransactions
									let descriptionNode = transactionNode.SelectSingleNode("description")
									where
										descriptionNode.InnerText.ToLower(CultureInfo.CurrentCulture)
											.Contains(filter.SearchTerm.ToLower(CultureInfo.CurrentCulture))
									select transactionNode).ToArray();
			return filteredTransactions;
		}


		/// <summary>
		/// Liest Kategorien aller Einträge aus XML-Datei.
		/// </summary>
		/// <returns>Kategorien aller Einträge.</returns>
		public string[] GetCategoriesFromAllTransactions()
		{
			string[] emptyList =
			{
				string.Empty
			};

			string[] categoryList =
				(from string category in ReadCategoriesFromOutgoingTransactions(fileName)
						.Concat(ReadCategoriesFromIncomingTransactions(fileName).Concat(emptyList))
				 orderby category
				 select category).Distinct().ToArray();

			return categoryList;
		}


		/// <summary>
		/// Liest Kategorien von Einnahmen aus XML-Datei.
		/// </summary>
		/// <returns>Kategorien der Einnahmen.</returns>
		public string[] GetCategoriesFromIncomingTransactions()
		{
			return ReadCategoriesFromIncomingTransactions(fileName);
		}


		/// <summary>
		/// Liest Kategorien von Einnahmen aus XML-Datei.
		/// </summary>
		/// <param name="fileName">Dateiname der XML-Datei.</param>
		/// <returns>Kategorien der Einnahmen.</returns>
		private static string[] ReadCategoriesFromIncomingTransactions(string fileName)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(fileName);

			const string xpath = "transactions/transaction[type='incoming']/category";
			XmlNodeList categoryNodesList = xmlDocument.SelectNodes(xpath);

			string[] categoriesList = (from XmlNode categoryNode in categoryNodesList
									   where !String.IsNullOrEmpty(categoryNode.InnerText)
									   orderby categoryNode.InnerText
									   select categoryNode.InnerText).Distinct().ToArray();

			return categoriesList;
		}


		/// <summary>
		/// Liest Kategorien von Ausgaben aus XML-Datei.
		/// </summary>
		/// <param name="fileName">Dateiname der XML-Datei.</param>
		/// <returns>Kategorien der Ausgaben.</returns>
		public static string[] ReadCategoriesFromOutgoingTransactions(string fileName)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(fileName);

			const string xpath = "transactions/transaction[type='outgoing']/category";
			XmlNodeList categoryNodesList = xmlDocument.SelectNodes(xpath);

			string[] categoriesList = (from XmlNode categoryNode in categoryNodesList
									   where !String.IsNullOrEmpty(categoryNode.InnerText)
									   orderby categoryNode.InnerText
									   select categoryNode.InnerText).Distinct().ToArray();

			return categoriesList;
		}


		/// <summary>
		/// Liest Beschreibungen von Ausgaben aus XML-Datei.
		/// </summary>
		/// <returns>Beschreibungen der Ausgaben.</returns>
		public string[] GetDescriptionsFromOutgoingTransactions()
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(fileName);

			const string xpath = "transactions/transaction[type='outgoing']/description";
			XmlNodeList descriptionNodesList = xmlDocument.SelectNodes(xpath);

			string[] descriptionsList = (from XmlNode descriptionNode in descriptionNodesList
										 where !string.IsNullOrEmpty(descriptionNode.InnerText)
										 orderby descriptionNode.InnerText
										 select descriptionNode.InnerText).Distinct().ToArray();

			return descriptionsList;
		}

		/// <summary>
		/// Erstellt XML-Datei.
		/// </summary>
		public void CreateXmlDocument()
		{
			string directory = Path.GetDirectoryName(fileName);

			if (!string.IsNullOrEmpty(directory))
			{
				Directory.CreateDirectory(directory);
			}

			XmlDocument xmlDocument = new XmlDocument();

			XmlNode declarationNode = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
			xmlDocument.AppendChild(declarationNode);

			XmlNode transactionsNode = xmlDocument.CreateElement("transactions");
			xmlDocument.AppendChild(transactionsNode);

			xmlDocument.Save(fileName);
		}

	}
}