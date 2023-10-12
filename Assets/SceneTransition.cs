using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] string progressVariableName = "transitionValue";
    [SerializeField] Material transitionMaterial;

    [SerializeField] bool isDebug = false;
    [SerializeField] float transitionDuration = 1f;
    [SerializeField] int sceneToLoadId;

    private void Update()
    {
        if(isDebug && Input.GetKeyDown(KeyCode.Space))
        {
            StartTransition();
        }
    }

    public void StartTransition()
    {
        StartCoroutine(TransitionCoroutine());
    }

    IEnumerator TransitionCoroutine()
    {
        Assert.AreNotEqual(SceneManager.GetActiveScene(), gameObject.scene, "MainScene should not set active");

        float transitionTimer = 0;

        while(transitionTimer < transitionDuration)
        {
            transitionMaterial.SetFloat(progressVariableName, 1 - transitionTimer / transitionDuration);
            transitionTimer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        SceneManager.LoadScene(sceneToLoadId, LoadSceneMode.Additive);

        transitionTimer = 0;

        while (transitionTimer < transitionDuration)
        {
            transitionMaterial.SetFloat(progressVariableName, transitionTimer / transitionDuration);
            transitionTimer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
}
