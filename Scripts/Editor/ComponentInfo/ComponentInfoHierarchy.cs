using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Core.Utilities.ComponentInfo;

namespace CoreEditor.Utilities.ComponentInfo
{
	[InitializeOnLoad]
	public static class ComponentInfoHierarchy
	{
		private class ComponentInfoNode
		{
			private readonly ComponentInfoPrototype _prototype;
			private readonly SerializedProperty _serializedProperty;

			private ComponentInfoNode _parent;

			private readonly HashSet<ComponentInfoNode> _children = new();

			public ComponentInfoNode(SerializedProperty serializedProperty, ComponentInfoPrototype prototype)
			{
				_prototype = prototype;
				_serializedProperty = serializedProperty;
				UpdateParent();
			}

			internal void PrintDebug(int indent)
			{
				var indentText = "";
				for (int i = 0; i < indent * 4; i++)
					indentText += "*";

				Debug.Log(indentText + " " + _prototype.GetHashCode() + " Node. Children -");
				indent++;
				foreach (var child in _children)
				{
					child.PrintDebug(indent);
				}
			}

			internal void UpdateParent()
			{
				if (_prototype.Prototype != null)
				{
					if (BaseNodes.TryGetValue(_prototype.Prototype, out var parentNode))
					{
						if (_parent == parentNode)
							return;
						_parent?.RemoveChild(this);

						_parent = parentNode;
						_parent.AddChild(this);

						BaseNodeHierarchy.Remove(this);
					}
					else
					{
						// We have a parent but it doesn't exist in the hierarchy yet. Add it to a list to be added later.
						if (WaitingForParentNodes.TryGetValue(_prototype.Prototype, out var childNodes))
						{
							childNodes.Add(this);
						}
						else
						{
							childNodes = new List<ComponentInfoNode>();
							childNodes.Add(this);
							WaitingForParentNodes[_prototype.Prototype] = childNodes;
						}
					}
				}
				else
				{
					_parent?.RemoveChild(this);
					_parent = null;
					BaseNodeHierarchy.Add(this);
				}
			}

			internal void UpdateChildrenSerialization()
			{
				foreach (var child in _children)
				{
					ComponentInfoPrototypeDrawer.UpdateProperty(child._serializedProperty);
					child.UpdateChildrenSerialization();
				}
			}

			private void AddChild(ComponentInfoNode child)
			{
				if (child == this) // Self reference.
					return;

				_children.Add(child);
			}

			private void RemoveChild(ComponentInfoNode child)
			{
				_children.Remove(child);
			}
		}

		private static readonly HashSet<ComponentInfoNode> BaseNodeHierarchy = new();
		private static readonly Dictionary<ComponentInfoPrototype, ComponentInfoNode> BaseNodes = new();

		private static readonly Dictionary<ComponentInfoPrototype, List<ComponentInfoNode>> WaitingForParentNodes =
			new();

		static ComponentInfoHierarchy()
		{
			BaseNodeHierarchy.Clear();
			BaseNodes.Clear();
			WaitingForParentNodes.Clear();

			ComponentInfoPrototype.OnUpdateHierarchy += UpdateHierarchy;
			ComponentInfoPrototype.UpdateChildrenProperties += UpdateChildrenProperties;
		}

		static void UpdateHierarchy(SerializedProperty serializedProperty, ComponentInfoPrototype prototype)
		{
			if (BaseNodes.TryGetValue(prototype, out var node))
			{
				node.UpdateParent();
			}
			else
			{
				BaseNodes[prototype] = new ComponentInfoNode(serializedProperty, prototype);
				ComponentInfoPrototypeDrawer.UpdateProperty(serializedProperty);

				if (WaitingForParentNodes.TryGetValue(prototype, out var waitingChildren))
				{
					foreach (var child in waitingChildren)
					{
						child.UpdateParent();
					}

					WaitingForParentNodes.Remove(prototype);
				}
			}
		}

		static void UpdateChildrenProperties(ComponentInfoPrototype prototype)
		{
			if (BaseNodes.TryGetValue(prototype, out var node))
			{
				node.UpdateChildrenSerialization();
			}
		}

		[MenuItem("Tools/Core/ComponentInfo/PrintNodes")]
		static void DebugNodes()
		{
			foreach (var node in BaseNodeHierarchy)
			{
				node.PrintDebug(0);
			}
		}

		[MenuItem("Tools/Core/ComponentInfo/RebuildNodes")]
		public static void RebuildNodes()
		{
#if DEBUG
			System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
			stopwatch.Start();
#endif

			BaseNodeHierarchy.Clear();
			BaseNodes.Clear();
			WaitingForParentNodes.Clear();

			foreach (string guid in AssetDatabase.FindAssets("t:ComponentInfoAsset"))
			{
				var componentInfoAsset =
					AssetDatabase.LoadAssetAtPath<ComponentInfoAsset>(AssetDatabase.GUIDToAssetPath(guid));
				componentInfoAsset.AddToHierarchy();
			}

			UpdateSceneNodes();

#if DEBUG
			stopwatch.Stop();
			var isExcessive = stopwatch.ElapsedMilliseconds > 0;

			if (Application.isPlaying || isExcessive)
			{
				Debug.Log(stopwatch.ElapsedMilliseconds);
			}
#endif
		}

		public static void UpdateSceneNodes()
		{
			foreach (var componentInfo in Object.FindObjectsOfType<MonoBehaviour>(true)
				.OfType<IComponentInfoFallback>())
			{
				componentInfo.AddToHierarchy();
			}
		}
	}
}