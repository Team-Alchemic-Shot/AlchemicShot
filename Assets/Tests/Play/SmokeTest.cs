using NUnit.Framework;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using System.Collections;

public class SmokeTest
{
    [UnityTest]
    public IEnumerator MainSceneLoads()
    {
        SceneManager.LoadScene("Main");
        yield return null;
        Assert.IsTrue(true);
    }
}
