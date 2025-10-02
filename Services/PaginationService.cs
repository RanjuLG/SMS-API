using Microsoft.EntityFrameworkCore;
using SMS.Interfaces;
using SMS.Models.DTO;
using System.Globalization;

namespace SMS.Services
{
    public class PaginationService : IPaginationService
    {
        public async Task<PaginatedResponse<T>> CreatePaginatedResponseAsync<T>(
            IQueryable<T> query, 
            PaginationRequest request, 
            object? appliedFilters = null)
        {
            var totalItems = await query.CountAsync();
            
            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var pagination = CreatePaginationMetadata(request.Page, request.PageSize, totalItems);
            var filters = CreateFilterMetadata(request, appliedFilters);

            return new PaginatedResponse<T>
            {
                Data = items,
                Pagination = pagination,
                Filters = filters
            };
        }

        public PaginationMetadata CreatePaginationMetadata(int currentPage, int pageSize, int totalItems)
        {
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            
            return new PaginationMetadata
            {
                CurrentPage = currentPage,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                HasNextPage = currentPage < totalPages,
                HasPreviousPage = currentPage > 1
            };
        }

        public FilterMetadata CreateFilterMetadata(PaginationRequest request, object? appliedFilters = null)
        {
            return new FilterMetadata
            {
                Search = request.Search,
                SortBy = request.SortBy,
                SortOrder = request.SortOrder,
                AppliedFilters = appliedFilters
            };
        }
    }
}
