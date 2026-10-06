using System;
using System.Collections.Generic;
using System.Text;

namespace MiniRPG.Core;

public class Trader(string name, int money)
{
    public string Name { get; } = name;
    public int Money { get; } = money > 0 ? money
       : throw new ArgumentOutOfRangeException(nameof(money), "Money cannot be zero or negative");
    private readonly List<Item> _items = new List<Item>();
    public IReadOnlyList<Item> Item { get { return _items; } }

    public void Buy(Character character, Item item)
    {
        if (_items.Contains(item) && _items.Remove(item) == true)
        {
            character.BuyItem(item);
        }
    }

    public void Sell(Character character, Item item)
    {
        character.SellItem(item);
    }

    
}
