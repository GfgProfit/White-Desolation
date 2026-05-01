using UnityEngine;

public static class CrateSceneReferenceResolver
{
    public static T FindSceneObject<T>() where T : Object
    {
        return Object.FindFirstObjectByType<T>(FindObjectsInactive.Include);
    }

    public static GameObject FindSceneGameObject(string objectName)
    {
        if (string.IsNullOrWhiteSpace(objectName))
        {
            return null;
        }

        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();

        for (int i = 0; i < transforms.Length; i++)
        {
            Transform target = transforms[i];

            if (target == null || target.gameObject == null || !target.gameObject.scene.IsValid())
            {
                continue;
            }

            if (target.name == objectName)
            {
                return target.gameObject;
            }
        }

        return null;
    }

    public static Transform FindDeepChildByPath(Transform root, params string[] path)
    {
        if (root == null || path == null || path.Length == 0)
        {
            return null;
        }

        Transform current = root;

        for (int i = 0; i < path.Length; i++)
        {
            current = FindDirectChild(current, path[i]);

            if (current == null)
            {
                return null;
            }
        }

        return current;
    }

    public static Transform FindDeepChild(Transform root, string childName)
    {
        if (root == null || string.IsNullOrWhiteSpace(childName))
        {
            return null;
        }

        if (root.name == childName)
        {
            return root;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindDeepChild(root.GetChild(i), childName);

            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    public static T FindComponentInChildrenByName<T>(GameObject root, string objectName) where T : Component
    {
        if (root == null || string.IsNullOrWhiteSpace(objectName))
        {
            return null;
        }

        T[] components = root.GetComponentsInChildren<T>(true);

        for (int i = 0; i < components.Length; i++)
        {
            T component = components[i];

            if (component != null && component.gameObject.name == objectName)
            {
                return component;
            }
        }

        return null;
    }

    private static Transform FindDirectChild(Transform root, string childName)
    {
        if (root == null || string.IsNullOrWhiteSpace(childName))
        {
            return null;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);

            if (child != null && child.name == childName)
            {
                return child;
            }
        }

        return null;
    }
}
