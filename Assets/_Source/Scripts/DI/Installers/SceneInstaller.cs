using UnityEngine;

[DefaultExecutionOrder(-1000)]
public abstract class SceneInstaller : MonoBehaviour
{
    [Header("Auto-inject scene on Awake")]
    [SerializeField] private bool InjectOnAwake = true;

    public static IContainer Container { get; private set; }

    protected abstract void Install(IContainer container);

    private void Awake()
    {
        Container = new DiContainer();
        Install(Container);

        if (InjectOnAwake)
        {
            Inject();
        }
    }

    private void Inject()
    {
        GameObject[] roots = gameObject.scene.GetRootGameObjects();

        for (int i = 0; i < roots.Length; i++)
        {
            Container.InjectGameObject(roots[i], true);
        }
    }

    private void OnDestroy()
    {
        Container?.Dispose();
        Container = null;
    }
}