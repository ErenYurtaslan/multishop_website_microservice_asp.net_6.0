namespace MultiShop.DtoLayer.IdentityDtos.UserDtos
{
    public class AdminResetPasswordDto
    {
        public string UserId { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
