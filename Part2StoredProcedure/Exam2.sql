CREATE PROCEDURE sp_RecommendCrossSellProducts
    @CustomerID INT,
    @TopN INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT sod.ProductID INTO #CustomerProducts
    FROM Sales.SalesOrderHeader soh
    JOIN Sales.SalesOrderDetail sod ON soh.SalesOrderID = sod.SalesOrderID
    WHERE soh.CustomerID = @CustomerID;

    SELECT soh.CustomerID INTO #SimilarCustomers
    FROM Sales.SalesOrderHeader soh
    JOIN Sales.SalesOrderDetail sod ON soh.SalesOrderID = sod.SalesOrderID
    WHERE sod.ProductID IN (SELECT ProductID FROM #CustomerProducts)
        AND soh.CustomerID <> @CustomerID
    GROUP BY soh.CustomerID
    HAVING COUNT(DISTINCT sod.ProductID) >= 2;

    SELECT TOP (@TopN)
        sod.ProductID,
        p.Name AS ProductName,
        COUNT(DISTINCT soh.CustomerID) AS RecommendationScore
    FROM Sales.SalesOrderHeader soh
    JOIN Sales.SalesOrderDetail sod ON soh.SalesOrderID = sod.SalesOrderID
    JOIN Production.Product p ON p.ProductID = sod.ProductID
    WHERE soh.CustomerID IN (SELECT CustomerID FROM #SimilarCustomers)
        AND sod.ProductID NOT IN (SELECT ProductID FROM #CustomerProducts)
    GROUP BY sod.ProductID, p.Name
    ORDER BY RecommendationScore DESC;

    DROP TABLE #CustomerProducts;
    DROP TABLE #SimilarCustomers;
END
