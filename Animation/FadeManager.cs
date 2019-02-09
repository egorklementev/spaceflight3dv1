using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeManager : MonoBehaviour {

    public Animator animator;
    public bool NeedToFade = false;
    public int NextSceneIndex = 0;

    private void Update()
    {
        animator.SetBool("NeedToFade", NeedToFade);
    }

    public void ToNextScene()
    {
        SceneManager.LoadScene(NextSceneIndex);
    }

}
