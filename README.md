# ComponentInfo Prototype
 
An implementation of the Data Prototype Model with a single source for Scriptable Objects. Allows for inheritance of Scriptable Objects like Prefabs.

https://user-images.githubusercontent.com/7192519/148136699-51755bdf-6586-4110-83c1-3e958563acbd.mp4

- Optionally override any serialized property.
- Entirely in editor, no standalone overhead.
- Support for components to also inherit and override from scriptable objects.
- Safety checks for circular/ self references.

#### Usage

See 'DebugComponentInfoAsset' & 'DebugComponent' for implementation.
Your data class needs to extend 'ComponentInfo'

```csharp
[System.Serializable]
public class DebugComponentInfo : ComponentInfo
{
	[SerializeField] private int intValue;
	[SerializeField] private Vector3 position;
	[SerializeField] private Texture assetReference;
}
```

And your Scriptable Object is a concrete implementation of ComponentInfoAsset.

```csharp
[CreateAssetMenu(fileName = "DebugComponentInfo", menuName = "Core/DebugComponentInfo")]
public class DebugComponentInfoAsset : ComponentInfoAsset<DebugComponentInfo> { }
```

#### Limitations

Untested with version control. Currently when modifying a prototype any assets inheriting from that asset would also be modified. This could cause a large number of files to be checked out. Would look into not dirtying these assets and only save when building the player.
