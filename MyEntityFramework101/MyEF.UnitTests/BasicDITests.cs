using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MyEF.UnitTests
{
    [TestClass]
    public class BasicDITests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var services = new ServiceCollection();
            services.AddDbContext<MyEF.Data.MyEfContext>(options => options.UseSqlite("Data Source=MyEF.db;Cache=Shared"));
            var provider = services.BuildServiceProvider();

            var dbContext = provider.GetService<MyEF.Data.MyEfContext>();
            dbContext.Database.EnsureCreated();
            var dob = new DateTime(2000, 12, 25);
            dbContext.Customer.Add(new Domain.entity.Customer { FirstName = "John", LastName = "Doe", DateOfBirth = dob, Email = "john.doe@cool.com" });
            dbContext.SaveChanges();
        }
    }
}