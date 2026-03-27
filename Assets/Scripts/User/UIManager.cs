using UnityEngine;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;

    [Header("Tutorial")]
    [Space(15)]
    public GameObject panel;
    public GameObject arrow;
    int panelCount;

    [SerializeField] private float posX;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        panel.transform.DOMoveX(35, 1);
        arrow.transform.DORotate(new Vector3(0, 0, 0), 1);

        posX = 530;
    }

    public void TogglePanel(GameObject panel)
    {
        if (panelCount == 0)
        {
            posX = 530;
            panel.transform.DOMoveX(-posX, 1);
            arrow.transform.DORotate(new Vector3(0, 0, 180), 1);
            panelCount = 1;
        }
        else
        {
            panel.transform.DOMoveX(35, 1);
            arrow.transform.DORotate(new Vector3(0, 0, 0), 1);
            panelCount--;
        }
        
    }
}
