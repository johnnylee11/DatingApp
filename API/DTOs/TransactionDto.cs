// API/DTOs/TransactionDto.cs
using System;

namespace API.DTOs
{
    public class TransactionDto
    {
        public long MSISDN { get; set; }
        public string Message { get; set; }
        public string BroadcastDate { get; set; }
        public string Country { get; set; }
    }
}
