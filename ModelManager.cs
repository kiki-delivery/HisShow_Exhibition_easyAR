using easyar;
using System.Collections;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;   // 다운핸들 사용용


/// <summary>
/// 성화마다 붙어있는 매니저
/// </summary>
public class ModelManager : MonoBehaviour
{

    [Header("유아이 매니저")]
    [SerializeField]
    UIManager uiManager;

    [Header("이미지 트래커")]
    [SerializeField]
    ImageTrackerFrameFilter tracker;

    [SerializeField]
    ImageTargetController targetController;

    [Header("객체의 부모")]
    [SerializeField]
    Transform parnet;

    [Header("모델 컨트롤러")]
    [SerializeField]
    ModelController modelController;


    enum State  // 다운로드 상태 구분
    {
        start,
        noHaveModel,
        haveModel,
        modelGo
    }


    string modelName;   // 모델 이름 담는 변수

    /// <summary>
    /// 이 모델은 컨트롤러의 몇번 인덱스에 있는지
    /// </summary>
    [HideInInspector]
    public int myNum;   

    State state;

    GameObject model;    // 번들에서 객체화 한 애 저장용

    private void Awake()
    {
        modelName = gameObject.name.Substring(0, 4);     // ㅁ-ㅁㅁ 까지만 담음
        //StartCoroutine(DownloadModels());
    }


    int i = 0;
    bool downloading;

    private void OnEnable() 
    {
        if (i == 0) // 첫 시작에 무조건 켜져서 여기로 오도록
        {
            i++;
        }
        else if (downloading) // 다운로드를 이미 했다면
        {
            state = State.haveModel;    // 이미 번들 다운 끝났다
        }
        else // 안했다면
        {
            state = State.noHaveModel;  // 번들 다운 해야된다
        }

        switch (state)
        {
            case State.noHaveModel:
                StartCoroutine(DownloadModels());
                break;
            case State.haveModel:                
                ModelSetting();
                break;
            case State.modelGo:
                ModelGo();
                break;
        }
    }

    

    private void OnDisable()    // 트래킹을 멈췄는데
    {
        if(i==1)    // 첫시작에 켜지면서 i==1이 될테니..
        {
            return;
        }
        if(!model)  // 아직 다운로드가 다 안됐으면(서버에서 다운 받을 때만 사용됨)
        {
            uiManager.donwloadUI.SetActive(false);    // 로딩바 내리고
            uiManager.DownFailON();                     // 다운로드 실패알림 UI 띄우기
        }
    }

    AsyncOperationHandle handle;
    AsyncOperationHandle<GameObject> obj;

    /// <summary>
    /// 번들 다운로드하여 객체화(비동기)
    /// </summary>
    /// <returns></returns>
    IEnumerator DownloadModels()
    {
        
        uiManager.donwloadUI.SetActive(true);     // 다운로드 UI 켜기


        /*
        handle = Addressables.DownloadDependenciesAsync(modelName);

        while (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.Log("다운로드 는" + handle.PercentComplete);

            yield return null;
        }
        */

        obj = Addressables.InstantiateAsync(modelName, parnet);

        

        downloading = true; // 객체화는 진행중, 객체화가 완료됐을 때라는 개념이 없는듯..(=> obj.Status == AsyncOperationStatus.Succeeded가 완료 체크지만, 상태가 변해도 객체가 완성 안된 경우가 있음)

        while (!obj.IsDone)    // 객체화 중이면
        {
            //Debug.Log("객체화는   " + obj.PercentComplete);   

            yield return obj;
        }

        while(obj.Status == AsyncOperationStatus.Failed)
        {
            yield return null;
        }

        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            state = State.haveModel;    
            ModelSetting();
        }

        yield return null;

    }

    void ModelSetting() // model에 객체화 한 애 담는 용도
    {
        model = obj.Result;     // 위에서 객체화 한 애 담음, 이 때 result가 비어서 안 담기는 경우가 있음
        modelController.models[myNum] = model;
    }

    private void Update()
    {
        if (model)   // model에 객체화 한 애가 담겼으면
        {
            state = State.modelGo;
            ModelGo();
        }
        else // 안 담기는 경우 대비하여 모델이 비어있는 동안은 계속 실행
        {
            model = obj.Result;     // 위에서 객체화 한 애 담음
            modelController.models[myNum] = model;
        }
    }

    void ModelGo()
    {
        targetController.enabled = false; // 이제 얘는 더이상 트래킹 될 필요 없으니 끔(오직 객체화 용도)
        targetController.Tracker = null;  // 같은 그림에 트래커가 중복으로 작동할 수 있으니 빼기

        // 내가 객체화 한 애한테 정보 넘겨주기
        model.GetComponent<ImageTargetController>().Tracker = tracker;  // 트래커 넣기
        model.GetComponent<PlayManager>().modelController = modelController;    // 모델 컨트롤러 넣기
        model.GetComponent<PlayManager>().modelName = modelName;
        model.GetComponent<PlayManager>().myNum = myNum;

        
        modelController.ModelSetting(myNum);    // 모델 컨트롤러에게 정보 넘겨주기

        gameObject.SetActive(false);   // 이것만 하면 이미지 인식했을 때 얘가 또 켜져버림 
        uiManager.donwloadUI.SetActive(false);  // 다운로드 끝났으니 끄기
    }
}



