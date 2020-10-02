using System.Threading.Tasks;

namespace TTL.CntLib
{
    public class CntLib 
    {
        public static void Initialize() { }
        public static async Task InitializeAsync()
        {
            await Task.Run(() => Initialize());
        }
    }
}
