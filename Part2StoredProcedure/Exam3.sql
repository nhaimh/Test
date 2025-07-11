CREATE FUNCTION fn_AnalyzeRepeatPurchaseBehavior
(
    @CustomerID INT
)
RETURNS TABLE
AS
RETURN
(
    WITH PurchaseDates AS (
        SELECT DISTINCT
            psc.ProductSubcategoryID,
            psc.Name AS ProductSubcategoryName,
            CAST(soh.OrderDate AS DATE) AS OrderDate
        FROM Sales.SalesOrderHeader soh
        JOIN Sales.SalesOrderDetail sod ON soh.SalesOrderID = sod.SalesOrderID
        JOIN Production.Product p ON p.ProductID = sod.ProductID
        JOIN Production.ProductSubcategory psc ON p.ProductSubcategoryID = psc.ProductSubcategoryID
        WHERE soh.CustomerID = @CustomerID
    ),
    RankedDates AS (
        SELECT *,
            ROW_NUMBER() OVER (PARTITION BY ProductSubcategoryID ORDER BY OrderDate) AS rn
        FROM PurchaseDates
    ),
    DateDiffs AS (
        SELECT
            cur.ProductSubcategoryID,
            cur.ProductSubcategoryName,
            DATEDIFF(DAY, prev.OrderDate, cur.OrderDate) AS DayGap
        FROM RankedDates cur
        JOIN RankedDates prev
            ON cur.ProductSubcategoryID = prev.ProductSubcategoryID
           AND cur.rn = prev.rn + 1
    )
    SELECT
        ProductSubcategoryID,
        ProductSubcategoryName,
        COUNT(*) + 1 AS PurchaseCount,
        ROUND(AVG(CAST(DayGap AS FLOAT)), 2) AS AvgDayGap
    FROM DateDiffs
    GROUP BY ProductSubcategoryID, ProductSubcategoryName
);
