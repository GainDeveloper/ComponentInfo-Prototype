using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Core.Utilities.ComponentInfo
{
	public abstract class ComponentInfoPrototype
	{
		public abstract ComponentInfoPrototype Prototype { get; }
		public abstract ComponentInfoAsset PrototypeAsset { get; }


#if UNITY_EDITOR
		public static Action<SerializedProperty, ComponentInfoPrototype> OnUpdateHierarchy;
		public static Action<ComponentInfoPrototype> UpdateChildrenProperties;
#endif
	}

	[Serializable]
	public class ComponentInfoPrototype<T> : ComponentInfoPrototype where T : ComponentInfo
	{
		[SerializeField] private ComponentInfoAsset<T> prototype;

		public override ComponentInfoPrototype Prototype => prototype != null ? prototype.Prototype : null;
		public override ComponentInfoAsset PrototypeAsset => prototype;


		[SerializeField, HideInInspector] private string[] modifiedProperties;
		[SerializeField] private T defaults;
		public T Info => defaults;

#if UNITY_EDITOR
		internal void UpdateHierarchy(SerializedProperty serializedProperty)
		{
			OnUpdateHierarchy?.Invoke(serializedProperty, this);
		}
#endif
	}
}