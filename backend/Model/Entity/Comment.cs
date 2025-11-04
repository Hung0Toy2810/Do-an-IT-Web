namespace Backend.Model.Entity
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CustomerId { get; set; }
        public long ProductId { get; set; }
        // navigation properties
        public virtual Customer? Customer { get; set; }
        public virtual Product? Product { get; set; }
    }
}