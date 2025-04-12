using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Xml.Linq;
using System.Management.Instrumentation;
using System.IO;
using Org.BouncyCastle.Utilities;

namespace 가족오락관_서버
{


    public delegate void JoinDele(string type, string nickname, string msg, int Score, int index, byte[] bytes);

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class QuizService : IQuizService
    {

        #region 문제 관련
        private string ImagePath = @"C:\Image\"; //사진주소
        private static List<int> numbers = new List<int>();
        #endregion


        private QuizDB db = QuizDB.Instance;
        private JoinDele MyJoin;
        private static object lockObj = new object(); // 동기화를 위한 락 객체
        IQuizCallback callback = null;

        private const int MaxPlayers = 4;
        private static int currentIndex = 0;

        DBMemberJoin dbmem = new DBMemberJoin();

        private static List<PlayerInfo> players = new List<PlayerInfo>();

        private static JoinDele List;

        static PlayerInfo playerInfo;

        #region 클라 -> 서버
        public bool JoinGame(string name)
        {
            try
            {
                playerInfo = dbmem.GetPlayerInfo(name);
                MyJoin = new JoinDele(UserHandler);

                // 고유 인덱스를 플레이어에 할당
                playerInfo.Index = currentIndex;

                // 콜백 채널을 설정
                callback = OperationContext.Current.GetCallbackChannel<IQuizCallback>();

                if (callback != null)
                {
                    // 델리게이트 리스트에 추가
                    lock (lockObj) // 동기화
                    {
                        List += MyJoin;
                    }

                    // 플레이어 정보 리스트에 추가
                    lock (lockObj) // 동기화
                    {
                        players.Add(playerInfo);
                    }
                    
                    // 접속자 정보 브로드캐스트
                    BroadcastMessage("Join[입장]", name, $"{name}님이 입장하셨습니다.", 000, playerInfo.Index,null);
                    currentIndex++; // 인덱스 증가
                   
                   


                    return true;
                }
                else
                {
                    Console.WriteLine("콜백 채널이 설정되지 않았습니다.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JoinGame 에러: {ex.Message}");
                return false;
            }
        }
        //11

        public void GameStart()
        {

            
            
                //BroadcastMessage("START", "게임", "께임을 시작하지", 0, 0, null);
                long quizcount = db.QuizCount();
                for (int i = 1; i <= quizcount; i++)
                {
                    numbers.Add(i);
                }
           
        }

     

        private static Dictionary<string, int> playerScores = new Dictionary<string, int>();
        public void GetQuiz()
        {
            try
            {
                Random random = new Random();

                int index = random.Next(numbers.Count);
                int selectedNumber = numbers[index];

                numbers.RemoveAt(index);
                string temp = QuizDB.Instance.GetQuiz(selectedNumber);

                string[] sp = temp.Split('#');
                int num = int.Parse(sp[0].ToString());
                string quiz = sp[1].ToString();
                int score = int.Parse(sp[3].ToString());
                string category = sp[4].ToString();

                if (category == "text")
                {
                    BroadcastMessage("Play[문제]", "", quiz, score, num, null);
                }
                else if (category == "image")
                {
                    byte[] bytes = GetPicture(quiz);
                    BroadcastMessage("Play[이미지]", "", quiz, score, num, bytes);
                }
            }
            catch
            {

            }
        }

        private byte[] GetPicture(string quiz)
        {
            byte[] bytePic = { 0 }; // 바이트 배열을 하나 만든다.
            try
            {
                // 해당 이미지 파일을 스트림 형식으로 오픈한다.
                FileStream picFileStream = new FileStream(ImagePath + quiz, FileMode.Open, FileAccess.Read, FileShare.Read);

                // 이미지 파일 스트림을 읽을 객체를 하나 만든다.
                BinaryReader picReader = new BinaryReader(picFileStream);
                // 이미지 파일을 바이트 배열에 넣는다.
                bytePic = picReader.ReadBytes(Convert.ToInt32(picFileStream.Length));
                // 파일스트림을 닫는다.
                picFileStream.Close();
                // 이미지 파일이 들어있는 바이트 배열을 리턴한다.
                return bytePic;
            }
            catch
            {
                // 초기값을 그냥 리턴한다.
                return bytePic;
            }
        }

        public void IsAnswer(string playerName, int num, string answer,int score)
        {
          
            if(QuizDB.Instance.IsAnswerOkay(num, answer) == false)
            {
                BroadcastMessage("PlayAnswer[채점]", playerName, answer, 0, playerInfo.Index, null);
               
            }
            else
            {
                BroadcastMessage("PlayAnswer[채점]", playerName, answer, 1, playerInfo.Index, null);

                lock (playerScores) // 동기화를 보장
                {
                    if (playerScores.ContainsKey(playerName))
                    {
                        Console.WriteLine($"[점수 업데이트] {playerName}: 기존 점수={playerScores[playerName]}, 추가 점수={score}");
                        playerScores[playerName] += score; // 기존 점수에 추가
                        BroadcastMessage("Score[점수]", playerName, "", playerScores[playerName], playerInfo.Index,null);
                    }
                    else
                    {
                        Console.WriteLine($"[새 플레이어 추가] {playerName}: 점수={score}");
                        playerScores[playerName] = score; // 새 플레이어 추가
                        BroadcastMessage("Score[점수]", playerName, "", playerScores[playerName], playerInfo.Index, null);
                    }

                    // 딕셔너리 상태 출력
                    Console.WriteLine("=== 현재 playerScores 상태 ===");
                    foreach (var kvp in playerScores)
                    {
                        Console.WriteLine($"플레이어: {kvp.Key}, 점수: {kvp.Value}");
                    }
                }

                // 점수 업데이트 후 메시지 전송
               

                GetQuiz();
            } 

        }



        public void Say(string name, string msg)
        {
            BroadcastMessage("Say[메시지]", name, msg, 0, playerInfo.Index, null);
        }
        #endregion

        // [비동기 호출] 서비스 -> 클라이언트
        private void BroadcastMessage(string msgType, string nickname, string msg, int score, int index, byte[] bytes)
        {
            if (List != null)
            {
                // 현재 등록된 모든 델리게이트에게 비동기 메시지를 전달
                foreach (JoinDele handler in List.GetInvocationList())
                {
                    try
                    {
                       
                        handler.BeginInvoke(msgType, nickname, msg, score, index, bytes, new AsyncCallback(EndAsync), null);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"BroadcastMessage 에러: {ex.Message}");
                    }
                }
            }
        }

        private void EndAsync(IAsyncResult ar)
        {
            JoinDele d = null;
            try
            {
                System.Runtime.Remoting.Messaging.AsyncResult asres = (System.Runtime.Remoting.Messaging.AsyncResult)ar;
                d = ((JoinDele)asres.AsyncDelegate);
                d.EndInvoke(ar); // 비동기 호출 종료
            }
            catch
            {
                // 예외 발생 시 델리게이트에서 제거
                MyJoin -= d;
            }
        }

        private void UserHandler(string type, string nickname, string msg, int score, int index, byte[] bytes)
        {
            try
            {
                // 클라이언트에게 메시지 보내기
                switch (type)
                {
                    case "Join[입장]":
                        lock (lockObj) // 동기화 처리
                        {
                            // 현재 접속 중인 클라이언트에게 기존 플레이어 정보 전송
                            foreach (var player in players)
                            {
                                // 현재 접속한 클라이언트의 콜백 채널에만 메시지 전송
                                if (callback != null)
                                {
                                    callback.DisplayPlayerInfo(
                                        player.Nickname,
                                        $"접속한플레이어{player.Nickname}",
                                        player.Pscore,
                                        player.Index
                                    );
                                }
                            }
                        }

                        // 현재 접속한 플레이어의 정보도 전송
                        if (callback != null)
                        {
                            callback.DisplayPlayerInfo(nickname, msg, score, index);
                        }
                        break;
                   
                    case "Say[메시지]": callback.Say_Ack(nickname, msg, index); break;
                    case "START": callback.Start_Ack(msg); break;
                    case "Play[문제]": callback.Quiz_Ack(msg,score, index); break;
                    case "Play[이미지]": callback.QuizImage_Ack(bytes, score, index); break;
                    case "PlayAnswer[채점]": callback.IsAnswer_Ack(nickname, msg, score); break;
                    case "Score[점수]":
                        lock (lockObj) // 동기화 처리
                        {
                            // 현재 접속 중인 클라이언트에게 기존 플레이어 정보 전송
                            foreach (var player in players)
                            {
                                // 현재 접속한 클라이언트의 콜백 채널에만 메시지 전송
                                if (callback != null)
                                {
                                    
                                    callback.Score_Ack(nickname, score);

                                }
                            }
                        }

                        
                       
                        break;




                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"에러 : {ex.Message}");
            }
        }
       
        private void GameStream()
        {
            // 게임 시작 관련 처리
            // 예: 게임 시작 시 모든 클라이언트에게 알림
            BroadcastMessage("GameStart", "게임", "게임이 시작되었습니다.", 0, 0, null);
        }
    }
}
