namespace BibliotecaApi.Auth
{
    public class AuthResponse
    {
        public string Message { get; set; } = string.Empty;

        // Depois vamos adicionar:
        // public string Token { get; set; }
        // public DateTime Expiration { get; set; }
    }
}
