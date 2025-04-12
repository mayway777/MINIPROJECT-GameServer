using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace 가족오락관_서버
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IQuizCallback))]
    public interface IQuizService
    {
        [OperationContract(IsOneWay = false, IsInitiating = true, IsTerminating = false)]
        bool JoinGame(string name);

        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void Say(string name, string msg);

        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void GameStart();
        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void GetQuiz();

        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
         void IsAnswer(string name, int num, string answer, int score);
        //[OperationContract]
        //void SubmitAnswer(string name, string answer);
    }

    public interface IQuizCallback
    {
        //[OperationContract]
        //void StartGame();

        //void GameStream(); 

        [OperationContract(IsOneWay = true)]
        void DisplayPlayerInfo(string playerName,string msg, int score, int index);

        [OperationContract(IsOneWay = true)]
        void Say_Ack(string playerName, string msg, int index);
       
        [OperationContract(IsOneWay = true)]
        void Start_Ack(string msg);

        [OperationContract(IsOneWay = true)]
        void GetQuiz_Ack(string msg);

        [OperationContract(IsOneWay = true)]
        void Quiz_Ack(string msg,int score,int num);
        [OperationContract(IsOneWay = true)]
        void IsAnswer_Ack(string ok, string msg, int index);
        [OperationContract(IsOneWay = true)]
        void Score_Ack(string name, int score);
        [OperationContract(IsOneWay = true)]
        void QuizImage_Ack(byte[] bytes, int score, int index);

        //[OperationContract]
        //void ReceiveAnswer(string question);

        //[OperationContract]
        //void UpdateScore(int playerNum, int score);

        //[OperationContract]
        //void NotifyGameEnd(string result);
    }
}
