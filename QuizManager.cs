using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 퀴즈출제 담당 스크립트
/// </summary>



public class QuizManager : MonoBehaviour
{

    [Header("퀴즈")]
    [SerializeField]
    GameObject quiz;

    [Header("문제 내용")]
    [SerializeField]
    TextMeshProUGUI quizText;

    [Header("해설 내용")]
    [SerializeField]
    TextMeshProUGUI quizInfoText;

    [Header("정답 UI")]
    [SerializeField]
    GameObject goodUI;

    [Header("오답 UI")]
    [SerializeField]
    GameObject badUI;

    [Header("칭찬 UI")]
    [SerializeField]
    PartyUIManager partyUI;

    [Header("스탬프 매니저")]
    [SerializeField]
    StampManager stampManager;

    [Header("플레이 유아이")] // 스탬프창 넘어가면 꺼야하니까
    [SerializeField]
    GameObject playUI;


    /// <summary>
    /// 중복출제 방지용 리스트
    /// </summary>
    List<int> playQuizList = new List<int>();

    /// <summary>
    /// 맞춘 갯수 카운트
    /// </summary>
    int score;


    [Header("문제 몇개")]
    [SerializeField]
    int quizCount;

    int whatQuiz;

    int quizNum;

    bool quizAnswer;

    [Header("퀴즈 목록")]
    [SerializeField]
    public Quiz[] quizList;

    [System.Serializable]
    public struct Quiz
    {
        [SerializeField]
        [Header("누구의 문제인지(ㅁ-ㅁㅁ)")]
        public string who;
        [Header("문제")]
        public string[] quiz;
        [Header("정답(체크 = O, 안체크 = X)")]
        public bool[] ox;
        [Header("정답 해설")]
        public string[] yesInfo;
        [Header("오답 해설")]
        public string[] noInfo;
    }

    private void Awake()
    {
        for (int i = 0; i < quizList.Length; i++)
        {

            quizList[i].quiz = new string[quizCount];
            quizList[i].ox = new bool[quizCount];
            quizList[i].yesInfo = new string[quizCount];
            quizList[i].noInfo = new string[quizCount];
        }

        List<Dictionary<string, object>> quiz = CSVReader.Read("Quiz"); // Csv파일 읽어오기
        DataGo(quiz);
    }

    void DataGo(List<Dictionary<string, object>> quiz)
    {
        for (int i = 0; i < quizList.Length; i++)
        {
            for (int j = 0; j < quizCount; j++) // 8개 단위로 끊기
            {
                quizList[i].who = quiz[j + (i * quizCount)]["Who"].ToString();

                quizList[i].quiz[j] = quiz[j + (i * quizCount)]["Quiz"].ToString();

                if ("O" == quiz[j + (i * quizCount)]["Ox"].ToString())
                {
                    quizList[i].ox[j] = true;
                }
                else
                {
                    quizList[i].ox[j] = false;
                }

                quizList[i].yesInfo[j] = quiz[j + (i * quizCount)]["YesInfo"].ToString();
                quizList[i].noInfo[j] = quiz[j + (i * quizCount)]["NoInfo"].ToString();
            }
        }
    }

    void Init()
    {
        quizText.text = "";               // 문제 내용 지우기
        quizInfoText.text = "";           // 해설 내용 지우기
        score = 0;
        playQuizList = new List<int>();

    }


    /// <summary>
    /// 도장UI가 켜졌을 때, 구약을 풀다가 왔으면 구약, 신약을 풀다가 왔으면 신약이 켜지도록 하려고
    /// </summary>
    bool old;

    public void WhoQuiz(string playName)    // 누구의 문제를 내야 하는가
    {
        if ("1" == playName.Substring(0, 1))    // 1번 대면 구약
        {
            old = true;
        }
        else
        {
            old = false;
        }

        for (int i = 0; i < quizList.Length; i++)
        {
            if (quizList[i].who == playName) // 문제집 제목이랑 같으면
            {
                whatQuiz = i;   // i번째 문제집 출제해라
                Init();         // 이전의 퀴즈 관련된 정보 전부 초기화
                QuizStart(i);   // 퀴즈 시작
                break;
            }
        }
    }

    void QuizStart(int num) // 퀴즈 시작
    {
        int a = RandomNum();
        quizNum = a;    // 현재 내가 풀고 있는 퀴즈가 뭔지 저장
        quizText.text = quizList[num].quiz[quizNum];  // num번째 문제집의 quizNum번째 문제 나옴
        quizAnswer = quizList[num].ox[quizNum];       // num번째 문제집의 quizNum번째 문제 정답 저장
    }





    bool same = true;

    int playQuizNum;

    int RandomNum()    // 중복 제거
    {
        same = true;

        if (playQuizList.Count > 7) // 총 8문제니 중복 검사 리스트가 7개 넘었다 = 8개 다 출제 했다
        {
            playQuizList = new List<int>();
        }

        playQuizNum = Random.Range(0, quizCount);    // 랜덤으로 뽑아


        while (same)    // 같을 동안은 반복
        {
            if (playQuizList.Contains(playQuizNum))  // 리스트에 이미 i가있으면
            {
                playQuizNum = Random.Range(0, quizCount);    // 그 문제는 출제한거다
            }
            else   // 아니면
            {
                same = false;   // 같은거 아니니 반복 종료
            }
        }

        playQuizList.Add(playQuizNum);

        return playQuizNum;
    }


    public void SelectO()   // O, X 버튼
    {
        CheckAnswer(true);
    }

    public void SelectX()   // O, X 버튼
    {
        CheckAnswer(false);
    }

    void CheckAnswer(bool playerAnswer)  // 정답 체크
    {

        if (quizList[whatQuiz].ox[quizNum] == playerAnswer)    // 정답이면
        {
            goodUI.SetActive(true);
            quizInfoText.text = quizList[whatQuiz].noInfo[quizNum];    // 정답해설지
            score++;    // 정답 갯수 올림
        }
        else  // 오답
        {
            badUI.SetActive(true);
            quizInfoText.text = quizList[whatQuiz].noInfo[quizNum]; // 오답 해설지
        }
    }

    public void NextQuiz()    // 다음문제 버튼 눌렀을 때
    {
        if (score > 2)    // 3문제 맞췄으면
        {
            partyUI.old = old;  // 구약인지 신약인지 전해줌
            partyUI.gameObject.SetActive(true);
            playUI.SetActive(false);
            score = 0;
        }
        else  // 3문제 못 맞췄으면
        {
            quiz.SetActive(true);
            QuizStart(whatQuiz);
        }
    }


    public void StampGive()    // 축하 UI에 딸려올 버튼
    {
        stampManager.StampCheck(quizList[whatQuiz].who);
    }
}
