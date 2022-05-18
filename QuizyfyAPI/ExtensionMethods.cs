using Microsoft.EntityFrameworkCore;
using QuizyfyAPI.Data;

namespace QuizyfyAPI
{
    public static class ExtensionMethods
    {
        public static int? ParseToNullableInt(this string text)
        {
            return int.TryParse(text, out var parsed) ? parsed : null;
        }

        public static IHost Migrate(this IHost webhost)
        {
            using (var scope = webhost.Services.GetService<IServiceScopeFactory>()?.CreateScope())
            {
                if(scope != null)
                {
                    using var dbContext = scope.ServiceProvider.GetRequiredService<QuizDbContext>();
                    dbContext.Database.Migrate();
                }
            }
            return webhost;
        }

        static System.Text.Encoding ISO_8859_1_ENCODING = System.Text.Encoding.GetEncoding("ISO-8859-1");
        public static (string, string) GetUsernameAndPasswordFromAuthorizeHeader(string authorizeHeader)
        {
            if (authorizeHeader == null || !authorizeHeader.Contains("Basic "))
                return (string.Empty, string.Empty);

            string encodedUsernamePassword = authorizeHeader.Substring("Basic ".Length).Trim();
            string usernamePassword = ISO_8859_1_ENCODING.GetString(Convert.FromBase64String(encodedUsernamePassword));

            string username = usernamePassword.Split(':')[0];
            string password = usernamePassword.Split(':')[1];

            return (username, password);
        }
    }
}
