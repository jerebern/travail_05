using BillingManagement.Business;
using BillingManagement.Models;
using BillingManagement.UI.ViewModels.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace BillingManagement.UI.ViewModels
{
    class MainViewModel : BaseViewModel
    {
		private BaseViewModel _vm;
		BillingManagementContext db = new BillingManagementContext();
		private ObservableCollection<Invoice> dbInvoices;		
		private ObservableCollection<Customer> dbCustomers;

		public BillingManagementContext DB
		{
			get => db;

			set
			{
				db = value;
				OnPropertyChanged();
			}

		}

		public ObservableCollection<Customer> DbCustomers
		{
			get => dbCustomers;

			set
			{
				dbCustomers = value;
				OnPropertyChanged();

			}

		}

		public ObservableCollection<Invoice> DbInvoices
		{
			get => dbInvoices;

			set
			{

				dbInvoices = value;
				OnPropertyChanged();
			}

		}

		public BaseViewModel VM
		{
			get { return _vm; }
			set {
				_vm = value;
				OnPropertyChanged();
			}
		}

		private string searchCriteria;

		public string SearchCriteria
		{
			get { return searchCriteria; }
			set { 
				searchCriteria = value;
				OnPropertyChanged();
			}
		}


		CustomerViewModel customerViewModel;
		InvoiceViewModel invoiceViewModel;

		public ChangeViewCommand ChangeViewCommand { get; set; }

		public DelegateCommand<object> AddNewItemCommand { get; private set; }

		public DelegateCommand<Invoice> DisplayInvoiceCommand { get; private set; }
		public DelegateCommand<Customer> DisplayCustomerCommand { get; private set; }

		public DelegateCommand<Customer> AddInvoiceToCustomerCommand { get; private set; }

		//COMMAND PAR MOI
		public RelayCommand<Customer> SearchCommand { get; set; }
		public MainViewModel()
		{
			InsertDataInDatabase();

			dbInvoices = new ObservableCollection<Invoice>();
			dbCustomers = new ObservableCollection<Customer>();

			var sort = dbCustomers.OrderBy(x => x.LastName);
			var CustomersSorted = new ObservableCollection<Customer>(sort);
			SearchCommand = new RelayCommand<Customer>(SearchCustomer, CanAddNewItem);
			ChangeViewCommand = new ChangeViewCommand(ChangeView);
			DisplayInvoiceCommand = new DelegateCommand<Invoice>(DisplayInvoice);
			DisplayCustomerCommand = new DelegateCommand<Customer>(DisplayCustomer);

			AddNewItemCommand = new DelegateCommand<object>(AddNewItem, CanAddNewItem);
			AddInvoiceToCustomerCommand = new DelegateCommand<Customer>(AddInvoiceToCustomer);

			customerViewModel = new CustomerViewModel();
			invoiceViewModel = new InvoiceViewModel(customerViewModel.Customers);
					   
			VM = customerViewModel;

		}

		private void ChangeView(string vm)
		{
			switch (vm)
			{
				case "customers":
					VM = customerViewModel;
					break;
				case "invoices":
					VM = invoiceViewModel;
					break;
			}
		}

		private void DisplayInvoice(Invoice invoice)
		{
			invoiceViewModel.SelectedInvoice = invoice;
			VM = invoiceViewModel;
		}

		private void DisplayCustomer(Customer customer)
		{
			customerViewModel.SelectedCustomer = customer;
			VM = customerViewModel;
		}

		private void AddInvoiceToCustomer(Customer c)
		{
			var invoice = new Invoice(c);
			c.Invoices.Add(invoice);
			DisplayInvoice(invoice);
		}

		private void AddNewItem (object item)
		{
			if (VM == customerViewModel)
			{
				var c = new Customer();
				customerViewModel.Customers.Add(c);
				customerViewModel.SelectedCustomer = c;
			}
		}

		private void SearchCustomer(object parametre)
		{
			string user_input = searchCriteria as string;

			Customer SelectedCustomer = customerViewModel.SelectedCustomer;
			IEnumerable<Customer> customers = customerViewModel.Customers.ToList<Customer>();
			IEnumerable<Customer> FoundCustomer = null;
			

			//Eviter erreur par ce Input vide
			if(user_input != null)
			{
							
			FoundCustomer = customers.Where(c => c.Name.ToUpper().StartsWith(user_input.ToUpper()) || c.LastName.ToUpper().StartsWith(user_input.ToUpper()));

			if (FoundCustomer != null &	FoundCustomer.Count() < 0)
				MessageBox.Show("Aucun " + user_input + " trouvé");
			else
				customerViewModel.SelectedCustomer = FoundCustomer.First();
			}
		}
		

		private void getDataFromDatabase()
		{
						
			List<Invoice> InvoicesList = db.Invoices.ToList();
			DbInvoices.Clear();
			foreach (Invoice i in InvoicesList)
				DbInvoices.Add(i);
			if (invoiceViewModel != null) invoiceViewModel.SelectedInvoice = DbInvoices.First();

			List<Customer> CustomersList = db.Customers.ToList();
			DbCustomers.Clear();
			foreach (Customer c in CustomersList)
				DbCustomers.Add(c);

			DbCustomers = new ObservableCollection<Customer>(DbCustomers.OrderBy(c => c.LastName));

			//if (customerViewModel != null) customerViewModel.Customers = Customers;
			//if (customerViewModel != null) customerViewModel.SelectedCustomer = Customers.First();

		}

		private void InsertDataInDatabase()
		{
			if(db.Customers.Count() <= 0)
			{	

				List<Customer> Customers = new CustomersDataService().GetAll().ToList();
				List<Invoice> Invoices = new InvoicesDataService(Customers).GetAll().ToList();

				foreach (Customer c in Customers)
				{
					db.Customers.Add(c);
					db.SaveChanges();
				}

			}


		}

		private bool CanAddNewItem(object o)
		{
			bool result = false;

			result = VM == customerViewModel;
			return result;
		}

	}
}
