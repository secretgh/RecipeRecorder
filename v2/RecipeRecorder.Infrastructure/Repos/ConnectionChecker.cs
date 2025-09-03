using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeRecorder.Infrastructure.Repos
{
    public class ConnectionChecker
    {
        public static async Task<bool> IsDBUp(RecipeDbContext context)
        {
            return await context.Database.CanConnectAsync();
        }
    }
}
