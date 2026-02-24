using APIServer.DTO.Loans;
using APIServer.DTO;
using Microsoft.AspNetCore.OData.Query;
using APIServer.DTO.Book;

namespace APIServer.Service.Interfaces
{
    public interface IBookCopyService
    {
        PagedResult<BookCopyInfoListDto> GetFilteredBookCopies(ODataQueryOptions<BookCopyInfoListDto> options);

        Task<object?> CreateBookCopyFullAsync(BookCopyRequest request);

        Task<bool> UpdateCopyAndVariantAsync(int copyId, UpdateCopyAndVariantRequest request);
    }
}
