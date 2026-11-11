namespace DireitoFacil.Application.DTOs;

/// <summary>
/// JWT emitido no login. O app envia no header: Authorization: Bearer {AccessToken}.
/// </summary>
public record TokenResponse(string AccessToken, DateTime ExpiraEm, UsuarioResponse Usuario)
{
    public string TokenType => "Bearer";
}
