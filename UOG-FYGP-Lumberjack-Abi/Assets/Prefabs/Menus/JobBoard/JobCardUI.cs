using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JobCardUI : MonoBehaviour
{
    public TextMeshProUGUI customerNameText;
    public TextMeshProUGUI jobNameText;
    public Image customerFaceImage;
    public Button acceptButton;

    JobBoardUI board;
    int jobIndex;

    public void Bind(JobBoardUI owner, int index, JobRewardSO job, CustomerTypeSO customer)
    {
        board = owner;
        jobIndex = index;

        if (customerNameText) customerNameText.text = customer.displayName;
        if (jobNameText) jobNameText.text = job.displayName;
        if (customerFaceImage) customerFaceImage.sprite = customer.portrait;

        if (acceptButton != null)
        {
            acceptButton.onClick.RemoveAllListeners();
            acceptButton.onClick.AddListener(OnAcceptClicked);
        }
    }

    void OnAcceptClicked()
    {
        if (board != null)
            board.AcceptJob(jobIndex);
    }
}
