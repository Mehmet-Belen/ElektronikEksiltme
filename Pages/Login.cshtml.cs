using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

public class LoginModel : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Kullanıcı adı gereklidir")]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Şifre gereklidir")]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public bool RememberMe { get; set; }

    public string ErrorMessage { get; set; } = string.Empty;

    public void OnGet()
    {
        // Login sayfası yüklendiğinde
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Basit demo authentication
        if (ValidateUser(Username, Password))
        {
            // Başarılı giriş - ana sayfaya yönlendir
            // Gerçek uygulamada burada session/cookie oluşturulur
            TempData["LoginMessage"] = $"Hoş geldiniz, {Username}!";
            return RedirectToPage("/Index");
        }
        else
        {
            ErrorMessage = "Kullanıcı adı veya şifre hatalı!";
            return Page();
        }
    }

    private bool ValidateUser(string username, string password)
    {
        // Demo için basit kontrol
        // Gerçek uygulamada veritabanından kontrol edilir
        return (username == "admin" && password == "123456") ||
               (username == "user" && password == "password");
    }
}
