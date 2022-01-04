using UnityEditor.Callbacks;
using Core.Utilities.ComponentInfo;

namespace CoreEditor.Utilities.ComponentInfo
{
	public class ComponentInfoSceneUpdate
	{
		[PostProcessScene(0)]
		public static void OnPostprocessScene()
		{
			ComponentInfoHierarchy.UpdateSceneNodes();
		}
	}
}