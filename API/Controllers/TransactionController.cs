// API/Controllers/TransactionsController.cs
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Services;
using API.DTOs;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : BaseApiController
    {
        private readonly TransactionService _transactionService;

        public TransactionsController(TransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet("duplicates")]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetDuplicateTransactions()
        {
            var duplicateTransactions = await _transactionService.GetDuplicateTransactionsAsync();
            return Ok(duplicateTransactions);
        }
    }
}
