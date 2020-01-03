using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.SqlServer;

namespace Lab09_LINQ
{
    class Program
    {
        
        static void Main(string[] args)
        {
            List<Customer> customers = new List<Customer>();
            List<ModifiedCustomer> modifiedCustomers = new List<ModifiedCustomer>();
            List<Product> products = new List<Product>();
            List<Category> categories = new List<Category>();

            using (var db = new Northwind())
            {
                customers = db.Customers.ToList();

                #region List_Customers_Based_on_specificCities_Skip_Some
                // Simple LINQ from Local Collection
                //Whole dataset is returned(More Data)
                var selectedCustomers = from customer in customers select customer;
                //printCustomers(selectedCustomers.ToList());

                //Same query over database directly
                //Only return actual data we need
                //Lazy loading: Query is not actuaslly executed
                Console.WriteLine("\n\nCreate custom object output\n" +
                    "===================================================n");
                var selectedCustomers2 = (from customer in db.Customers
                                          where
                customer.City == "London" || customer.City == "Berlin"
                                          orderby customer.ContactName
                                          select customer).ToList();
                //Force Data by pushing to a list . ToList() or by taking aggregate eg. Sum, Count
                printCustomers(selectedCustomers2);

                Console.WriteLine($"There are {selectedCustomers2.Count} records returned");
                #endregion

                #region List_Customers_Concatenate_CityAndCountry_into_Location
                Console.WriteLine("\n\nCreate custom object output\n" +
                    "=====================================================n");
                var selectedCustomers3 = (from customer in db.Customers
                                          select new
                                          {
                                              Name = customer.ContactName,
                                              Location = customer.City + " " + customer.Country
                                          }).Skip(10).ToList(); // Take(10) or Skip(10)
                foreach (var c in selectedCustomers3)
                {
                    Console.WriteLine($"{c.Name,-20}{c.Location}");
                }

                //or

                var selectedCustomer4 =
                    (from c in db.Customers
                     select new
                     ModifiedCustomer(c.ContactName, c.City + " " + c.Country))
                     .ToList();
                #endregion

                #region Group_and_List_Customers_ByCity
                Console.WriteLine("\n\nGROUP and list all customers by CITY\n" +
                    "=========================================================n");
                var selectCustomer5 =
                    from cust in db.Customers
                    group cust by cust.City into Cities
                    orderby Cities.Count() descending
                    where Cities.Count() > 1
                    select new
                    {
                        City = Cities.Key,
                        Count = Cities.Count()
                    };
                foreach (var c in selectCustomer5.ToList())
                {
                    Console.WriteLine($"{c.City,-15}{c.Count}");
                }
                #endregion

                #region List_Products_Inner_Join_Category_Showing_Name
                Console.WriteLine("\n\nList of Products Inner Join Category Showing Name\n" +
                   "==================================================================n");
                var listofproducts =
                     (from p in db.Products
                      join c in db.Categories
                      on p.CategoryID equals c.CategoryID
                      select new
                      {
                          ID = p.ProductID,
                          Name = p.ProductName,
                          Category = c.CategoryName
                      }).ToList();
                listofproducts.ForEach(p => Console.WriteLine($"{p.Category,-15}{p.Name,-30 }{p.Category}"));

                Console.WriteLine("\n\nNow print off the same List But using much smarter 'dot' Notation to Join Tables\n" +
                    "===========================================================================");
                products = db.Products.ToList();
                categories = db.Categories.ToList();
                products.ForEach(p => Console.WriteLine($"{p.ProductID,-15}{p.ProductName,-30}{p.Category.CategoryName}"
                    ));
                #endregion

                #region List_Categories_with_Count_of_Products_and_subList_of_ProductName
                Console.WriteLine("\n\nList Categories with Count of Products and sub List of Product Name\n" +
                "==========================================================================");
                categories.ForEach(c =>
                {
                    Console.WriteLine($"{c.CategoryID,-10}{c.CategoryName,-15} has {c.Products.Count} products");
                    //loop within loop
                    foreach (var p in c.Products)
                    {
                        Console.WriteLine($"\t\t\t\t{p.ProductID,-5}{p.ProductName}");
                    }
                });
                #endregion

                #region other_LINQ_Notations:_Distinct_OrderBy_Contains(SQL_Like)
                Console.WriteLine($"\n\nLINQ Lambda Notation\n");
                customers = db.Customers.ToList();
                Console.WriteLine($"Count is {customers.Count}");
                Console.WriteLine($"Count is {db.Customers.Count()}");

                //Distinct
                Console.WriteLine($"\n\nList of Cities: Select.. Distinct..OrderBy\n");
                Console.WriteLine("Using SELECT to select one Column\n");
                var cityList = db.Customers.Select(c => c.City).Distinct().OrderBy(c => c).ToList();
                cityList.ForEach(city => Console.WriteLine(city));

                Console.WriteLine("\n\n\nContains(same as SQL like)\n" +
                "============================================================");
                var cityListFiltered =
                    db.Customers.Select(c => c.City)
                    .Where(city => city.Contains("o"))
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();
                cityListFiltered.ForEach(city => Console.WriteLine(city));
                #endregion
            }
        }

        static void printCustomers(List<Customer> customers)
        {
            customers.ForEach(c => Console.WriteLine($"{c.CustomerID,-10}{c.ContactName,-30}{c.CompanyName,-30}{c.City}"));
        }
    }

    #region Class_Models_from_Northwind
    public partial class Customer
    {
        public string CustomerID { get; set; }
        public string CompanyName { get; set; }
        public string ContactName { get; set; }
        public string ContactTitle { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
    }
    public class Category
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public virtual ICollection<Product> Products { get; set; }

        public Category()
        {
            this.Products = new List<Product>();
        }
    }
    public class Product
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int? CategoryID { get; set; }
        public virtual Category Category { get; set; }
        public string QuantityPerUnit { get; set; }
        public decimal? UnitPrice { get; set; } = 0;
        public short? UnitsInStock { get; set; } = 0;
        public short? UnitsOnOrder { get; set; } = 0;
        public short? ReorderLevel { get; set; } = 0;
        public bool Discontinued { get; set; } = false;
    }
    public class Northwind : DbContext
    {
        public DbSet<Category> Categories { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<Customer> Customers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=(localdb)\mssqllocaldb;" + "Initial Catalog=Northwind;" + "Integrated Security = true;" + "MultipleActiveResultSets=true;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>()
                .Property(c => c.CategoryName)
                .IsRequired()
                .HasMaxLength(15);

            // define a one-to-many relationship
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Products)
                .WithOne(p => p.Category);

            modelBuilder.Entity<Product>()
                .Property(c => c.ProductName)
                .IsRequired()
                .HasMaxLength(40);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products);
        }
    }
    public class ModifiedCustomer
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public ModifiedCustomer(string name, string location)
        {

        }
    }
    #endregion
}
