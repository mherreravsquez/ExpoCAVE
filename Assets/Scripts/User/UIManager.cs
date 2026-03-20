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
    }

    public void TogglePanel(GameObject panel)
    {
        if (panelCount == 0)
        {
            panel.transform.DOMoveX(-644, 1);
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
