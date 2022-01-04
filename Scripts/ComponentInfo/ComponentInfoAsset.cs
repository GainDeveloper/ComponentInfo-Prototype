using UnityEngine;
#if UNITY_EDITOR
	using UnityEditor;
#endif

namespace Core.Utilities.ComponentInfo
{
	public class ComponentInfoAsset<T> : ComponentInfoAsset where T : ComponentInfo
	{
		[SerializeField] private ComponentInfoPrototype<T> prototype;
		public override ComponentInfoPrototype Prototype => prototype;

#if UNITY_EDITOR
		private SerializedProperty _prototypeProperty;
#endif

		private void OnEnable()
		{
			AddToHierarchy();
		}

		public override void AddToHierarchy()
		{
#if UNITY_EDITOR
			var so = new SerializedObject(this);
			_prototypeProperty = so.FindProperty("prototype");
			prototype.UpdateHierarchy(_prototypeProperty);
#endif
		}
	}

	public abstract class ComponentInfoAsset : ScriptableObject
	{
		public abstract void AddToHierarchy();

		public abstract ComponentInfoPrototype Prototype { get; }
	}
}