using TMPro;
using UnityEngine;

public class FollowUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;      // 따라다닐 텍스트 UI
    [SerializeField] private Transform target;    // 따라갈 오브젝트의 Transform
    [SerializeField] private Vector3 offset = new Vector3(0, 3, 0); // 텍스트 위치 오프셋
    [SerializeField] private Camera mainCamera;   // 메인 카메라

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main; // 메인 카메라 할당
        }

        gameObject.SetActive(false);
    }

    public void SetTarget(Transform transform)
    {
        target = transform;
    }

    public void ShowText(string message)
    {
        gameObject.SetActive(true);
        _text.text = message;
    }

    private void Update()
    {
        if (target != null)
        {
            // 타겟 위치에 오프셋을 더하여 월드 좌표 계산
            Vector3 worldPosition = target.position + offset;

            // 월드 좌표를 화면 좌표로 변환
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

            // 텍스트 UI 위치를 화면 좌표에 맞춰 업데이트
            _text.transform.position = screenPosition;
        }
    }
}
