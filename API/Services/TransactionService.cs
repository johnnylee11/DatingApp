using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using API.Entities;
using API.DTOs;
using AutoMapper;
using Microsoft.Extensions.Logging;
using API.Data;

namespace API.Services
{
    public class TransactionService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<TransactionService> _logger;
        private readonly string[] dateTimeFormats = { "yyyy-MM-dd HH:mm:ss.fff", "yyyy-MM-dd HH:mm:ss:fff", "dd MMM yyyy HH:mm:ss:fff" };

        public TransactionService(DataContext context, IMapper mapper, ILogger<TransactionService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<TransactionDto>> GetTransactionsAsync()
        {
            _logger.LogInformation("Starting GetTransactionsAsync task.");
            var transactions = await _context.Transactions.ToListAsync();
            return _mapper.Map<List<TransactionDto>>(transactions);
        }

        public async Task<TransactionDto> GetTransactionByMsisdnAsync(long msisdn)
        {
            _logger.LogInformation($"Starting GetTransactionByMsisdnAsync task for MSISDN: {msisdn}.");
            var transaction = await _context.Transactions.FirstOrDefaultAsync(t => t.MSISDN == msisdn);
            return _mapper.Map<TransactionDto>(transaction);
        }

        public async Task<List<TransactionDto>> GetTransactionsByCountryAsync(string country)
        {
            _logger.LogInformation($"Starting GetTransactionsByCountryAsync task for country: {country}.");
            var transactions = await _context.Transactions.Where(t => t.Country == country).ToListAsync();
            return _mapper.Map<List<TransactionDto>>(transactions);
        }

        public async Task<List<TransactionDto>> GetDuplicateTransactionsAsync()
        {
            try
            {
                _logger.LogInformation("Starting GetDuplicateTransactionsAsync task.");

                var transactions = await _context.Transactions
                    .OrderBy(t => t.MSISDN)
                    .ThenBy(t => t.BroadcastDate)
                    .ToListAsync();

                var duplicateTransactions = new List<Transaction>();

                for (int i = 1; i < transactions.Count; i++)
                {
                    var currentTransaction = transactions[i];
                    var previousTransaction = transactions[i - 1];

                    if (currentTransaction.MSISDN == previousTransaction.MSISDN)
                    {
                        _logger.LogInformation($"Checking duplicates for MSISDN: {currentTransaction.MSISDN}");

                        if (TryParseDate(currentTransaction.BroadcastDate, out var currentBroadcastDate) &&
                            TryParseDate(previousTransaction.BroadcastDate, out var previousBroadcastDate))
                        {
                            var timeDifference = (currentBroadcastDate - previousBroadcastDate).TotalSeconds;

                            _logger.LogInformation($"Time difference between {previousBroadcastDate} and {currentBroadcastDate} is {timeDifference} seconds");

                            if (timeDifference <= 3)
                            {
                                duplicateTransactions.Add(currentTransaction);
                                duplicateTransactions.Add(previousTransaction);
                                _logger.LogInformation($"Duplicate found: MSISDN={currentTransaction.MSISDN}, TimeDifference={timeDifference}");
                            }
                            else
                            {
                                _logger.LogInformation($"No duplicate: MSISDN={currentTransaction.MSISDN}, TimeDifference={timeDifference}");
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"Date parsing failed for: Current={currentTransaction.BroadcastDate}, Previous={previousTransaction.BroadcastDate}");
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"MSISDNs do not match: Current={currentTransaction.MSISDN}, Previous={previousTransaction.MSISDN}");
                    }
                }

                var distinctDuplicateTransactions = duplicateTransactions.Distinct().ToList();
                return _mapper.Map<List<TransactionDto>>(distinctDuplicateTransactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting duplicate transactions.");
                throw;
            }
        }

        private bool TryParseDate(string dateString, out DateTime date)
        {
            foreach (var format in dateTimeFormats)
            {
                if (DateTime.TryParseExact(dateString.Trim(), format, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {
                    return true;
                }
            }

            _logger.LogWarning($"Date parsing failed for: {dateString} using formats: {string.Join(", ", dateTimeFormats)}");
            date = default;
            return false;
        }
    }
}
