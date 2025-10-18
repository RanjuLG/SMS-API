using SMS.Models.DTO;

namespace SMS.Interfaces
{
    public interface IPaginationService
    {
        Task<PaginatedResponse<T>> CreatePaginatedResponseAsync<T>(
            IQueryable<T> query, 
            PaginationRequest request, 
            object? appliedFilters = null);

        PaginationMetadata CreatePaginationMetadata(
            int currentPage, 
            int pageSize, 
            int totalItems);

        FilterMetadata CreateFilterMetadata(
            PaginationRequest request, 
            object? appliedFilters = null);
    }
}
