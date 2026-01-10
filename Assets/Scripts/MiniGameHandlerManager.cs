
using MiniGame.Kitchen;
using UnityEngine;
public class MiniGameHandlerManager
{
    public static void Init()
    {
        var go = new GameObject("KitchenNode");
        MonoBehaviour.DontDestroyOnLoad(go);
        Debug.Log("fzy entry project enter");
        Debug.Log("fzy add component type:" + typeof(KitchenEntry).AssemblyQualifiedName);
        go.AddComponent<KitchenEntry>();
        Debug.Log("fzy entry project finish");
    }
}
