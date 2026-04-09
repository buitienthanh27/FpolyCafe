using System;

namespace FpolyCafe.Application.Modules.Customers.DTOs;

public record CustomerDto(
    int CustomerId,
    string FullName,
    string PhoneNumber,
    int RewardPoints,
    DateTime CreatedAt
);

public record CreateCustomerDto(
    string FullName,
    string PhoneNumber
);

public record UpdateCustomerDto(
    string FullName,
    string PhoneNumber
);
