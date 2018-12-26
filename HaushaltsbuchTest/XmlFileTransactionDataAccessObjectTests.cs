using System;
using System.Collections.Specialized;
using System.IO;
using System.Xml;
using Haushaltsbuch;
using Haushaltsbuch.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HaushaltsbuchTest
{
	[TestClass]
	[DeploymentItem("TestDocuments\\ComparisonXmlDocument.xml", "XmlFileEditorTest")]
	[DeploymentItem("TestDocuments\\EmptyXmlDocument.xml", "XmlFileEditorTest")]
	[DeploymentItem("TestDocuments\\SampleXmlDocumentForDeletion.xml", "XmlFileEditorTest")]
	[DeploymentItem("TestDocuments\\SampleXmlDocument.xml", "XmlFileReaderTest")]
	public class XmlFileTransactionDataAccessObjectTests
	{
		#region Felder

		/// <summary>
		/// Speicherort von Testdatei zum Vergleichen.
		/// </summary>
		private const string ComparisonXmlDocument = "XmlFileEditorTest\\ComparisonXmlDocument.xml";



		/// <summary>
		/// Speicherort von Testdatei mit Beispieldaten zum Löschen.
		/// </summary>
		private const string SampleXmlDocumentForDeletion = "XmlFileEditorTest\\SampleXmlDocumentForDeletion.xml";

		/// <summary>
		/// Testdatei zum Vergleichen.
		/// </summary>
		private XmlDocument comparisonXmlDocument;

		/// <summary>
		/// Leere Testdatei.
		/// </summary>
		private XmlDocument emptyXmlDocument;

		/// <summary>
		/// Testdatei mit Beispieldaten zum Löschen.
		/// </summary>
		private XmlDocument sampleXmlDocumentForDeletion;

		#endregion



		/// <summary>
		/// Testet Schreiben von Eintrag in XML-Datei.
		/// </summary>
		[TestMethod]
		public void TestWriteTransaction()
		{
			// Arrange
			DateTime dateTime = new DateTime(2013, 11, 23);

			Transaction transaction1 = new Transaction(dateTime)
			{
				TransactionType = Helper.TransactionType.Outgoing,
				Amount = "-100,00",
				Category = "outgoingCategory",
				Description = "outgoingDescription"
			};

			Transaction transaction2 = new Transaction(dateTime)
			{
				TransactionType = Helper.TransactionType.Outgoing,
				Amount = "-100,00",
				Category = string.Empty,
				Description = string.Empty
			};

			string expected = "expected";
			string actual = "actual";

			var transactionDataAccessObject = new XmlFileTransactionDataAccessObject(EmptyXmlDocument);


			// Act
			transactionDataAccessObject.WriteTransaction(transaction1);
			transactionDataAccessObject.WriteTransaction(transaction2);

			comparisonXmlDocument.Load(ComparisonXmlDocument);
			emptyXmlDocument.Load(EmptyXmlDocument);

			XmlNode comparisonXmlDocumentTransactionsNode = comparisonXmlDocument.SelectSingleNode("transactions");
			if (comparisonXmlDocumentTransactionsNode != null)
			{
				expected = comparisonXmlDocumentTransactionsNode.InnerText;
			}

			XmlNode emptyXmlDocumentTransactionsNode = emptyXmlDocument.SelectSingleNode("transactions");
			if (emptyXmlDocumentTransactionsNode != null)
			{
				actual = emptyXmlDocumentTransactionsNode.InnerText;
			}

			// Assert
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		/// Testet Schreiben von leerem Eintrag in XML-Datei.
		/// </summary>
		[TestMethod]
		public void TestWriteEmptyTransaction()
		{
			// Arrange
			string expected = "expected";
			string actual = "actual";

			emptyXmlDocument.Load(EmptyXmlDocument);

			XmlNode emptyXmlDocumentTransactionsNode = emptyXmlDocument.SelectSingleNode("transactions");
			if (emptyXmlDocumentTransactionsNode != null)
			{
				expected = emptyXmlDocumentTransactionsNode.InnerText;
			}

			// Act
			var transactionDataAccessObject = new XmlFileTransactionDataAccessObject(EmptyXmlDocument);
			transactionDataAccessObject.WriteTransaction(null);

			emptyXmlDocumentTransactionsNode = emptyXmlDocument.SelectSingleNode("transactions");
			if (emptyXmlDocumentTransactionsNode != null)
			{
				actual = emptyXmlDocumentTransactionsNode.InnerText;
			}

			// Assert
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		/// Testet Entfernen von Eintrag aus XML-Datei.
		/// </summary>
		[TestMethod]
		public void TestDeleteTransaction()
		{
			// Arrange
			DateTime dateTime = new DateTime(2013, 11, 23);

			Transaction transaction = new Transaction(dateTime)
			{
				TransactionType = Helper.TransactionType.Outgoing,
				Amount = "-100,00",
				Category = "outgoingCategory",
				Description = "outgoingDescription"
			};

			string expected = string.Empty;
			string actual = "actual";

			// Act
			var transactionDataAccessObject = new XmlFileTransactionDataAccessObject(SampleXmlDocumentForDeletion);

			transactionDataAccessObject.DeleteTransaction(transaction);
			transactionDataAccessObject.DeleteTransaction(transaction);

			sampleXmlDocumentForDeletion.Load(SampleXmlDocumentForDeletion);

			XmlNode transactionsNode = sampleXmlDocumentForDeletion.SelectSingleNode("transactions");
			if (transactionsNode != null)
			{
				actual = transactionsNode.InnerText;
			}

			// Assert
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		/// Testet Entfernen von nicht vorhandenem Eintrag aus XML-Datei.
		/// </summary>
		[TestMethod]
		public void TestDeleteNotExistingTransaction()
		{
			// Arrange
			string expected = "expected";
			string actual = "actual";

			sampleXmlDocumentForDeletion.Load(SampleXmlDocumentForDeletion);

			XmlNode transactionsNode = sampleXmlDocumentForDeletion.SelectSingleNode("transactions");
			if (transactionsNode != null)
			{
				expected = transactionsNode.InnerText;
			}

			DateTime dateTime = new DateTime(2013, 11, 23);

			Transaction transaction = new Transaction(dateTime)
			{
				TransactionType = Helper.TransactionType.Outgoing,
				Amount = "-100,00",
				Category = "notExisting",
				Description = "notExisting"
			};

			// Act
			var transactionDataAccessObject = new XmlFileTransactionDataAccessObject(SampleXmlDocumentForDeletion);

			transactionDataAccessObject.DeleteTransaction(transaction);

			transactionsNode = sampleXmlDocumentForDeletion.SelectSingleNode("transactions");
			if (transactionsNode != null)
			{
				actual = transactionsNode.InnerText;
			}

			// Assert
			Assert.AreEqual(expected, actual);
		}


		/// <summary>
		/// Testet Erstellen von XML-Datei.
		/// </summary>
		[TestMethod]
		public void TestCreateXmlDocument()
		{
			// Arrange
			const string FILENAME = "Haushaltsbuch\\test.xml";

			var fileTransactionDataAccessObject = new XmlFileTransactionDataAccessObject(FILENAME);
			// Act
			//fileTransactionDataAccessObject.CreateXmlDocument(); // wird im Konstruktor bereits ausgeführt

			// Assert
			Assert.IsTrue(File.Exists(FILENAME));
		}


		#region Felder

		/// <summary>
		/// Speicherort von Testdatei mit Beispieldaten.
		/// </summary>
		private const string SampleXmlDocument = "XmlFileReaderTest\\SampleXmlDocument.xml";

		/// <summary>
		/// Speicherort von leerer Testdatei.
		/// </summary>
		private const string EmptyXmlDocument = "XmlFileReaderTest\\EmptyXmlDocument.xml";



		#endregion

		#region Methoden


		/// <summary>
		/// Initialisiert Klasse zum Bearbeiten von XML-Datei, sowie Testdateien.
		/// </summary>
		[TestInitialize]
		public void Initialize()
		{
			comparisonXmlDocument = new XmlDocument();
			emptyXmlDocument = new XmlDocument();
			sampleXmlDocumentForDeletion = new XmlDocument();
		}
		/// <summary>
		/// Testet Lesen von Beschreibungen von Ausgaben aus XML-Datei.
		/// </summary>
		[TestMethod]
		public void TestGetDescriptionsFromOutgoingTransactions()
		{
			// Arrange
			string[] expectedDescriptions =
			{
				"outgoingDescription1", "outgoingDescription2"
			};
			var transactionDataAccessObject = new XmlFileTransactionDataAccessObject(SampleXmlDocument);
			// Act
			string[] actualDescriptions = transactionDataAccessObject.GetDescriptionsFromOutgoingTransactions();

			// Assert
			CollectionAssert.AreEqual(expectedDescriptions, actualDescriptions);
		}

		/// <summary>
		/// Testet Lesen von Beschreibungen von Ausgaben aus XML-Datei mit leerer Datei.
		/// </summary>
		[TestMethod]
		public void TestGetDescriptionsFromOutgoingTransactionsWithEmptyFile()
		{
			// Arrange
			string[] expectedDescriptions = new string[0];

			var transactionDataAccessObject = new XmlFileTransactionDataAccessObject(EmptyXmlDocument);


			// Act
			string[] actualDescriptions = transactionDataAccessObject.GetDescriptionsFromOutgoingTransactions();

			// Assert
			CollectionAssert.AreEqual(expectedDescriptions, actualDescriptions);
		}

		/// <summary>
		/// Testet Lesen von Beschreibungen von Einnahmen aus XML-Datei.
		/// </summary>
		[TestMethod]
		public void TestGetDescriptionsFromIncomingTransactions()
		{
			// Arrange
			string[] expectedDescriptions =
			{
				"incomingDescription1", "incomingDescription2"
			};
			var transactionDataAccessObject = new XmlFileTransactionDataAccessObject(SampleXmlDocument);

			// Act
			string[] actualDescriptions = transactionDataAccessObject.GetDescriptionsFromIncomingTransactions();

			// Assert
			CollectionAssert.AreEqual(expectedDescriptions, actualDescriptions);
		}

		/// <summary>
		/// Testet Lesen von Beschreibungen von Einnahmen aus XML-Datei mit leerer Datei.
		/// </summary>
		[TestMethod]
		public void TestGetDescriptionsFromIncomingTransactionsWithEmptyFile()
		{
			// Arrange
			StringCollection[] expectedDescriptions = new StringCollection[0];
			var transactionDataAccessObject = new XmlFileTransactionDataAccessObject(EmptyXmlDocument);


			// Act
			string[] actualDescriptions = transactionDataAccessObject.GetDescriptionsFromIncomingTransactions();

			// Assert
			CollectionAssert.AreEqual(expectedDescriptions, actualDescriptions);
		}

		/// <summary>
		/// Testet Lesen von Kategorien von Ausgaben aus XML-Datei.
		/// </summary>
		[TestMethod]
		public void TestGetCategoriesFromOutgoingTransactions()
		{
			// Arrange
			string[] expectedCategories =
			{
				"outgoingCategory1", "outgoingCategory2"
			};
			var transactionDataAccessObject = new XmlFileTransactionDataAccessObject(SampleXmlDocument);


			// Act
			string[] actualCategories = transactionDataAccessObject.GetCategoriesFromOutgoingTransactions();

			// Assert
			CollectionAssert.AreEqual(expectedCategories, actualCategories);
		}

		/// <summary>
		/// Testet Lesen von Kategorien von Ausgaben aus XML-Datei mit leerer Datei.
		/// </summary>
		[TestMethod]
		public void TestGetCategoriesFromOutgoingTransactionsWithEmptyFile()
		{
			// Arrange
			string[] expectedCategories = new string[0];
			var transactionDataAccessObject = new XmlFileTransactionDataAccessObject(EmptyXmlDocument);

			// Act
			string[] actualCategories = transactionDataAccessObject.GetDescriptionsFromOutgoingTransactions();

			// Assert
			CollectionAssert.AreEqual(expectedCategories, actualCategories);
		}

		/// <summary>
		/// Testet Lesen von Kategorien von Einnahmen aus XML-Datei.
		/// </summary>
		[TestMethod]
		public void TestGetCategoriesFromIncomingTransactions()
		{
			// Arrange
			string[] expectedCategories =
			{
				"incomingCategory1", "incomingCategory2"
			};
			var transactionDataAccessObject = new XmlFileTransactionDataAccessObject(SampleXmlDocument);


			// Act
			string[] actualCategories = transactionDataAccessObject.GetCategoriesFromIncomingTransactions();

			// Assert
			CollectionAssert.AreEqual(expectedCategories, actualCategories);
		}

		/// <summary>
		/// Testet Lesen von Kategorien von Einnahmen aus XML-Datei mit leerer Datei.
		/// </summary>
		[TestMethod]
		public void TestGetCategoriesFromIncomingTransactionsWithEmptyFile()
		{
			// Arrange
			string[] expectedCategories = new string[0];
			var transactionDataAccessObject = new XmlFileTransactionDataAccessObject(EmptyXmlDocument);


			// Act
			string[] actualCategories = transactionDataAccessObject.GetCategoriesFromIncomingTransactions();

			// Assert
			CollectionAssert.AreEqual(expectedCategories, actualCategories);
		}

		/// <summary>
		/// Testet Lesen von Kategorien aller Einträge aus XML-Datei.
		/// </summary>
		[TestMethod]
		public void GetCategoriesFromAllTransactions()
		{
			// Arrange
			string[] expectedCategories =
			{
				string.Empty, "incomingCategory1", "incomingCategory2", "outgoingCategory1", "outgoingCategory2"
			};
			var transactionDataAccessObject = new XmlFileTransactionDataAccessObject(SampleXmlDocument);


			// Act
			string[] actualCategories = transactionDataAccessObject.GetCategoriesFromAllTransactions();

			// Assert
			CollectionAssert.AreEqual(expectedCategories, actualCategories);
		}

		/// <summary>
		/// Testet Lesen von Kategorien aller Einträge aus XML-Datei mit leerer Datei.
		/// </summary>
		[TestMethod]
		public void TestGetCategoriesFromAllTransactionsWithEmptyFile()
		{
			// Arrange
			string[] expectedCategories =
			{
				string.Empty
			};
			var transactionDataAccessObject = new XmlFileTransactionDataAccessObject(EmptyXmlDocument);


			// Act
			string[] actualCategories = transactionDataAccessObject.GetCategoriesFromAllTransactions();

			// Assert
			CollectionAssert.AreEqual(expectedCategories, actualCategories);
		}

		/// <summary>
		/// Testet Lesen von Einträgen aus XML-Datei ohne Filter.
		/// </summary>
		[TestMethod]
		public void TestGetTransactionsWithoutCategoryFilter()
		{
			// Arrange
			Filter filter = new Filter
			{
				Category = string.Empty,
				Month = "11",
				Year = "2013",
				SearchTerm = string.Empty
			};
			var transactionDataAccessObject = new XmlFileTransactionDataAccessObject(SampleXmlDocument);


			// Act
			int numberOfTransactions = transactionDataAccessObject.GetTransactions( filter).Length;

			// Assert
			Assert.AreEqual(8, numberOfTransactions);
		}

		/// <summary>
		/// Testet Lesen von Einträgen aus XML-Datei mit Filter.
		/// </summary>
		[TestMethod]
		public void TestGetTransactionsWithCategoryFilter()
		{
			// Arrange
			Filter filter = new Filter
			{
				Category = "outgoingCategory2",
				Month = "11",
				Year = "2013",
				SearchTerm = string.Empty
			};
			var transactionDataAccessObject = new XmlFileTransactionDataAccessObject(SampleXmlDocument);


			// Act
			int numberOfTransactions = transactionDataAccessObject.GetTransactions(filter).Length;

			// Assert
			Assert.AreEqual(2, numberOfTransactions);
		}

		/// <summary>
		/// Testet Lesen von allen Einträgen eines Jahres aus XML-Datei.
		/// </summary>
		[TestMethod]
		public void TestGetAllTransactionsOfYear()
		{
			// Arrange
			Filter filter = new Filter
			{
				Category = string.Empty,
				Month = "00",
				Year = "2013",
				SearchTerm = string.Empty
			};
			var transactionDataAccessObject = new XmlFileTransactionDataAccessObject(SampleXmlDocument);

			// Act
			int numberOfTransactions = transactionDataAccessObject.GetTransactions(filter).Length;

			// Assert
			Assert.AreEqual(9, numberOfTransactions);
		}

		/// <summary>
		/// Testet Lesen von Einträgen aus XML-Datei mit Suchbegriff als Filter.
		/// </summary>
		[TestMethod]
		public void TestGetTransactionsWithSearchFilter()
		{
			// Arrange
			Filter filter = new Filter
			{
				Category = string.Empty,
				Month = "00",
				Year = "2013",
				SearchTerm = "outgoingDescription2"
			};
			var transactionDataAccessObject = new XmlFileTransactionDataAccessObject(SampleXmlDocument);


			// Act
			int numberOfTransactions = transactionDataAccessObject.GetTransactions( filter).Length;

			// Assert
			Assert.AreEqual(3, numberOfTransactions);
		}

		/// <summary>
		/// Testet Lesen von Einträgen aus XML-Datei mit leerer Datei.
		/// </summary>
		[TestMethod]
		public void TestGetTransactionsWithEmptyFile()
		{
			// Arrange
			Filter filter = new Filter
			{
				Category = "outgoingCategory2",
				Month = "11",
				Year = "2013",
				SearchTerm = string.Empty
			};
			var transactionDataAccessObject = new XmlFileTransactionDataAccessObject(EmptyXmlDocument);

			// Act
			int numberOfTransactions = transactionDataAccessObject.GetTransactions( filter).Length;

			// Assert
			Assert.AreEqual(0, numberOfTransactions);
		}

		#endregion

	}
}