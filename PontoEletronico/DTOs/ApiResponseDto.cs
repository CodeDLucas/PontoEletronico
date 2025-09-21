namespace PontoEletronico.DTOs;

public class ApiResponseDto<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ApiResponseDto<T> SuccessResult(T data, string message = "Operação realizada com sucesso")
    {
        return new ApiResponseDto<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponseDto<T> ErrorResult(string message, List<string>? errors = null)
    {
        return new ApiResponseDto<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }

    public static ApiResponseDto<T> ErrorResult(List<string> errors)
    {
        return new ApiResponseDto<T>
        {
            Success = false,
            Message = "Erro na validação dos dados",
            Errors = errors
        };
    }
}

public class PagedResponseDto<T>
{
    public List<T> Data { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }

    public PagedResponseDto(List<T> data, int totalCount, int page, int pageSize)
    {
        Data = data;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        HasNextPage = page < TotalPages;
        HasPreviousPage = page > 1;
    }
}