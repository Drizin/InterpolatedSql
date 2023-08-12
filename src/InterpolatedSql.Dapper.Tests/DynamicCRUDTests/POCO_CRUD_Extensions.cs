using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace InterpolatedSql.Dapper.Tests.DynamicCRUDTests
{
    public static class POCO_CRUD_Extensions
    {
        /// <summary>
        /// Saves (if new) or Updates (if existing)
        /// </summary>
        public static void Save(this IDbConnection conn, Product p)
        {
            if (p.ProductId == default(int))
                conn.Insert(p);
            else
                conn.Update(p);
            
            p.MarkAsClean();
        }

        public static void Insert(this IDbConnection conn, Product p)
        {
            string cmd = @"
            INSERT INTO [Production].[Product]
            (
                [Class],
                [Color],
                [DaysToManufacture],
                [DiscontinuedDate],
                [FinishedGoodsFlag],
                [ListPrice],
                [MakeFlag],
                [ModifiedDate],
                [Name],
                [ProductLine],
                [ProductModelID],
                [ProductNumber],
                [ProductSubcategoryID],
                [ReorderPoint],
                [SafetyStockLevel],
                [SellEndDate],
                [SellStartDate],
                [Size],
                [SizeUnitMeasureCode],
                [StandardCost],
                [Style],
                [Weight],
                [WeightUnitMeasureCode]
            )
            VALUES
            (
                @Class,
                @Color,
                @DaysToManufacture,
                @DiscontinuedDate,
                @FinishedGoodsFlag,
                @ListPrice,
                @MakeFlag,
                @ModifiedDate,
                @Name,
                @ProductLine,
                @ProductModelId,
                @ProductNumber,
                @ProductSubcategoryId,
                @ReorderPoint,
                @SafetyStockLevel,
                @SellEndDate,
                @SellStartDate,
                @Size,
                @SizeUnitMeasureCode,
                @StandardCost,
                @Style,
                @Weight,
                @WeightUnitMeasureCode
            )";

            p.ProductId = conn.QuerySingle<int>(cmd + "SELECT SCOPE_IDENTITY();", p);
        }

        public static void Update(this IDbConnection conn, Product p)
        {
            string cmd = @"
            UPDATE [Production].[Product] SET
                [Class] = @Class,
                [Color] = @Color,
                [DaysToManufacture] = @DaysToManufacture,
                [DiscontinuedDate] = @DiscontinuedDate,
                [FinishedGoodsFlag] = @FinishedGoodsFlag,
                [ListPrice] = @ListPrice,
                [MakeFlag] = @MakeFlag,
                [ModifiedDate] = @ModifiedDate,
                [Name] = @Name,
                [ProductLine] = @ProductLine,
                [ProductModelID] = @ProductModelId,
                [ProductNumber] = @ProductNumber,
                [ProductSubcategoryID] = @ProductSubcategoryId,
                [ReorderPoint] = @ReorderPoint,
                [SafetyStockLevel] = @SafetyStockLevel,
                [SellEndDate] = @SellEndDate,
                [SellStartDate] = @SellStartDate,
                [Size] = @Size,
                [SizeUnitMeasureCode] = @SizeUnitMeasureCode,
                [StandardCost] = @StandardCost,
                [Style] = @Style,
                [Weight] = @Weight,
                [WeightUnitMeasureCode] = @WeightUnitMeasureCode
            WHERE
                [ProductID] = @ProductId";

            conn.Execute(cmd, p);
        }

        public static void Update(this IDbConnection conn, Product p, HashSet<string> changedProperties)
        {
            if (!changedProperties.Any())
                return;

            var cmd = conn.SqlBuilder($@"
            UPDATE [Production].[Product] SET
            /**sets**/
            WHERE
                [ProductID] = @ProductId");
            cmd.Replace("/**sets**/", FormattableStringFactory.Create(string.Join("," + Environment.NewLine, changedProperties.Select(prop => $"[{prop}] = @{prop}"))));

            //Assert.AreEqual // "UPDATE [Production].[Product] SET\r\n[Name] = @Name,\r\n[ProductNumber] = @ProductNumber\r\nWHERE\r\n    [ProductID] = @ProductId"

            cmd.AddObjectProperties(p);
            cmd.Execute();
        }
    }
}
