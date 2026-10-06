using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace MiniRPG.Core;

/* TODO:
 * Potions, rings, 
*/

public class Item(string name, int price)
{
    public string Name { get; } = name;
    public int Price { get; } = price >= 0 ? price
        : throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative");
}

// Weapons

public class Weapon(string name, int price, int damage) : Item(name, price)
{
    public int Damage { get; } = damage > 0 ? damage
        : throw new ArgumentOutOfRangeException(nameof(damage), "Damage cannot be zero or negative");
    public string DisplayName => $"{Name} (+{Damage} DMG)";
}

// Armor

public class Armor(string name, int price, int defense) : Item(name, price)
{
    public int Defense { get; } = defense > 0 ? defense
        : throw new ArgumentOutOfRangeException(nameof(defense), "Defense cannot be zero or negative");
    public string DisplayName => $"{Name} (+{Defense} ARM)";
}

// Poitions

public class Potion(string name, int price, int heal) : Item(name, price)
{
    public int Heal { get; } = heal > 0 ? heal
        : throw new ArgumentOutOfRangeException(nameof(heal), "Heal cannot be zero or negative");
    public string DisplayName => $"{Name} (+{Heal} HP)";
}