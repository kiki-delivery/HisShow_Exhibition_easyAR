using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using easyar;

/// <summary>
/// 성화의 행동을 결정함
/// </summary>

public class PlayManager : MonoBehaviour
{
    [Header("번들사용 할 때 체크")]
    public bool bundleMode;


    [Header("자기자신")]
    [Header("-----수동------")]
    public ImageTargetController imageTargetController;

    [Header("_pivot")]
    [SerializeField]
    GameObject _pivot;

    [Header("TimeLine")]
    public PlayableDirector director;

    [Header("ModelController")]
    public ModelController modelController; // 얘한테는 이미 내 정보가 다 넘어가있음

    [Header("포토존이면 체크")]
    public bool pho;

    [Header("MiddlePoint(포토존 전용)")]
    [SerializeField]
    GameObject middlePoint;

    [Header("Bgm")] // 이어보기로 재생해야 돼서 퍼블릭
    public BGMManager bgm;

    [Header("모델 번호")]
    [Header("-----자동------")]
    public string modelName;

    [Header("모델컨트롤러에서 나의 순서")]
    public int myNum;   // 모델 컨트롤러가 나를 구별하는 번호(모델번호X, 인덱스 번호)

    /// <summary>
    ///  한 번만 실행하도록 하는 용도
    /// </summary>
    bool activeGo, uiSetting;


    private void Awake()
    {
        _pivot.transform.localScale = new Vector3(0, 0, 0); // 혹시 처음에 1, 1, 1이면 눈 앞에 반짝 할테니
        if (!bundleMode)
        {
            _pivot.SetActive(false);  
        }

    }

    /// <summary>
    /// 문제 푸는 중에는 이 성화 비추면 반응 안하도록
    /// </summary>
    public bool go;

    private void OnEnable()
    {
        go = true;
        StartCoroutine(TargetCheck());
    }


    /// <summary>
    /// 퀴즈 풀 때, 성화의 BGM은 흐르고 다른 것들은 멈춰야해서 필요
    /// </summary>
    public void PivotBye()
    {
        _pivot.transform.localScale = new Vector3(0, 0, 0); // 끄기전에 크기 0,0,0
        director.Stop();
    }


    IEnumerator TargetCheck()
    {
        while (go)
        {
            if (!imageTargetController.IsTracked)  // 트래킹 안했으면
            {
                if (_pivot.activeSelf)    // 한 번만 실행하려고 넣은 if문
                {
                    _pivot.transform.localScale = new Vector3(0, 0, 0); // 끄기전에 크기 0,0,0
                    _pivot.SetActive(false);    // 끈다

                    if (pho) // 포토존인데
                    {
                        if (middlePoint)    // 미들포인트가 있으면(=> 미들포인트 = 버튼 = 캔버스로 화면 다 차지하게)
                        {
                            middlePoint.transform.localScale = new Vector3(0, 0, 0);    // 얘가 켜졌냐 안켜졌냐로 중간지점 왔냐 안왔냐를 판단해서 끄는건 하면 안됨
                        }
                    }

                    uiSetting = false;  // 트래킹 안하면서 UI 세팅을 다시 해야하므로 false
                }

                if (stop == false)
                {
                    Coroutine = true;   // 타임라인 3초뒤 끄기
                }

            }
            else if (imageTargetController.GetComponent<ImageTargetController>().IsTracked) // 트래킹 했으면
            {
                if (!_pivot.activeSelf) // 혹시 꺼져있냐
                {
                    yield return null;  // 이거 안하면 눈 앞에 보였다가 사진에 붙음, 지금은 소용 없는듯..
                    _pivot.SetActive(true); // 켜고나서
                    _pivot.transform.localScale = new Vector3(1, 1, 1);    // 크기 1, 1, 1로

                    if (pho)    // 혹시 포토존이냐
                    {
                        if (middlePoint)    // 미들 포인트 있으면
                        {
                            middlePoint.transform.localScale = new Vector3(1, 1, 1);    // 크기 1, 1, 1로
                        }
                    }
                }

                if (!activeGo)   // 나 말고 다른애들 다 끄는 작업 안했으면
                {
                    modelController.playName = modelName;   // 모델 컨트롤러에게 내가 누군지 알려주고(문제 출제용, 파일 이름이 1-02 이런식이니)
                    modelController.playNum = myNum;    // 모델 컨트롤러에게 내가 누군지 알려주고
                    modelController.ActiveSetting(myNum);    // 나 제외하고 다 꺼라
                    activeGo = true;    // 작업 했다
                }
                if (!uiSetting)
                {
                    if (bgm)
                    {
                        bgm.BgmPlay();
                    }

                    modelController.UISetting();
                    uiSetting = true;
                }
                
                if (director.state == PlayState.Paused) // 혹시 타임라인 멈춰있으면
                {
                    if (!pho)   // 포토존이 아닐 경우
                    {
                        director.Play();    // 영상 재생
                        stop = false;   
                    }
                    else
                    {
                        if (middlePoint)    // 포토존인데 미들포인트가 있으면
                        {
                            if (!middlePoint.activeInHierarchy) // 미들포인트가 꺼져있어야만(켜져있다 = 인터렉션 해야한다)
                            {
                                director.Play();    // 영상 재생
                                stop = false;
                            }
                        }
                        else
                        {
                            director.Play();    // 영상 재생
                            stop = false;
                        }
                    }
                }
                
                if (Coroutine)  // 혹시 3초뒤 정지할 예정이었으면
                {
                    Coroutine = false;  // 정지하려던거 멈추고
                    stop = false;       // 타임라인은 재생중이다
                }
            }

            yield return null;

        }
    }

    private void OnDisable()
    {
        _pivot.SetActive(false);
        uiSetting = false;
        activeGo = false;
    }

    /// <summary>
    /// 타임라인 끄도록 예약 했는지 확인
    /// </summary>
    bool coroutine;

    /// <summary>
    /// 타임라인 정지 여부 확인
    /// </summary>
    bool stop = true;

    /// <summary>
    ///  타임라인 끄도록 예약 했는지 확인
    /// </summary>
    public bool Coroutine
    {
        get
        {
            return coroutine;
        }
        set
        {
            coroutine = value;
            if (coroutine)
            {
                StartCoroutine("Stop");
            }
            else
            {
                StopCoroutine("Stop");
            }
        }

    }


    public void ContinueVideo() // 이어보기 누름
    {
        if (middlePoint)    // 미들 포인트 있으면
        {
            if (!middlePoint.activeInHierarchy) // 미들 포인트가 활성화 안됐나?
            {
                director.Play();    // 타임라인 재생
                bgm.BgmPlay();  // BGM 재생
            }
        }
        else
        {
            director.Play();    
            bgm.BgmPlay();
        }
        
    }



    IEnumerator Stop()
    {
        stop = true;        // 끄는 중이다
        yield return new WaitForSeconds(3);
        director.Pause();   // 눈에 안보일 때 멈추는거라 PAUSE해도 상관없는듯
        if (bgm)
        {
            bgm.BgmPause();
        }

        modelController.MarkMiss(); // 타임라인 멈췄으니 이어볼지 말지


    }

}
