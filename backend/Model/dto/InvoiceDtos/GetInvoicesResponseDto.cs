namespace Backend.Model.dto.InvoiceDtos
{
    public class GetInvoicesResponseDto
    {
        public List<InvoiceListItemDto> Invoices { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}