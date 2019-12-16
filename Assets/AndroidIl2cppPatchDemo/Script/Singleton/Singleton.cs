#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T instance;
	private static object @lock = new object();

	public static T Instance
	{
		get
		{
			lock (@lock)
			{
				if (instance == null)
				{
					instance = (T) FindObjectOfType(typeof(T));
					
					if ( FindObjectsOfType(typeof(T)).Length > 1 )
					{
						Debug.LogError("[Singleton] Something went really wrong " +
						               " - there should never be more than 1 singleton!" +
						               " Reopenning the scene might fix it.");
						return instance;
					}
					
					if (instance == null)
					{
#if UNITY_EDITOR
						GameObject singleton =  EditorUtility.CreateGameObjectWithHideFlags("", HideFlags.DontSave);
#else
						GameObject singleton = new GameObject();
#endif
						instance = singleton.AddComponent<T>();
						singleton.name = "(singleton) "+ typeof(T).ToString();
						DontDestroyOnLoad(singleton);

					} 
				}
				return instance;
			}
		}
	}
}
