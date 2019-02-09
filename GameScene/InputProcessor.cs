using UnityEngine;

public class InputProcessor : MonoBehaviour {

    public GemManager gemManager;

    void Update()
    {
        #region GemSelection
        if (!gemManager.areGemsActing && !gemManager.isBonusActing)
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hitInfo = new RaycastHit();
                bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
                if (hit)
                {
                    if (hitInfo.transform.gameObject.tag == "Gem")
                    {
                        gemManager.GemSelectionFirst(hitInfo);
                    }
                }
            }
            else if (Input.GetMouseButton(0))
            {
                RaycastHit hitInfo = new RaycastHit();
                bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
                if (hit)
                {
                    if (hitInfo.transform.gameObject.tag == "Gem")
                    {
                        gemManager.GemSelectionSecond(hitInfo);
                    }
                }
            }
            if (Input.GetMouseButtonUp(0))
            {
                gemManager.GemSelectionReset();
            }
        }
        #endregion

        //////////////////////
        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hitInfo = new RaycastHit();
            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
            if (hit)
            {
                gemManager.GemSelectionDebug(hitInfo);
            }
        }
        //////////////////////
    }
}
