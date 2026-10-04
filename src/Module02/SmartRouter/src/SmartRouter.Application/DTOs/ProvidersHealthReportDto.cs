namespace SmartRouter.Application.DTOs;

public record ProvidersHealthReportDto(
    string OverallStatus,
    DateTimeOffset TimestampUtc,
    List<ProviderHealthItemDto> Providers);
