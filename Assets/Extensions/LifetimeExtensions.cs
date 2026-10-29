using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Assets.Extensions
{
#if UNITY_EDITOR
    public static class SOHelpers
    {
        public static T CreateSOInstance<T>(string Path = "Assets/Database/", string fileName = "Data") where T : ScriptableObject
        {
            var pathName = $"{Path}{fileName}.asset";
            T SOInstance;
            if (AssetDatabase.AssetPathExists(pathName))
            {
                SOInstance = AssetDatabase.LoadAssetAtPath(pathName, typeof(T)) as T;
            }
            else
            {
                SOInstance = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(SOInstance, pathName);
            }
            AssetDatabase.SaveAssetIfDirty(SOInstance);
            if (Application.isPlaying)
                AssetDatabase.SaveAssets();
            return SOInstance;
        }

    }
#endif

    public static class MonoBehaviourExtensions
    {
        public static IEnumerator StopTimeUntilDisabledRoutine(GameObject interruptor)
        {
            while (interruptor.activeSelf)
            {
                Time.timeScale = 0f;
                yield return null;
            }
            Time.timeScale = 1f;
        }

    }

    public static class TranformExtensions
    {
        public static void ToggleColliderIfPresent<T>(this Transform transform, bool toggle) where T : Collider
        {
            if (transform.gameObject.TryGetComponent<T>(out var mainCapsule))
            {
                mainCapsule.enabled = toggle;
            }
        }

        public static bool TryGetComponentInParent<T>(this Transform transform, out T component)
        {
            component = transform.GetComponentInParent<T>();
            return component != null;
        }
    }

    public static class LayoutGroupHelper
    {
        public static void SetAmountOfChildren(this Transform layoutGroupTransform, int amount, bool destroySurplus = false)
        {
            while (layoutGroupTransform.childCount < amount)
            {
                Object.Instantiate(layoutGroupTransform.GetChild(0).gameObject, layoutGroupTransform);
            }
            for (int i = 0; i < layoutGroupTransform.childCount; i++)
            {
                if(i < amount)
                    layoutGroupTransform.GetChild(i).gameObject.SetActive(true);
                else if (destroySurplus)
                {
#if UNITY_EDITOR
                    if(Application.isEditor)
                    {
                        var objectToDestroy = layoutGroupTransform.GetChild(i);
                        UnityEditor.EditorApplication.delayCall += () =>
                        {
                            if (objectToDestroy)
                            {
                                Object.DestroyImmediate(objectToDestroy.gameObject);
                            }
                            i--;
                        };
                    }
                    else
#endif
                        Object.Destroy(layoutGroupTransform.GetChild(i).gameObject);

                }
                else
                    layoutGroupTransform.GetChild(i).gameObject.SetActive(false);
            }
        }

        public static void SetActiveChild(this Transform parentTransform, int index)
        {
            for (int i = 0; i < parentTransform.childCount; i++)
                parentTransform.GetChild(i).gameObject.SetActive(i == index);
        }
    }
}