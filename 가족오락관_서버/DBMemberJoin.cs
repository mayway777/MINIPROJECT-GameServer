using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace 가족오락관_서버
{
    [DataContract]
    public class PlayerInfo
    {
        [DataMember]
        public int Index { get; set; }
        [DataMember]
        public int Pscore { get; set; }
        [DataMember]
        public string Nickname { get; set; }
        
       

        public PlayerInfo(int index, int pscore, string nickname)
        {
            Index = index;
            Nickname = nickname;
            Pscore = pscore;

        }
        public PlayerInfo(string name)
        {
            Nickname = name;
            Pscore = 0;
        }

    }

    public class DBMemberJoin
    {
        public PlayerInfo GetPlayerInfo(string name)
        {
            PlayerInfo playerInfo = new PlayerInfo(name);



            //using (SqlConnection connection = new SqlConnection())
            //{
            //    string query = "SELECT Photo, Nickname FROM Players WHERE PlayerId = @PlayerId";
            //    SqlCommand command = new SqlCommand(query, connection);
            //    command.Parameters.AddWithValue("@PlayerId", name);

            //    connection.Open();
            //    using (SqlDataReader reader = command.ExecuteReader())
            //    {
            //        if (reader.Read())
            //        {
            //            playerInfo.Index = playerNum;
            //            playerInfo.Photo = reader["Photo"] as byte[];
            //            playerInfo.Nickname = reader["Nickname"].ToString();

            //        }
            //    }
            //}

            return playerInfo;
        }
    }
}
