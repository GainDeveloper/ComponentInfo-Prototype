using UnityEditor;
using UnityEngine;
using Core.Utilities.ComponentInfo;

namespace CoreEditor.Utilities.ComponentInfo
{
	[CustomPropertyDrawer(typeof(ComponentInfoPrototype), useForChildren: true)]
	public class ComponentInfoPrototypeDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var prototypeSp = property.FindPropertyRelative("prototype");
			var modifiedPropertiesSp = property.FindPropertyRelative("modifiedProperties");
			var defaultsSp = property.FindPropertyRelative("defaults");

			var height = EditorGUI.GetPropertyHeight(prototypeSp);

			position.height = height;
			EditorGUI.BeginChangeCheck();
			var originalPrototype = prototypeSp.objectReferenceValue;
			EditorGUI.PropertyField(position, prototypeSp, new GUIContent("Parent"));
			if (EditorGUI.EndChangeCheck())
			{
				if (IsValidParent(prototypeSp))
				{
					property.serializedObject.ApplyModifiedProperties();
					var valueObject = fieldInfo.GetValue(property.GetParent()) as ComponentInfoPrototype;
					ComponentInfoPrototype.OnUpdateHierarchy?.Invoke(property, valueObject);
				}
				else
					prototypeSp.objectReferenceValue = originalPrototype;
			}

			position.y += height + 8;

			foreach (var childSp in defaultsSp.GetVisibleChildren())
			{
				DrawProperty(property, childSp, modifiedPropertiesSp, prototypeSp, ref position);
			}

			if (property.serializedObject.hasModifiedProperties)
			{
				property.serializedObject.ApplyModifiedProperties();
				var valueObject = fieldInfo.GetValue(property.GetParent()) as ComponentInfoPrototype;
				ComponentInfoPrototype.UpdateChildrenProperties?.Invoke(valueObject);
			}
		}

		private static bool IsValidParent(SerializedProperty prototypeSp)
		{
			var self = prototypeSp.serializedObject.targetObject as ComponentInfoAsset;
			if (
				self == null) //This is a component referencing. It's always safe since it can't be self referenced or circular.
				return true;

			var currentParent = prototypeSp.objectReferenceValue as ComponentInfoAsset;

			if (currentParent == self)
			{
				Debug.LogError("Self-reference found. Aborting.");
				return false;
			}

			while (currentParent != null)
			{
				currentParent = currentParent.Prototype.PrototypeAsset;

				if (currentParent == self)
				{
					Debug.LogError("Circular reference found. Aborting.");
					return false;
				}
			}

			return true;
		}

		private static void DrawProperty(SerializedProperty baseProperty, SerializedProperty serializedProperty,
			SerializedProperty modifiedPropertiesSp,
			SerializedProperty prototypeSp, ref Rect position)
		{
			var height = EditorGUI.GetPropertyHeight(serializedProperty, includeChildren: serializedProperty.isArray);

			position.height = height;
			var toggleRect = new Rect(position.x, position.y, 16 * (EditorGUI.indentLevel + 1), position.height);
			var propertyRect = new Rect(position.x + 32, position.y, position.width - 32, position.height);
			position.y += height;

			var isPropertyModified = IsPropertyModified(modifiedPropertiesSp, serializedProperty);
			var prototypeProperty = PrototypeProperty(prototypeSp, baseProperty, serializedProperty);

			EditorGUI.BeginDisabledGroup(prototypeProperty == null);
			isPropertyModified = EditorGUI.Toggle(toggleRect, isPropertyModified || prototypeProperty == null);
			if (prototypeProperty != null)
				SetPropertyModified(modifiedPropertiesSp, serializedProperty, isPropertyModified);
			EditorGUI.EndDisabledGroup();

			if (!isPropertyModified)
				prototypeProperty.CopyPropertyTo(serializedProperty);

			EditorGUI.BeginDisabledGroup(!isPropertyModified);
			EditorGUI.PropertyField(propertyRect, serializedProperty);

			if (serializedProperty.hasChildren && !serializedProperty.isArray && serializedProperty.isExpanded)
			{
				EditorGUI.indentLevel++;
				foreach (var childSp in serializedProperty.GetVisibleChildren())
				{
					DrawProperty(baseProperty, childSp, modifiedPropertiesSp, prototypeSp, ref position);
				}

				EditorGUI.indentLevel--;
			}

			EditorGUI.EndDisabledGroup();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var height = 0f;

			var prototypeSp = property.FindPropertyRelative("prototype");
			var defaultsSp = property.FindPropertyRelative("defaults");

			height += EditorGUI.GetPropertyHeight(prototypeSp) + 8;

			foreach (var childSp in defaultsSp.GetVisibleChildren())
				height += EditorGUI.GetPropertyHeight(childSp);

			return height;
		}

		// Prototype

		private static SerializedProperty PrototypeProperty(SerializedProperty prototype,
			SerializedProperty baseProperty, SerializedProperty property)
		{
			if (prototype.objectReferenceValue == null)
				return null;

			var baseLength = baseProperty.propertyPath.Length;
			var relativePropertyPath = property.propertyPath.Substring(baseLength + 1);

			return new SerializedObject(prototype.objectReferenceValue).FindProperty(
				"prototype." + relativePropertyPath);
		}

		internal static void UpdateProperty(SerializedProperty baseProperty)
		{
			if (baseProperty.serializedObject.targetObject == null)
				return;

			baseProperty.serializedObject.UpdateIfRequiredOrScript();

			var prototypeSp = baseProperty.FindPropertyRelative("prototype");
			var modifiedPropertiesSp = baseProperty.FindPropertyRelative("modifiedProperties");
			var defaultsSp = baseProperty.FindPropertyRelative("defaults");

			foreach (var childSp in defaultsSp.GetChildren())
			{
				if (childSp == null)
					continue;

				UpdateLocalProperty(childSp);
			}

			void UpdateLocalProperty(SerializedProperty serializedProperty)
			{
				var isPropertyModified = IsPropertyModified(modifiedPropertiesSp, serializedProperty);
				var prototypeProperty = PrototypeProperty(prototypeSp, baseProperty, serializedProperty);

				EditorGUI.BeginDisabledGroup(prototypeProperty == null);
				isPropertyModified = isPropertyModified || prototypeProperty == null;
				if (prototypeProperty != null)
					SetPropertyModified(modifiedPropertiesSp, serializedProperty, isPropertyModified);
				EditorGUI.EndDisabledGroup();

				if (!isPropertyModified)
					prototypeProperty.CopyPropertyTo(serializedProperty);

				foreach (var childSp in serializedProperty.GetChildren())
				{
					UpdateLocalProperty(childSp);
				}
			}

			baseProperty.serializedObject.ApplyModifiedProperties();
		}
		
		// Modified Properties

		private static int PropertyModifiedIndex(SerializedProperty modifiedProperties, SerializedProperty property)
		{
			for (int i = 0; i < modifiedProperties.arraySize; i++)
			{
				if (modifiedProperties.GetArrayElementAtIndex(i).stringValue == property.propertyPath)
					return i;
			}

			return -1;
		}

		private static bool IsPropertyModified(SerializedProperty modifiedProperties, SerializedProperty property)
		{
			return PropertyModifiedIndex(modifiedProperties, property) >= 0;
		}

		private static void SetPropertyModified(SerializedProperty modifiedProperties, SerializedProperty property,
			bool modified)
		{
			int i = PropertyModifiedIndex(modifiedProperties, property);
			if (modified)
			{
				if (i == -1)
				{
					modifiedProperties.InsertArrayElementAtIndex(0);
					modifiedProperties.GetArrayElementAtIndex(0).stringValue = property.propertyPath;
				}
			}
			else if (i >= 0)
			{
				modifiedProperties.DeleteArrayElementAtIndex(i);
			}
		}
	}
}