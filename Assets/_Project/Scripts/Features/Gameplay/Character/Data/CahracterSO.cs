using UnityEngine;

[CreateAssetMenu(fileName = "New Character", menuName = "BlockJam3D/Character Data")]
public class CharacterSO : ScriptableObject
{
    [Header("Character Info")]
    public string characterName;
    public CharacterType characterType;
    
    [Header("Prefab")]
    public GameObject characterPrefab;
    
    [Header("Visual")]
    public Sprite characterIcon;
    public Color characterColor = Color.white;
}
