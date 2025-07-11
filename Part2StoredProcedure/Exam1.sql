CREATE PROCEDURE sp_AnalyzeYearlyGrowth
    @StartYear INT,
    @EndYear INT,
    @ProductCategoryID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH RevenueCTE AS (
        SELECT
            YEAR(soh.OrderDate) AS OrderYear,
            SUM(soh.TotalDue) AS TotalRevenue
        FROM Sales.SalesOrderHeader AS soh
        INNER JOIN Sales.SalesOrderDetail AS sod ON soh.SalesOrderID = sod.SalesOrderID
        INNER JOIN Production.Product AS p ON sod.ProductID = p.ProductID
        INNER JOIN Production.ProductSubcategory AS sub ON p.ProductSubcategoryID = sub.ProductSubcategoryID
        INNER JOIN Production.ProductCategory AS cat ON sub.ProductCategoryID = cat.ProductCategoryID
        WHERE YEAR(soh.OrderDate) BETWEEN @StartYear AND @EndYear
            AND (@ProductCategoryID IS NULL OR cat.ProductCategoryID = @ProductCategoryID)
        GROUP BY YEAR(soh.OrderDate)
    )
    SELECT
        r1.OrderYear,
        r1.TotalRevenue,
        ROUND(
            CASE WHEN r0.TotalRevenue IS NULL THEN NULL
                 ELSE ((r1.TotalRevenue - r0.TotalRevenue) * 100.0 / r0.TotalRevenue)
            END, 2) AS YoYGrowthRate
    FROM RevenueCTE r1
    LEFT JOIN RevenueCTE r0 ON r0.OrderYear = r1.OrderYear - 1
    ORDER BY r1.OrderYear;
END
