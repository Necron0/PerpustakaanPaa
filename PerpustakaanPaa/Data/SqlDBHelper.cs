using Npgsql;

namespace PerpustakaanPaa.Data
{
    public class SqlDBHelper
    {
        private NpgsqlConnection _connection;

        public SqlDBHelper(string connStr)
        {
            _connection = new NpgsqlConnection(connStr);
        }

        public NpgsqlCommand GetCommand(string query)
        {
            _connection.Open();
            var cmd = new NpgsqlCommand(query, _connection);
            cmd.CommandType = System.Data.CommandType.Text;
            return cmd;
        }

        public void Close()
        {
            if (_connection?.State == System.Data.ConnectionState.Open)
                _connection.Close();
        }
    }
}