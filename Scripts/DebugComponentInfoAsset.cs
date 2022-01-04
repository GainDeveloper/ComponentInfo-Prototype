using Core.Utilities.ComponentInfo;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "DebugComponentInfo", menuName = "Core/DebugComponentInfo")]
public class DebugComponentInfoAsset : ComponentInfoAsset<DebugComponentInfo>
{
}

[System.Serializable]
public class DebugComponentInfo : ComponentInfo
{
	[SerializeField] private int intValue;
	public int IntValue => intValue;

	[SerializeField] private Vector3 position;
	[SerializeField] private Texture assetReference;
}