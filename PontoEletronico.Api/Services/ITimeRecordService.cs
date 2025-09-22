using PontoEletronico.DTOs;

namespace PontoEletronico.Services;

public interface ITimeRecordService
{
    Task<ApiResponseDto<TimeRecordResponseDto>> CreateTimeRecordAsync(string userId, TimeRecordCreateDto request);
    Task<ApiResponseDto<PagedResponseDto<TimeRecordListDto>>> GetUserTimeRecordsAsync(string userId, TimeRecordFilterDto filter);
    Task<ApiResponseDto<PagedResponseDto<TimeRecordSummaryDto>>> GetUserTimeRecordsSummaryAsync(string userId, TimeRecordFilterDto filter);
    Task<ApiResponseDto<TimeRecordResponseDto>> GetTimeRecordByIdAsync(int id, string userId);
    Task<ApiResponseDto<bool>> DeleteTimeRecordAsync(int id, string userId);
    Task<ApiResponseDto<List<TimeRecordListDto>>> GetTodayTimeRecordsAsync(string userId);
}