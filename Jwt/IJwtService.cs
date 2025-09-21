namespace JobSearcher.Jwt
{
    public interface IJwtService
    {
        public string GenerateToken(int userId);
        public int? ValidateToken(string token);
    }
}