﻿namespace APIServer.DTO.Common
{
    public class PaginationRequestDTO
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
