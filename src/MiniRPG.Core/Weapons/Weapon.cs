using System;
using System.Collections.Generic;
using System.Text;

namespace MiniRPG.Core;

/*
 * Price 
*/

public class Weapon (string name, int damage)
{
    public string Name { get; } = name;
    public int Damage { get; } = damage > 0 ? damage 
        : throw new ArgumentOutOfRangeException(nameof(damage), "Damage cannot be zero or negative");
    public string DisplayName => $"{Name} (+{Damage})";
}
