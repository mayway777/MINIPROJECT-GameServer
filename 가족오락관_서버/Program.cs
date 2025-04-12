using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace 가족오락관_서버
{
    internal class Program
    {
        private static QuizDB db = QuizDB.Instance;

        static void Main(string[] args)
        {

            //app.config에 정의함.
            Uri wsdl_uri = new Uri(ConfigurationManager.AppSettings["wsdl_uri"]);
            Uri nettcp_uri = new Uri(ConfigurationManager.AppSettings["nettcp_uri"]);

            ServiceHost host = new ServiceHost(typeof(QuizService));
            //host.AddServiceEndpoint(typeof(IQuizService), new NetTcpBinding(), nettcp_uri);
            host.Open();
            db.Open();




            Console.WriteLine("가족 오락관 서버 작동중...");
            Console.ReadLine();

            host.Close();
            db.Close();
        }
    }
}
