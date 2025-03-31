using System;
using System.Collections.Generic;

public class Team
{
    public List<UnitCharacter> members;

    public Team()
    {
        members = new List<UnitCharacter>();
    }

    public void AddMember(UnitCharacter unit)
    {
        members.Add(unit);
        unit.team = this;
    }

    public void RemoveMember(UnitCharacter unit)
    {
        members.Remove(unit);
    }

    public void ResetMember()
    {
        members.Clear();
    }
}