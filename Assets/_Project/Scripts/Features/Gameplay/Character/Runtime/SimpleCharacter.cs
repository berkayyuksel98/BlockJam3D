using System.Collections.Generic;
using UnityEngine;

public class SimpleCharacter : MonoBehaviour
{
    public List<Transform> colorTypeCharacters;
    public void Initialize(ColorType colorType)
    {
        colorTypeCharacters.ForEach(t => t.gameObject.SetActive(false));
        int index = (int)colorType;
        if (index >= 0 && index < colorTypeCharacters.Count)
        {
            colorTypeCharacters[index].gameObject.SetActive(true);
        }
    }
}
