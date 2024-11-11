using TMPro;
using UnityEngine;

public class FollowUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;      // ����ٴ� �ؽ�Ʈ UI
    [SerializeField] private Transform target;    // ���� ������Ʈ�� Transform
    [SerializeField] private Vector3 offset = new Vector3(0, 3, 0); // �ؽ�Ʈ ��ġ ������
    [SerializeField] private Camera mainCamera;   // ���� ī�޶�

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main; // ���� ī�޶� �Ҵ�
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
            // Ÿ�� ��ġ�� �������� ���Ͽ� ���� ��ǥ ���
            Vector3 worldPosition = target.position + offset;

            // ���� ��ǥ�� ȭ�� ��ǥ�� ��ȯ
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

            // �ؽ�Ʈ UI ��ġ�� ȭ�� ��ǥ�� ���� ������Ʈ
            _text.transform.position = screenPosition;
        }
    }
}
