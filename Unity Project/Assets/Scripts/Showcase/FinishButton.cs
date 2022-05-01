using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FinishButton : MonoBehaviour //The script used to start the processing of the CA and controls the ui transition.
{
    [SerializeField] private NewCellGridController gridController;
    [SerializeField] private CAClickDrawer cAClickDrawer;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject leftPanel;
    [SerializeField] private Transform leftPanelGoal;

    public void onFinished() {
        gridController.setCAStepPaused(true);
        cAClickDrawer.setCanDraw(false);
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        StartCoroutine(transition());
        //gridController.processCA();
    }

    private IEnumerator transition() {
        float duration = 1f;
        float e_time = 0f;
        while (e_time < duration) {
            float fade = Mathf.Lerp(1,0,(e_time/duration));
            canvasGroup.alpha = fade;
            e_time+=Time.deltaTime;
            yield return null;
        }
        e_time = 0f;
        Vector3 position = leftPanel.transform.position; 
        Vector3 target = leftPanelGoal.position;
        while (e_time < duration) {
            Vector3 new_pos = Vector3.Lerp(position,target,(e_time/duration));
            leftPanel.transform.position = new_pos;
            e_time+=Time.deltaTime;
            yield return null;
        }
        leftPanel.transform.position = target;
        leftPanel.GetComponent<CanvasGroup>().interactable = true;
        gridController.processCA();
    }
}
