namespace backend.Model.dto.Comment
{
    public class CreateCommentDto
    {
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; } // 1-5
        public long ProductId { get; set; }
    }
    public class UpdateCommentDto
    {
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; }
    }
    public class CommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CustomerName { get; set; } = "Ẩn danh";
    }

    public class GetCommentsRequest
    {
        public int? LastCommentId { get; set; }
        public int PageSize { get; set; } = 5;
    }
}