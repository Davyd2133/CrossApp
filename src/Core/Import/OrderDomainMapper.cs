using System;
using System.Collections.Generic;
using Core.Domain;
using Core.Dto;

namespace Core.Import;

public static class OrderDomainMapper
{
    public static OrderDomainImportResult Convert<TImportResult>(
        TImportResult importResult,
        Func<TImportResult, IEnumerable<OrderDto>> getOrders,
        Func<TImportResult, IEnumerable<string>> getErrors)
    {
        if (importResult is null)
        {
            throw new ArgumentNullException(nameof(importResult));
        }

        if (getOrders is null)
        {
            throw new ArgumentNullException(nameof(getOrders));
        }

        if (getErrors is null)
        {
            throw new ArgumentNullException(nameof(getErrors));
        }

        IEnumerable<OrderDto> records = getOrders(importResult)
            ?? throw new ArgumentException(
                "Колекція імпортованих замовлень не може бути null.");

        IEnumerable<string> importErrors = getErrors(importResult)
            ?? throw new ArgumentException(
                "Колекція помилок імпорту не може бути null.");

        var orders = new List<Order>();
        var errors = new List<string>(importErrors);

        int recordNumber = 0;

        foreach (OrderDto dto in records)
        {
            recordNumber++;

            try
            {
                orders.Add(Order.FromDto(dto));
            }
            catch (ArgumentException ex)
            {
                errors.Add(
                    $"Запис {recordNumber}: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                errors.Add(
                    $"Запис {recordNumber}: {ex.Message}");
            }
        }

        return new OrderDomainImportResult(
            orders.AsReadOnly(),
            errors.AsReadOnly());
    }
}