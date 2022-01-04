using UnityEngine;

namespace Core.Utilities.ComponentInfo
{
	public class ComponentInfoBase<T> : MonoBehaviour, IComponentInfoFallback where T : ComponentInfo
	{
		[SerializeField] private ComponentInfoFallback<T> fallbackInfo;
		protected T Info => fallbackInfo.Info;

		protected virtual void OnValidate()
		{
			AddToHierarchy();
		}

		public void AddToHierarchy()
		{
			fallbackInfo.AddToHierarchy(this, "fallbackInfo");
		}
	}

	public interface IComponentInfoFallback
	{
		public void AddToHierarchy();
	}
}