using Dapper;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace InterpolatedSql.Dapper.Tests.DynamicCRUDTests
{
    public class DynamicCRUDTests
    {
        IDbConnection cn;

        #region Setup
        [SetUp]
        public void Setup()
        {
            cn = new SqlConnection(TestHelper.GetMSSQLConnectionString());
        }
        #endregion

        int businessEntityID = 1;
        string nationalIDNumber = "295847284";
        DateTime birthDate = new DateTime(1969, 01, 29);
        string maritalStatus = "S"; // single
        string gender = "M";


        [Test]
        public void TestFullInsert()
        {
            var product = new Product() { Name = "ProductName", ProductNumber = "1234", SellStartDate = DateTime.Now, ModifiedDate = DateTime.Now, SafetyStockLevel = 5, ReorderPoint = 700 };

            int deleted = cn.Execute($@"
                DELETE r FROM [Production].[Product] p INNER JOIN [Production].[ProductReview] r ON p.[ProductId]=r.[ProductId] WHERE (p.[ProductNumber]='1234' OR p.[ProductNumber]='12345');
                DELETE p FROM [Production].[Product] p WHERE (p.[ProductNumber]='1234' OR p.[ProductNumber]='12345');
            ");

            cn.Save(product);

            product.Name = "Name2";
            product.ProductNumber = "12345";
            cn.Update(product, product.ChangedProperties);
        }



    }
}
