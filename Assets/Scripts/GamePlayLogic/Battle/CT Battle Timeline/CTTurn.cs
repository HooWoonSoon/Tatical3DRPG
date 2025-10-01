using System;
using System.Collections.Generic;

[Serializable]
public class CTTurn
{
    public List<CharacterBase> cTTimelineQueue = new List<CharacterBase>();
    public int turnCount = 0;

    public CTTurn(List<CharacterBase> cTTimelineQueue, int turnCount)
    {
        this.cTTimelineQueue = cTTimelineQueue;
        this.turnCount = turnCount;
    }

    public CharacterBase GetCharacterAt(int index)
    {
        if (index < 0 || index >= cTTimelineQueue.Count) return null;
        return cTTimelineQueue[index];
    }

    public List<CharacterBase> GetCharacterQueue() { return cTTimelineQueue; }
}

