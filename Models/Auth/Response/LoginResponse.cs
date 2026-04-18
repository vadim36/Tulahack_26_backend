namespace backend.Models.Auth.Response
{
    public class LoginResponse
    {
        public string accessToken { get; set; }
        public string refreshToken { get; set; }
    }
}
