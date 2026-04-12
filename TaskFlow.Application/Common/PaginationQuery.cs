namespace TaskFlow.Application.Common;

/// <summary>
/// Validated pagination parameters. Can only be constructed through <see cref="Create"/>,
/// which guarantees the values are always within acceptable bounds.
/// </summary>
public sealed class PaginationQuery
{
    public const int MinPageNumber = 1;
    public const int MinPageSize   = 1;
    public const int MaxPageSize   = 50;

    public int PageNumber { get; }
    public int PageSize   { get; }

    private PaginationQuery(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize   = pageSize;
    }

    public static Result<PaginationQuery> Create(int pageNumber, int pageSize)
    {
        if (pageNumber < MinPageNumber)
            return Result<PaginationQuery>.Failure(
                Error.Create(Error.Codes.Validation,
                    $"Sayfa numarası en az {MinPageNumber} olmalıdır."));

        if (pageSize < MinPageSize || pageSize > MaxPageSize)
            return Result<PaginationQuery>.Failure(
                Error.Create(Error.Codes.Validation,
                    $"Sayfa boyutu {MinPageSize} ile {MaxPageSize} arasında olmalıdır."));

        return Result<PaginationQuery>.Success(new PaginationQuery(pageNumber, pageSize));
    }
}
