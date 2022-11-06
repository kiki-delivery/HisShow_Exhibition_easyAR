using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using easyar;
using UnityEngine.Playables;
using VideoRecording;


/// <summary>
/// 성화 전체를 컨트롤하는 스크립트
/// </summary>

public class ModelController : MonoBehaviour
{
    [Header("유아이 매니저")]
    [SerializeField]
    UIManager uiManager;

    [Header("번들사용 할 때 체크")]
    [SerializeField]
    bool bundleMode;

    [Header("recoding Manager")]
    [SerializeField]
    RecordingManager recordingManager;

    /// <summary>
    /// 녹화종료가 아닌 다른 버튼을 눌러서 녹화가 종료되는 경우 버튼모양을 바꾸기위해 참조 
    /// </summary>
    [SerializeField]
    GameObject recordStartBtn;

    [SerializeField]
    GameObject recordEndBtn;


    [Header("사용할 모델들")]
    [Header("----------수동(번들 사용시 채우기 X)----------")]
    public GameObject[] models;


    [Header("번들 사용시 수동으로 채워야함")]
    [Header("----------자동----------")]
    [SerializeField]
    ModelManager[] modelManager;
    [SerializeField]
    ImageTargetController[] imageTargetController;
    [SerializeField]
    PlayableDirector[] director;

    /// <summary>
    /// 현재 플레이 중인 성화
    /// </summary>
    public int playNum;

    /// <summary>
    /// 내가 직접 꺼버린 모델번호 저장용(직접 꺼버렸는데 비추고 있다고 바로 재생되면 안되니)
    /// </summary>
    int endNum;


    /// <summary>
    /// 퀴즈 문제집 이름이랑 매칭용
    /// </summary>
    public string playName;


    private void Awake()
    {
        if (bundleMode)
        {
            for (int i = 0; i < modelManager.Length; i++)   // 각 타겟들의 인덱스 알려주기
            {
                modelManager[i].myNum = i;
            }
        }
        else // 번들 안 쓸때는.. 모든 정보 수동으로 저장..
        {
            modelManager = new ModelManager[models.Length];
            imageTargetController = new ImageTargetController[models.Length];
            director = new PlayableDirector[models.Length];

            for (int i = 0; i < models.Length; i++)
            {
                imageTargetController[i] = models[i].GetComponent<PlayManager>().imageTargetController;
                director[i] = models[i].GetComponent<PlayManager>().director;
                models[i].GetComponent<PlayManager>().modelName = models[i].name.Substring(0, 4);
                models[i].GetComponent<PlayManager>().myNum = i;
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        if (bundleMode)
        {
            models = new GameObject[modelManager.Length];
            imageTargetController = new ImageTargetController[modelManager.Length];
            director = new PlayableDirector[modelManager.Length];
        }

    }

    // 타임라인 재생
    public void TimeLineGo()
    {
        if (director[playNum])
        {
            if (director[playNum].gameObject.activeInHierarchy)
            {
                director[playNum].playableGraph.GetRootPlayable(0).SetSpeed(1);
            }
        }
    }

    public void TimeLineStop()  // = 퍼즈 개념
    {
        if (director[playNum])
        {
            if (director[playNum].gameObject.activeInHierarchy)
            {
                director[playNum].playableGraph.GetRootPlayable(0).SetSpeed(0); // pause쓰면 동작이 처음으로 돌아간 상태로 멈춤
            }
        }
    }

    private void Update()
    {
        StartCoroutine(ActiveCheck());
    }

    bool bgmPlay = true;

    /// <summary>
    /// 이미지 인식을 한 성화가 있는지 체크하는 용도
    /// </summary>
    /// <returns></returns>
    IEnumerator ActiveCheck()
    {
        for (int i = 0; i < modelManager.Length; i++)
        {
            if (models[i] != null && imageTargetController[i] != null)
            {
                if (imageTargetController[i].IsTracked) // 인식 했을 때
                {

                    if (uiManager.markerMissUI.activeSelf)  // 이어보기 UI가 켜져있으면
                    {

                        uiManager.markerMissUI.SetActive(false);    // 꺼라
                    }

                    if (!models[i].activeSelf)  // 인식한 모델이 꺼져있는데
                    {
                        if (uiManager.quizUI.activeSelf || uiManager.stampUI.activeSelf) // 퀴즈 풀고 있거나 스탬프 UI 활성화 상태면
                        {
                            yield return null;  // 멈춰! (개념으로 썻지만 그냥 뒤까지 실행..)
                        }
                        else if (i != endNum)    // 내가 직접 꺼버린 모델이 아니면
                        {
                            models[i].SetActive(true);  // 켜라
                            end = false;  // 직전에 직접 끈 모델이 있으면, 다시 가도 3초 기다릴 필요 없도록
                        }
                        else  // 만약 직접 꺼버린 모델이면
                        {
                            if (!end)   // 끝내기 누르고 3초가 지나기전에는 켜지마라
                            {
                                models[i].SetActive(true);
                            }
                        }

                    }
                }
            }
        }


        yield return null;
    }


    // 이 아래로는 다른 함수에서 호출하는 용도

    /// <summary>
    /// 생성된 모델의 데이터 받기
    /// </summary>
    /// <param name="num"></param>
    public void ModelSetting(int num)
    {
        imageTargetController[num] = models[num].GetComponent<PlayManager>().imageTargetController;
        director[num] = models[num].GetComponent<PlayManager>().director;
    }

    public void ActiveSetting(int num)   // 인식중인 애 외에는 전부 끄기, 한 번만 함
    {

        for (int i = 0; i < modelManager.Length; i++)
        {
            if (models[i] != null)
            {
                if (i != num && models[i].activeSelf)
                {
                    models[i].SetActive(false);
                }
            }
        }

        if (uiManager.videoEndUI.activeSelf)    // 혹시 비디오 다 봤는데 아무것도 안하고 다른애 비춘거면
        {
            uiManager.videoEndUI.SetActive(false);  // 끄자
        }
    }


    public void UISetting() 
    {
        uiManager.notice.SetActive(false);  // 화면 비추라는 알림 끄기

    }


    public void MarkMiss()  // 마커 잃었을 때 작동
    {
        uiManager.notice.SetActive(true);   // 화면 비추라는 알림 켜기

        if (!uiManager.markerMissUI.activeSelf)
        {
            uiManager.markerMissUI.SetActive(true);
        }

        if (uiManager.videoEndUI.activeSelf) // 비디오 앤드 유아이 떠 있으면 뜰 필요 없으니 
        {
            uiManager.markerMissUI.SetActive(false);
        }
    }

    public void VideoComplete() // 비디오 다 봤으면
    {
        uiManager.videoEndUI.SetActive(true);

        if (uiManager.markerMissUI.activeSelf)
        {
            uiManager.markerMissUI.SetActive(false);
        }
    }

    public void RecordingEnd()
    {
        if (recordingManager.recON)
        {
            recordingManager.RecordingEnd();
            recordStartBtn.SetActive(true);
            recordEndBtn.SetActive(false);
        }
    }

    public void StampWant() // 스탬프 받기 누르면
    {
        uiManager.StampQuiz(playName); // 퀴즈 시작        

        StopCoroutine("EndVideo");
        end = true;
        if (models[playNum] != null)
        {
            models[playNum].GetComponent<PlayManager>().go = false;
            models[playNum].GetComponent<PlayManager>().PivotBye();
        }
        endNum = playNum;
        StartCoroutine("EndVideo");
    }

    public void ContinueVideo() // 화면 안 비추고 있는데 비디오 계속 볼거면
    {
        models[playNum].GetComponent<PlayManager>().ContinueVideo();
    }

    public void EndVideoBtn()   // 보던거 멈출거면
    {
        StopCoroutine("EndVideo");
        end = true;
        if (models[playNum] != null)
        {
            models[playNum].GetComponent<PlayManager>().bgm.BgmStop();
            models[playNum].SetActive(false);
        }
        endNum = playNum;
        StartCoroutine("EndVideo");
    }


    bool end = false;


    IEnumerator EndVideo()
    {
        yield return new WaitForSeconds(3);
        end = false;
    }
}
