using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transaction_Management.Services
{
    /// <summary>
    /// Service qu?n l? c?u h?nh ti?n t? và ð?nh d?ng hi?n th? s? cho ?ng d?ng.
    /// </summary>
    internal class CurrencyConfigService
    {
        /// <summary>
        /// Mô h?nh d? li?u ð?i di?n cho m?t lo?i ti?n t?.
        /// </summary>
        public class CurrencyModel
        {
            /// <summary>M? ti?n t? (VND, USD, EUR).</summary>
            public string Code { get; set; }

            /// <summary>Tên ti?n t? (Vi?t Nam Ð?ng, Ðô la M?).</summary>
            public string Name { get; set; }

            /// <summary>K? hi?u ti?n t? (ð, $, €).</summary>
            public string Symbol { get; set; }

            /// <summary>Tr? v? chu?i hi?n th? tên và k? hi?u.</summary>
            public override string ToString() => $"{Name} ({Symbol})";
        }

        /// <summary>
        /// Mô h?nh d? li?u ð?i di?n cho m?t ð?nh d?ng hi?n th? s?.
        /// </summary>
        public class FormatModel
        {
            /// <summary>ID duy nh?t ð? lýu tr? ð?nh d?ng.</summary>
            public string Id { get; set; }

            /// <summary>Ví d? hi?n th? (1.000.000 ð, $1,000,000.00).</summary>
            public string DisplayText { get; set; }

            /// <summary>Chu?i ð?nh d?ng C# (N0, N2, v.v.).</summary>
            public string FormatString { get; set; }

            /// <summary>Tr? v? chu?i ví d? ð?nh d?ng.</summary>
            public override string ToString() => DisplayText;
        }

        /// <summary>
        /// L?y danh sách ti?n t? ðý?c h? tr? b?i h? th?ng.
        /// </summary>
        /// <returns>Danh sách các lo?i ti?n t? (VND, USD, EUR).</returns>
        public List<CurrencyModel> GetSupportedCurrencies()
        {
            return new List<CurrencyModel>
            {
                new CurrencyModel { Code = "VND", Name = "Vi?t Nam Ð?ng", Symbol = "ð" },
                new CurrencyModel { Code = "USD", Name = "Ðô la M?", Symbol = "$" },
                new CurrencyModel { Code = "EUR", Name = "Euro", Symbol = "€" }
            };
        }

        /// <summary>
        /// L?y danh sách ð?nh d?ng hi?n th? s? ðý?c h? tr?.
        /// </summary>
        /// <returns>Danh sách các ð?nh d?ng s?.</returns>
        public List<FormatModel> GetSupportedFormats()
        {
            return new List<FormatModel>
            {
                new FormatModel { Id = "Standard_VND", DisplayText = "1.000.000 ð", FormatString = "{0:N0} ð" },
                new FormatModel { Id = "Standard_USD", DisplayText = "$1,000,000.00", FormatString = "${0:N2}" },
                new FormatModel { Id = "Plain_Number", DisplayText = "1 000 000", FormatString = "#,##0" }
            };
        }

        /// <summary>
        /// T?i m? ti?n t? ð? lýu trý?c ðó (m?c ð?nh là VND).
        /// </summary>
        /// <returns>M? ti?n t? ðý?c lýu.</returns>
        public string LoadSavedCurrencyCode()
        {
            // TODO: Thay b?ng ð?c t? Database / JSON file / AppSettings n?u c?n
            // T?m th?i m?c ð?nh là VND n?u ch?y l?n ð?u
            return "VND";
        }

        /// <summary>
        /// T?i ID ð?nh d?ng ð? lýu trý?c ðó.
        /// </summary>
        /// <returns>ID ð?nh d?ng ðý?c lýu.</returns>
        public string LoadSavedFormatId()
        {
            return "Standard_VND";
        }

        /// <summary>
        /// Lýu c?u h?nh ti?n t? m?i xu?ng h? th?ng.
        /// </summary>
        /// <param name="currencyCode">M? ti?n t? c?n lýu.</param>
        public void SaveCurrencySetting(string currencyCode)
        {
            // Logic lýu tr? c?u h?nh 
            System.Diagnostics.Debug.WriteLine($"[Service] Ð? lýu m? ti?n t? m?i: {currencyCode}");
        }

        /// <summary>
        /// Lýu c?u h?nh ð?nh d?ng m?i xu?ng h? th?ng.
        /// </summary>
        /// <param name="formatId">ID ð?nh d?ng c?n lýu.</param>
        public void SaveFormatSetting(string formatId)
        {
            // Logic lýu tr? c?u h?nh
            System.Diagnostics.Debug.WriteLine($"[Service] Ð? lýu ID ð?nh d?ng m?i: {formatId}");
        }
    }
}
