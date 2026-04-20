namespace PerpustakaanPaa.Models
{
    public class ApiResponse
    {
        public static object Success(object data, int statusCode = 200)
            => new { status = statusCode, data };

        public static object SuccessList(object data, int total)
            => new { status = 200, meta = new { total }, data };

        public static object Error(string message, int statusCode = 400)
            => new { status = statusCode, message };
    }
}