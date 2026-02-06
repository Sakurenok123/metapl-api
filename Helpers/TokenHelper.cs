// TokenHelper.cs (упрощенный)
namespace MetaPlApi.Helpers
{
    public static class TokenHelper
    {
        // Заглушка - возвращает пустую строку вместо JWT токена
        public static string GenerateJwtToken(int userId, string role, IConfiguration configuration)
        {
            // Возвращаем пустую строку или простой идентификатор
            return $"user-{userId}-{DateTime.UtcNow.Ticks}";
        }
    }
}
