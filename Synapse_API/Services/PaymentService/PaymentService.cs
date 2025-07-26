using Synapse_API.Models.Entities;
using System.Net;
using Synapse_API.Utils;
using Microsoft.AspNetCore.Mvc;
using Synapse_API.Data;
using Synapse_API.Models.Dto.PaymentDTOs;
using Synapse_API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Synapse_API.Services.PaymentService
{
    public class PaymentService
    {
        private readonly IConfiguration _config;
        private readonly SynapseDbContext _context;

        public PaymentService(IConfiguration config, SynapseDbContext context)
        {
            _config = config;
            _context = context;
        }

        // PHƯƠNG THỨC MỚI: Khởi tạo thanh toán
        public async Task<string> CreatePaymentAsync(int userId, HttpContext context)
        {
            // 1. Tạo một bản ghi Payment trong DB với trạng thái "Pending"
            var payment = new Payment
            {
                UserID = userId,
                Amount = 50000, // Giá 50,000 VND
                Status = PaymentStatus.Pending,
                PaymentDate = DateTime.UtcNow,
                PaymentMethod = PaymentMethodEnum.VNPay
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync(); // Lưu để lấy PaymentID

            // 2. Gọi hàm helper để tạo URL
            return CreatePaymentUrlHelper(payment, context);
        }

        // PHƯƠNG THỨC MỚI: Xử lý callback
        public async Task<(string redirectUrl, bool isSuccess)> ProcessVnpayCallbackAsync(VnpayCallbackDto response)
        {
            var frontendSuccessUrl = _config["Vnpay:FrontendSuccessUrl"]; // Lấy từ appsettings
            var frontendFailedUrl = _config["Vnpay:FrontendFailedUrl"];   // Lấy từ appsettings

            // 1. Kiểm tra chữ ký
            var isValidSignature = IsValidSignature(response);
            if (!isValidSignature)
            {
                return ($"{frontendFailedUrl}?error=invalid_signature", false);
            }

            var payment = await _context.Payments.FindAsync(int.Parse(response.TxnRef));
            if (payment == null)
            {
                return ($"{frontendFailedUrl}?error=order_not_found", false);
            }

            // 2. Kiểm tra mã phản hồi và cập nhật DB
            if (response.ResponseCode == "00") // Thành công
            {
                // Chỉ xử lý nếu giao dịch đang chờ
                if (payment.Status == PaymentStatus.Pending)
                {
                    payment.Status = PaymentStatus.Completed;

                    var subscription = await _context.Subscriptions
                        .FirstOrDefaultAsync(s => s.UserID == payment.UserID);

                    if (subscription != null)
                    {
                        subscription.PlanType = PlanType.Premium;
                        subscription.StartDate = DateTime.UtcNow;
                        subscription.EndDate = DateTime.UtcNow.AddMonths(1);
                        subscription.IsActive = true;
                        _context.Subscriptions.Update(subscription);
                    }
                    else
                    {
                        _context.Subscriptions.Add(new Subscription
                        {
                            UserID = payment.UserID,
                            PlanType = PlanType.Premium,
                            StartDate = DateTime.UtcNow,
                            EndDate = DateTime.UtcNow.AddMonths(1),
                            IsActive = true
                        });
                    }
                    await _context.SaveChangesAsync();
                }
                return ($"{frontendSuccessUrl}?orderId={payment.PaymentID}", true);
            }
            else // Thất bại
            {
                payment.Status = PaymentStatus.Failed;
                _context.Payments.Update(payment);
                await _context.SaveChangesAsync();
                return ($"{frontendFailedUrl}?orderId={payment.PaymentID}&errorCode={response.ResponseCode}", false);
            }
        }

        // ---- CÁC HÀM HELPER PRIVATE ----

        private string CreatePaymentUrlHelper(Payment payment, HttpContext context)
        {
            var timeZoneById = _config["TimeZoneId"];
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneById);
            var vnpayTmnCode = _config["Vnpay:TmnCode"];
            var vnpayHashSecret = _config["Vnpay:HashSecret"];
            var vnpayUrl = _config["Vnpay:BaseUrl"];
            var vnpayReturnUrl = _config["Vnpay:PaymentBackReturnUrl"];

            var pay = new SortedList<string, string>(new VnpayComparer())
            {
                { "vnp_Version", "2.1.0" },
                { "vnp_Command", "pay" },
                { "vnp_TmnCode", vnpayTmnCode },
                { "vnp_Amount", ((long)payment.Amount * 100).ToString() },
                { "vnp_CreateDate", TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneInfo).ToString("yyyyMMddHHmmss") },
                { "vnp_CurrCode", "VND" },
                { "vnp_IpAddr", GetIpAddress(context) },
                { "vnp_Locale", "vn" },
                { "vnp_OrderInfo", $"Thanh toan goi Premium cho user {payment.UserID}" },
                { "vnp_OrderType", "other" },
                { "vnp_ReturnUrl", vnpayReturnUrl },
                { "vnp_TxnRef", payment.PaymentID.ToString() }
            };

            var data = string.Join("&", pay.Select(kvp => $"{kvp.Key}={WebUtility.UrlEncode(kvp.Value)}"));
            var vnp_SecureHash = VnPayLibrary.HmacSHA512(vnpayHashSecret, data);

            return $"{vnpayUrl}?{data}&vnp_SecureHash={vnp_SecureHash}";
        }

        private bool IsValidSignature(VnpayCallbackDto response)
        {
            var vnpayHashSecret = _config["Vnpay:HashSecret"];
            var pay = new SortedList<string, string>(new VnpayComparer());

            // Dùng reflection để lấy tất cả thuộc tính và giá trị của DTO
            foreach (var property in response.GetType().GetProperties())
            {
                var value = property.GetValue(response)?.ToString();
                if (!string.IsNullOrEmpty(value) && property.Name != "SecureHash")
                {
                    var queryAttr = property.GetCustomAttributes(typeof(FromQueryAttribute), false).FirstOrDefault() as FromQueryAttribute;
                    if (queryAttr != null)
                    {
                        pay.Add(queryAttr.Name, value);
                    }
                }
            }

            var checkSumData = string.Join("&", pay.Select(kvp => $"{kvp.Key}={WebUtility.UrlEncode(kvp.Value)}"));
            var checkSum = VnPayLibrary.HmacSHA512(vnpayHashSecret, checkSumData);

            return checkSum.Equals(response.SecureHash, StringComparison.InvariantCultureIgnoreCase);
        }

        private string GetIpAddress(HttpContext context)
        {
            var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = context.Connection.RemoteIpAddress?.ToString();
            }
            return ipAddress ?? "127.0.0.1";
        }

        public class VnpayComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                if (x == y) return 0;
                if (x == null) return -1;
                if (y == null) return 1;
                return string.CompareOrdinal(x, y);
            }
        }
    }
}