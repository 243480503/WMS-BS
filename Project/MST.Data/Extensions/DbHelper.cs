/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace MST.Data.Extensions
{
    public class DbHelper
    {
        private static string connstring = ConfigurationManager.ConnectionStrings["MSTDbContext"].ConnectionString;
        public static int ExecuteSqlCommand(string cmdText)
        {
            using (DbConnection conn = new SqlConnection(connstring))
            {
                DbCommand cmd = new SqlCommand();
                PrepareCommand(cmd, conn, null, CommandType.Text, cmdText, null);
                return cmd.ExecuteNonQuery();
            }
        }

        public static int ExecuteSqlCommand(string cmdText,int timeOut)
        {
            using (DbConnection conn = new SqlConnection(connstring))
            {
                DbCommand cmd = new SqlCommand();
                cmd.CommandTimeout = timeOut;
                PrepareCommand(cmd, conn, null, CommandType.Text, cmdText, null);
                return cmd.ExecuteNonQuery();
            }
        }

        private static void PrepareCommand(DbCommand cmd, DbConnection conn, DbTransaction isOpenTrans, CommandType cmdType, string cmdText, DbParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (isOpenTrans != null)
                cmd.Transaction = isOpenTrans;
            cmd.CommandType = cmdType;
            if (cmdParms != null)
            {
                cmd.Parameters.AddRange(cmdParms);
            }
        }
    }
}
