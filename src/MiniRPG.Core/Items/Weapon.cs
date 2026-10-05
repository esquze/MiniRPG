using System;
using System.Collections.Generic;
using System.Text;

namespace MiniRPG.Core;

/*
 * Price 
*/

public class Weapon (string name, int damage, int price)
{
    public string Name { get; } = name;
    public int Damage { get; } = damage > 0 ? damage 
        : throw new ArgumentOutOfRangeException(nameof(damage), "Damage cannot be zero or negative");
    public int Price { get; private set; } = price >= 0 ? price
        : throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative");
    public string DisplayName => $"{Name} (+{Damage})";
}
