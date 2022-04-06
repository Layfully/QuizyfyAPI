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
    }
}
