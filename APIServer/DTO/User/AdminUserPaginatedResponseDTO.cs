namespace APIServer.DTO.User
{
    public class AdminUserPaginatedResponseDTO
    {
        public List<AdminUserResponseDTO> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
