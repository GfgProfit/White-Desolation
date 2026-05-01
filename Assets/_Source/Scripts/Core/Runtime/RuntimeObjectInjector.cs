using UnityEngine;

public static class RuntimeObjectInjector
{
    public static void Inject(GameObject gameObject, bool includeInactive = true)
    {
        if (gameObject == null)
        {
            return;
        }

        SceneInstaller.Container?.InjectGameObject(gameObject, includeInactive);
    }
}
