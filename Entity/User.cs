namespace Entities.Models.MainEngine
{
    public class User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string UserName { get; set; } = "";
        public string? Password { get; set; }
        public string? ImageUrl { get; set; }
        public string? Salt { get; set; }
        public string? RefreshToken { get; set; }
        public string? UserAgent { get; set; }
        public string? IP { get; set; }
    }
}
