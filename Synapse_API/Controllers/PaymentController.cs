using Microsoft.AspNetCore.Authorization;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Synapse_API.Data;
using Synapse_API.Models.Entities;
using Synapse_API.Models.Enums;
using Synapse_API.Services;
using Synapse_API.Utils;
using Synapse_API.Services.PaymentService;
using Microsoft.EntityFrameworkCore;
using Synapse_API.Models.Dto.PaymentDTOs;

namespace Synapse_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService _paymentService;
        private readonly UserService _userService;

        // Chỉ giữ lại những service thực sự cần thiết
        public PaymentController(PaymentService paymentService, UserService userService)
        {
            _paymentService = paymentService;
            _userService = userService;
        }

        [HttpPost("create-payment-url")]
        [Authorize]
        public async Task<IActionResult> CreatePaymentUrl()
        {
            var userId = _userService.GetMyUserId(User);
            if (userId == 0)
            {
                return Unauthorized("User not found.");
            }

            // Ủy quyền hoàn toàn cho Service
            var paymentUrl = await _paymentService.CreatePaymentAsync(userId, HttpContext);

            return Ok(new { PaymentUrl = paymentUrl });
        }

        [HttpGet("payment-callback")]
        public async Task<IActionResult> PaymentCallback([FromQuery] VnpayCallbackDto response)
        {
            // Ủy quyền hoàn toàn cho Service
            var (redirectUrl, _) = await _paymentService.ProcessVnpayCallbackAsync(response);

            // Chỉ việc redirect theo kết quả từ service
            return Redirect(redirectUrl);
        }
    }
}
