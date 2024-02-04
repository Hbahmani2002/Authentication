using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace AngularAuthApi.Helpers
{
    public class DecodeSqlPath
    {
        public static string DecodeSql(string path)
        {
            
            if (string.IsNullOrWhiteSpace(path)) { return null; }
            for (int i = 0; i < 3; i++)
            {
                path = Encoding.UTF8.GetString(Convert.FromBase64String(path));

            }
            return path;
        }


    }
}
