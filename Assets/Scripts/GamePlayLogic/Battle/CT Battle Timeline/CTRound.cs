using System;
using System.Collections.Generic;

[Serializable]
public class CTRound
{
    public List<CharacterBase> cTTimelineQueue = new List<CharacterBase>();
    public int roundCount;

    public CTRound(List<CharacterBase> cTTimelineQueue, int roundCount)
    {
        this.cTTimelineQueue = cTTimelineQueue;
        this.roundCount = roundCount;
    }

    public CharacterBase GetCharacterAt(int index)
    {
        if (index < 0 || index >= cTTimelineQueue.Count) return null;
        return cTTimelineQueue[index];
    }

    public List<CharacterBase> GetCharacterQueue() { return cTTimelineQueue; }
}

