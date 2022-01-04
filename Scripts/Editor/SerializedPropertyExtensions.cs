using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using Object = UnityEngine.Object;

namespace CoreEditor.Utilities
{
    public static class SerializedPropertyExtensions
    {
	    public static object GetParent(this SerializedProperty prop)
        	{
        		var path = prop.propertyPath.Replace(".Array.data[", "[");
        		object obj = prop.serializedObject.targetObject;
        		var elements = path.Split('.');
        		foreach(var element in elements.Take(elements.Length-1))
        		{
        			if(element.Contains("["))
        			{
        				var elementName = element.Substring(0, element.IndexOf("["));
        				var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[","").Replace("]",""));
        				obj = GetValue(obj, elementName, index);
        			}
        			else
        			{
        				obj = GetValue(obj, element);
        			}
        		}
        		return obj;
        	}
     
        private static object GetValue(object source, string name)
        {
	        if(source == null)
		        return null;
	        var f = GetField(source.GetType(), name);

	        if (f == null)
		        return null;
            
	        return f.GetValue(source);
        }

        private static FieldInfo GetField(Type type, string name)
        {
	        var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
	        if(f == null)
	        {
		        var baseType = type.BaseType;
		        if (baseType != null)
			        f = GetField(baseType, name);
	        }

	        return f;
        }
     
        private static object GetValue(object source, string name, int index)
        {
        	var enumerable = GetValue(source, name) as IEnumerable;
        	var enm = enumerable.GetEnumerator();
        	while(index-- >= 0)
        		enm.MoveNext();
        	return enm.Current;
        }
        
        public static void CopyPropertyTo(this SerializedProperty source, SerializedProperty target)
        {
            if (source.isArray)
            {
	            target.arraySize = source.arraySize;
	            for (int i = 0; i < target.arraySize; i++)
	            {
		            source.GetArrayElementAtIndex(i).CopyPropertyTo(target.GetArrayElementAtIndex(i));
	            }
            }
            else if (source.hasChildren)
            {
	            var baseLength = source.propertyPath.Length;
	            foreach (var sourceChild in source.GetChildren())
	            {
		            var targetChild = target.FindPropertyRelative(sourceChild.propertyPath.Substring(baseLength + 1));
		            sourceChild.CopyPropertyTo(targetChild);
	            }
            }
            else
            {
	            CopyPropertyValueTo(source, target);
            }
        }
        
        private static void CopyPropertyValueTo(SerializedProperty source, SerializedProperty target)
		{
			switch (source.propertyType)
			{
				case SerializedPropertyType.Generic:
					return;
				case SerializedPropertyType.Integer:
					target.intValue = source.intValue;
					break;
				case SerializedPropertyType.Boolean:
					target.boolValue = source.boolValue;
					break;
				case SerializedPropertyType.Float:
					target.floatValue = source.floatValue;
					break;
				case SerializedPropertyType.String:
					target.stringValue = source.stringValue;
					break;
				case SerializedPropertyType.Color:
					target.colorValue = source.colorValue;
					break;
				case SerializedPropertyType.ObjectReference:
					target.objectReferenceValue = source.objectReferenceValue;
					break;
				case SerializedPropertyType.LayerMask:
					target.intValue = source.intValue;
					break;
				case SerializedPropertyType.Enum:
					target.enumValueIndex = source.enumValueIndex;
					break;
				case SerializedPropertyType.Vector2:
					target.vector2Value = source.vector2Value;
					break;
				case SerializedPropertyType.Vector3:
					target.vector3Value = source.vector3Value;
					break;
				case SerializedPropertyType.Vector4:
					target.vector4Value = source.vector4Value;
					break;
				case SerializedPropertyType.Rect:
					target.rectValue = source.rectValue;
					break;
				case SerializedPropertyType.Character:
					target.stringValue = source.stringValue;
					break;
				case SerializedPropertyType.AnimationCurve:
					target.animationCurveValue = source.animationCurveValue;
					break;
				case SerializedPropertyType.Bounds:
					target.boundsValue = source.boundsValue;
					break;
				case SerializedPropertyType.Quaternion:
					target.quaternionValue = source.quaternionValue;
					break;
				case SerializedPropertyType.ExposedReference:
					target.exposedReferenceValue = source.exposedReferenceValue;
					break;
				case SerializedPropertyType.Vector2Int:
					target.vector2IntValue = source.vector2IntValue;
					break;
				case SerializedPropertyType.Vector3Int:
					target.vector3IntValue = source.vector3IntValue;
					break;
				case SerializedPropertyType.RectInt:
					target.rectIntValue = source.rectIntValue;
					break;
				case SerializedPropertyType.BoundsInt:
					target.boundsIntValue = source.boundsIntValue;
					break;
			}
		}

        /// <summary>
        /// Gets all children of `SerializedProperty` at 1 level depth.
        /// </summary>
        /// <param name="serializedProperty">Parent `SerializedProperty`.</param>
        /// <returns>Collection of `SerializedProperty` children.</returns>
        public static IEnumerable<SerializedProperty> GetChildren(this SerializedProperty serializedProperty)
        {
	        var currentProperty = serializedProperty.Copy();
	        var nextSiblingProperty = serializedProperty.Copy();
	        {
		        nextSiblingProperty.Next(false);
	        }
     
	        if (currentProperty.Next(true))
	        {
		        do
		        {
			        if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty))
				        break;
     
			        yield return currentProperty;
		        }
		        while (currentProperty.Next(false));
	        }
        }
     
        /// <summary>
        /// Gets visible children of `SerializedProperty` at 1 level depth.
        /// </summary>
        /// <param name="serializedProperty">Parent `SerializedProperty`.</param>
        /// <returns>Collection of `SerializedProperty` children.</returns>
        public static IEnumerable<SerializedProperty> GetVisibleChildren(this SerializedProperty serializedProperty)
        {
	        SerializedProperty currentProperty = serializedProperty.Copy();
	        SerializedProperty nextSiblingProperty = serializedProperty.Copy();
	        {
		        nextSiblingProperty.NextVisible(false);
	        }
     
	        if (currentProperty.NextVisible(true))
	        {
		        do
		        {
			        if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty))
				        break;
     
			        yield return currentProperty;
		        }
		        while (currentProperty.NextVisible(false));
	        }
        }
    }
}