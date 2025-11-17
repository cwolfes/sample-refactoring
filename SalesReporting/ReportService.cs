using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SalesReporting
{
    /// <summary>
    /// Generates a simple sales report for a given year and month.
    /// </summary>
    public class ReportService
    {
        // Exchange‑rates table
        private static readonly IReadOnlyDictionary<string, double> ExchangeRates
            = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
            {
                { "USD", 1.0 },   // base currency
                { "EUR", 1.1 },
                { "GBP", 1.3 }
            };

        // Public API

        /// <summary>
        /// Reads *sales.json*, filters by year/month, aggregates the totals,
        /// and writes a human‑readable report to a file.
        /// </summary>
        public async Task CreateReportForYearAndMonthAsync(int year, int month)
        {
            var allSales = await LoadSalesAsync("sales.json");
            var filtered = allSales
                .Where(s => s.Date.Year == year && s.Date.Month == month)
                .ToList();

            double totalUsd = filtered.Sum(s => s.Amount * GetExchangeRate(s.Currency));

            var reportLines = new[]
            {
                $"Monatlicher Verkaufsbericht ({month:D2}/{year})",
                new string('-', 40),
                $"Gesamt Umsatz in USD: {totalUsd:N2}"
            };

            var fileName = $"report_{year}_{month:D2}.txt";
            await File.WriteAllLinesAsync(fileName, reportLines);

            Console.WriteLine($"Report generated successfully: {fileName}");
        }

      
        /// <summary>
        /// Looks up the exchange rate for a given currency.
        /// If the currency is unknown, it defaults to 1.0 (USD).
        /// </summary>
        private static double GetExchangeRate(string currency)
            => ExchangeRates.TryGetValue(currency, out var rate) ? rate : 1.0;

        /// <summary>
        /// Reads the JSON file and returns a <see cref="List{SaleObject}"/>.
        /// Throws if deserialization fails.
        /// </summary>
        private static async Task<List<SaleObject>> LoadSalesAsync(string path)
        {
            var json = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<List<SaleObject>>(json)
                   ?? throw new InvalidOperationException($"Unable to deserialize {path}.");
        }
    }

    /// <summary>
    /// Model representing a single sale record.
    /// </summary>
    public sealed class SaleObject
    {
        public DateTime Date { get; set; }
        public double Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
    }
}