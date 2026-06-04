using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transaction_Management.Services
{
    internal class CurrencyConfigService
    {
        // Cấu trúc dữ liệu đại diện cho Tiền tệ
        public class CurrencyModel
        {
            public string Code { get; set; }       // VND, USD, EUR
            public string Name { get; set; }       // Việt Nam Đồng, Đô la Mỹ
            public string Symbol { get; set; }     // đ, $, €
            public override string ToString() => $"{Name} ({Symbol})";
        }

        // Cấu trúc dữ liệu đại diện cho Định dạng hiển thị số
        public class FormatModel
        {
            public string Id { get; set; }         // chuẩn id lưu trữ
            public string DisplayText { get; set; } // 1.000.000 đ
            public string FormatString { get; set; } // {0:N0} đ
            public override string ToString() => DisplayText;
        }

        
            // Danh sách hệ thống hỗ trợ sẵn
            public List<CurrencyModel> GetSupportedCurrencies()
            {
                return new List<CurrencyModel>
            {
                new CurrencyModel { Code = "VND", Name = "Việt Nam Đồng", Symbol = "đ" },
                new CurrencyModel { Code = "USD", Name = "Đô la Mỹ", Symbol = "$" },
                new CurrencyModel { Code = "EUR", Name = "Euro", Symbol = "€" }
            };
            }

            public List<FormatModel> GetSupportedFormats()
            {
                return new List<FormatModel>
            {
                new FormatModel { Id = "Standard_VND", DisplayText = "1.000.000 đ", FormatString = "{0:N0} đ" },
                new FormatModel { Id = "Standard_USD", DisplayText = "$1,000,000.00", FormatString = "${0:N2}" },
                new FormatModel { Id = "Plain_Number", DisplayText = "1 000 000", FormatString = "#,##0" }
            };
            }

            // Đọc cấu hình Code tiền tệ đã lưu
            public string LoadSavedCurrencyCode()
            {
                // TODO: Thay bằng đọc từ Database / JSON file / AppSettings nếu cần
                // Tạm thời mặc định là VND nếu chạy lần đầu
                return "VND";
            }

            // Đọc cấu hình Định dạng đã lưu
            public string LoadSavedFormatId()
            {
                return "Standard_VND";
            }

            // Lưu cấu hình xuống hệ thống khi có thay đổi
            public void SaveCurrencySetting(string currencyCode)
            {
                // Logic lưu trữ cấu hình 
                System.Diagnostics.Debug.WriteLine($"[Service] Đã lưu mã tiền tệ mới: {currencyCode}");
            }

            public void SaveFormatSetting(string formatId)
            {
                // Logic lưu trữ cấu hình
                System.Diagnostics.Debug.WriteLine($"[Service] Đã lưu ID định dạng mới: {formatId}");
            }
        }
    }
