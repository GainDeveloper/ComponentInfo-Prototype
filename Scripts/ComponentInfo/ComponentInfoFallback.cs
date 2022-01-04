using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Core.Utilities.ComponentInfo
{
	[System.Serializable]
	public class ComponentInfoFallback<T> : ComponentInfoFallback where T : ComponentInfo
	{
		private T _overrideInfo;
		public T Info => _overrideInfo ?? fallbackInfo.Info;

		[SerializeField] private ComponentInfoPrototype<T> fallbackInfo;

		public void AddToHierarchy(Component component, string propertyName)
		{
#if UNITY_EDITOR
			var so = new SerializedObject(component);

			if (component.gameObject.scene.isLoaded)
				fallbackInfo.UpdateHierarchy(so.FindProperty(propertyName + ".fallbackInfo"));
#endif
		}
	}

	public abstract class ComponentInfoFallback
	{
	}
}