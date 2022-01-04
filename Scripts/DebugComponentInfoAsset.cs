using Core.Utilities.ComponentInfo;
using UnityEngine;

[CreateAssetMenu(fileName = "DebugComponentInfo", menuName = "Core/DebugComponentInfo")]
public class DebugComponentInfoAsset : ComponentInfoAsset<DebugComponentInfo>
{

}

[System.Serializable]
public class DebugComponentInfo : ComponentInfo
{
	[SerializeField] private int x;
	public int X => x;
}