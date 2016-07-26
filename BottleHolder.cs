using UnityEngine;
using System.Collections;
using VRTK;

public class BottleHolder : MonoBehaviour {
    public GameObject leftController;
    public GameObject rightController;
    private VRTK_ControllerEvents leftEvents;
    private VRTK_ControllerEvents rightEvents;
    // Use this for initialization
    void Start () {
        leftEvents = leftController.GetComponent<VRTK_ControllerEvents>();
        rightEvents = rightController.GetComponent<VRTK_ControllerEvents>();
        leftEvents.AliasReshapeOn += new VRTK.ControllerInteractionEventHandler(AttemptReshape);
        rightEvents.AliasReshapeOn += new VRTK.ControllerInteractionEventHandler(AttemptReshape);
        //leftEvents.TriggerPressed += new VRTK.ControllerInteractionEventHandler(Testing); 
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void AttemptReshape(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        GameObject currentController = ((VRTK_ControllerEvents)sender).transform.gameObject;
        Debug.Log("The Reshape Button has been pressed");
        for (int i=0; i<transform.childCount; i++)
        {
            Transform currentBottle = transform.GetChild(i);
            BottleBehavior currentBottleBehavior = currentBottle.GetComponent<BottleBehavior>();
            currentBottleBehavior.AttemptInteraction(currentController);
        }
    }

    //void EndReshape(object sender, VRTK.ControllerInteractionEventArgs e)
    //{
    //    GameObject currentController = ((VRTK_ControllerEvents)sender).transform.gameObject;
    //    for (int i = 0; i < transform.childCount; i++)
    //    {
    //        Transform currentBottle = transform.GetChild(i);
    //        currentBottle.EndInteraction(currentController); 
    //    }
    //}
}
