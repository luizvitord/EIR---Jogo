using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Dialogue {
    public string name;
    [TextArea(5, 10)]
    public string text;
    
    // NOVO: Adicionamos um campo para a imagem do personagem!
    public Sprite portrait; 
}

[CreateAssetMenu(fileName = "DialogueData", menuName = "ScriptableObject/TalkScript", order = 1)]
public class DialogueData : ScriptableObject {
    public List<Dialogue> talkScript;
}