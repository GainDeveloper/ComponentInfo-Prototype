using UnityEditor;
using UnityEngine;
using Core.Utilities.ComponentInfo;

namespace CoreEditor.Utilities.ComponentInfo
{
	[CustomPropertyDrawer(typeof(ComponentInfoFallback), useForChildren: true)]
	public class ComponentInfoFallbackDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var fallbackInfo = property.FindPropertyRelative("fallbackInfo");
			EditorGUI.PropertyField(position, fallbackInfo);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var fallbackInfo = property.FindPropertyRelative("fallbackInfo");
			return EditorGUI.GetPropertyHeight(fallbackInfo);
		}
	}
}