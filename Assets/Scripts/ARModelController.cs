using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ARModelController : MonoBehaviour
{
    public static ARModelController instance;

    public GameObject startModelPrefab;
    public GameObject redModelPrefab;
    public GameObject blueModelPrefab;
    public GameObject greenModelPrefab;

    public Transform cameraSocket;
    public Transform targetSocket;

    public Button resetButton;
    public Button finishButton;
    public GameObject colorButtonPanel;

    private GameObject currentModel;
    private bool isTracking = false;
    private HashSet<string> clickedColors = new HashSet<string>();

    // 脱卡参数
    // CameraSocket 有 Euler(-90,0,0) 旋转 → local(x,y,z) → world(x,z,-y)
    // localY 控制前后距离，localZ 控制上下
    private float lostForward = -2f;   // localY → world Z 前方 2 单位
    private float lostDown = -0.2f;    // localZ → world Y，略微偏下
    private float lostScale = 45f;     // 脱卡后模型放大倍数

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        resetButton.onClick.AddListener(ResetModel);
        finishButton.onClick.AddListener(() =>
            SceneManager.LoadScene("6_EndingStory"));

        ShowColorButtons(false);
        resetButton.gameObject.SetActive(false);
        finishButton.gameObject.SetActive(false);
    }

    public void OnTargetFound()
    {
        isTracking = true;

        if (currentModel != null)
        {
            OnTargetReFound();
            return;
        }

        ClearModels();
        ShowColorButtons(true);
        resetButton.gameObject.SetActive(false);

        currentModel = Instantiate(startModelPrefab, targetSocket);
        currentModel.transform.localPosition = Vector3.zero;
        currentModel.transform.localRotation = Quaternion.identity;
        currentModel.transform.localScale = Vector3.one;

        var control = currentModel.GetComponent<ModelTouchControl>();
        if (control != null)
            control.enableControl = false;
    }

    public void OnTargetLost()
    {
        if (currentModel == null) return;

        isTracking = false;
        ShowColorButtons(false);

        currentModel.transform.SetParent(cameraSocket, false);
        currentModel.transform.localPosition = new Vector3(0, lostForward, lostDown);
        currentModel.transform.localRotation = Quaternion.Euler(-90, 180, 0);
        currentModel.transform.localScale = Vector3.one * lostScale;

        var control = currentModel.GetComponent<ModelTouchControl>();
        if (control == null)
            control = currentModel.AddComponent<ModelTouchControl>();
        control.enableControl = true;

        resetButton.gameObject.SetActive(true);
    }

    public void OnTargetReFound()
    {
        if (currentModel == null) return;

        isTracking = true;

        currentModel.transform.SetParent(targetSocket, false);
        currentModel.transform.localPosition = Vector3.zero;
        currentModel.transform.localRotation = Quaternion.identity;
        currentModel.transform.localScale = Vector3.one;

        ShowColorButtons(true);
        resetButton.gameObject.SetActive(false);

        var control = currentModel.GetComponent<ModelTouchControl>();
        if (control != null)
            control.enableControl = false;
    }

    public void ShowRedModel() => SwitchModel(redModelPrefab, "Red");
    public void ShowBlueModel() => SwitchModel(blueModelPrefab, "Blue");
    public void ShowGreenModel() => SwitchModel(greenModelPrefab, "Green");

    private void SwitchModel(GameObject prefab, string color)
    {
        if (prefab == null) return;

        ClearModels();

        currentModel = Instantiate(
            prefab,
            isTracking ? targetSocket : cameraSocket
        );

        if (isTracking)
        {
            currentModel.transform.localPosition = Vector3.zero;
            currentModel.transform.localRotation = Quaternion.identity;
            currentModel.transform.localScale = Vector3.one;

            var control = currentModel.GetComponent<ModelTouchControl>();
            if (control != null)
                control.enableControl = false;
        }
        else
        {
            currentModel.transform.localPosition = new Vector3(0, lostForward, lostDown);
            currentModel.transform.localRotation = Quaternion.Euler(-90, 180, 0);
            currentModel.transform.localScale = Vector3.one * lostScale;

            var control = currentModel.GetComponent<ModelTouchControl>();
            if (control == null)
                control = currentModel.AddComponent<ModelTouchControl>();
            control.enableControl = true;
        }

        clickedColors.Add(color);

        if (clickedColors.Count >= 3)
            finishButton.gameObject.SetActive(true);
    }

    public void ResetModel()
    {
        if (currentModel == null) return;

        currentModel.transform.localPosition = new Vector3(0, lostForward, lostDown);
        currentModel.transform.localRotation = Quaternion.Euler(-90, 180, 0);
        currentModel.transform.localScale = Vector3.one * lostScale;
    }

    public void ShowColorButtons(bool show)
    {
        if (colorButtonPanel != null)
            colorButtonPanel.SetActive(show);
    }

    private void ClearModels()
    {
        foreach (Transform t in targetSocket)
            Destroy(t.gameObject);

        foreach (Transform t in cameraSocket)
            Destroy(t.gameObject);
    }
}
