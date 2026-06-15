namespace MultiShop.WebUI.Services.Interfaces
{
    public class SignInResult
    {
        public bool IsSuccess { get; set; }
        public string Error { get; set; }

        public static SignInResult Success() => new SignInResult { IsSuccess = true };
        public static SignInResult Fail(string error) => new SignInResult { IsSuccess = false, Error = error };
    }
}
