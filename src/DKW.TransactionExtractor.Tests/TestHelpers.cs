using System;
using DKW.TransactionExtractor.Models;

namespace DKW.TransactionExtractor.Tests;

public static class TestHelpers
{
    public static Transaction CreateTransaction(string description, decimal amount, DateTime? date = null)
    {
        var dt = date ?? DateTime.Today;
        return new Transaction
        {
            StatementDate = dt,
            TransactionDate = dt,
            PostedDate = dt,
            Description = description,
            Amount = amount
        };
    }
}
