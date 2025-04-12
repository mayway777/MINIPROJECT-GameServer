using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 가족오락관_서버
{
    internal class QuizDB
    {
        #region 싱글톤

        public static QuizDB Instance { get; private set; } = null;

        static QuizDB()
        {
            Instance = new QuizDB();
        }

        private QuizDB()
        {
        }

        #endregion

        private MySqlConnection conn = null;

        private const string servername = "127.0.0.1";// "127.0.0.1";  //localhost
        private const string dbname = "wb40";
        private const string userid = "root";
        private const string userpw = "1234";

        #region 데이터베이스 연결 및 종료
        public bool Open()
        {
            try
            {
                string constr = string.Format(@"Data Source={0};Initial Catalog={1};User ID={2};Password={3}", servername, dbname, userid, userpw);
                conn = new MySqlConnection(constr);
                conn.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool Close()
        {
            conn.Close();
            return false;
        }
        #endregion

        public string GetQuiz(int selectedNumber)
        {
            string query = string.Format($"SELECT * FROM quiztbl WHERE quizNum = {selectedNumber};");

            string temp = null;
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                //cmd.CommandType = System.Data.CommandType.Text;
                MySqlDataReader r = cmd.ExecuteReader();
                while (r.Read())
                {
                    int quizNum = int.Parse(r["quizNum"].ToString());
                    string quiz = r["quiz"].ToString();
                    string quizAnswer = r["quizAnswer"].ToString();
                    int quizScore = int.Parse(r["quizScore"].ToString());
                    string quizCategory = r["quizCategory"].ToString();

                    temp = quizNum + "#" + quiz + "#" + quizAnswer + "#" + quizScore + "#" + quizCategory;
                }
                r.Close();

            }
            return temp;
        }


        public bool IsAnswerOkay(int num, string Answer)
        {

            string query = string.Format($"select quiz from quiztbl where quizNum = {num} AND quizAnswer = '{Answer}';");

            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                string Num = (string)cmd.ExecuteScalar();
                if (Num == null)
                {

                    return false;
                }
                else
                {

                    return true;
                }
            }

        }

        public long QuizCount()
        {
            string query = string.Format("select count(*) from quiztbl;");

            long count = 0;
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                count = (long)cmd.ExecuteScalar();
            }
            return count;
        }

    }
}