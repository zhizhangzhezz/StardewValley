using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIInventoryTextBox : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMeshTop1 = null;
    [SerializeField] private TextMeshProUGUI textMeshTop2 = null;
    [SerializeField] private TextMeshProUGUI textMeshTop3 = null;
    [SerializeField] private TextMeshProUGUI textMeshBottom1 = null;
    [SerializeField] private TextMeshProUGUI textMeshBottom2 = null;
    [SerializeField] private TextMeshProUGUI textMeshBottom3 = null;

    public void SetTextBoxText(string top1, string top2, string top3, string bottom1, string bottom2, string bottom3)
    {
        textMeshTop1.text = top1;
        textMeshTop2.text = top2;
        textMeshTop3.text = top3;
        textMeshBottom1.text = bottom1;
        textMeshBottom2.text = bottom2;
        textMeshBottom3.text = bottom3;
    }
}
