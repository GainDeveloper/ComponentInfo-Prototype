using Core.Utilities.ComponentInfo;
using UnityEngine;

public class DebugComponent : ComponentInfoBase<DebugComponentInfo>
{
	private void Update()
	{
		Debug.Log(Info.X);
	}
}