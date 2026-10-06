using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace MiniRPG.Core;

/* TODO:
 * Points to use on upgardes after LVL Up
 * PosionDamage, FireDamage
 * Buying stuff
 * Change the logic of defense for different armor types
*/

public class Character (string name, int maxHealth, int strength, 
    int defense, int agility, int attackSpeed, int money)
{
    // PROPERTIES

    public string Name { get; } = name;
    public int MaxHealth { get; } = maxHealth > 0 ? maxHealth 
        : throw new ArgumentOutOfRangeException(nameof(maxHealth), "MaxHealth cannot be zero or negative");
    public int Strength { get; } = strength;
    public int Defense { get; } = defense >= 0 ? defense 
        : throw new ArgumentOutOfRangeException(nameof(defense), "Armor cannot be negative");
    public int Agility { get; private set; } = agility >= 0 ? agility
        : throw new ArgumentOutOfRangeException(nameof(agility), "Agility cannot be negative");
    public int AttackSpeed { get; private set; } = attackSpeed >= 0 ? attackSpeed
        : throw new ArgumentOutOfRangeException(nameof(attackSpeed), "AttackSpeed cannot be negative");
    public int Money { get; private set; } = money >= 0 ? money
        : throw new ArgumentOutOfRangeException(nameof(money), "Money cannot be negative");

    private int health = maxHealth;
    public int Health 
    {
        get => health;
        private set => health = Math.Clamp(value,0,MaxHealth);
    }
    public bool IsAlive => Health > 0;
    public Weapon? Weapon { get; private set; }
    public Armor? Armor { get; private set; }
    public int AttackPower => Weapon?.Damage + Strength ?? Strength;
    public int Experience { get; private set; } = 0;
    public int Level { get; private set; } = 1;
    public int IsLevelUp => Level * Level * 100;
    private int AttackChance => Random.Shared.Next(1, 11) + AttackSpeed;
    public int UpgradePoint { get; private set; } = 0;


    // LISTS

    private readonly List<Weapon> _weapons = new();
    public IReadOnlyList<Weapon> Weapons => _weapons.AsReadOnly();
    private readonly List<Armor> _armors = new();
    public IReadOnlyList<Armor> Armors => _armors.AsReadOnly();
    private readonly Dictionary<Potion, int> _potions = new Dictionary<Potion, int>();
    public IReadOnlyDictionary<Potion, int> Potions => _potions.AsReadOnly();
    private readonly HashSet<string> _skills = new HashSet<string>();
    public IReadOnlySet<string> Skills => _skills.AsReadOnly();

    // METHODS

    // Health

    public void TakeDamage(int damage)
    {
        if (damage < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(damage),"Damage cannot be negative");
        } else if (damage <= Defense) {
            return;
        }
        Health -= damage - Defense;
    }

    public void Heal(int amount)
    {
        if(amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Heal cannot be negative");
        } else if (IsAlive )
        {
            Health += amount;
        }
    }

    // Levels

    public void GainExperience(int points)
    {
        if (points < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(points), "Points cannot be zero or negative");
        }
        Experience += points;

        while (Experience >= IsLevelUp)
        {
            Level += 1;
        }
    }

    // Attack

    public void Attack(Enemy target)
    {
        if (!IsAlive || !target.IsAlive) return;
        
        if (AttackChance < target.Agility)
        {
            return;
        }
        target.TakeDamage(AttackPower);

        if(!target.IsAlive)
        {
            GainExperience(target.Experience);
            Money += target.Money;
        }
    }

    // Weapons

    public void PickUp(Weapon weapon)
    {
        if (Weapon == null)
        {
            Weapon = weapon;
        }
        _weapons.Add(weapon);
    }

    public void Equip(Weapon weapon)
    {
        if (!_weapons.Contains(weapon))
        {
            throw new ArgumentException($"There is no {weapon} in inventory");
        }
        Weapon = weapon;
    }

    public void EquipStrongestWeapon()
    {
        if (_weapons.Count > 0)
        {
            Weapon strongest = _weapons[0];
            foreach (var weapon in _weapons)
            {
                if (weapon.Damage > strongest.Damage)
                {
                    strongest = weapon;
                }
            }
            Equip(strongest);
        }
    }

    public bool Drop(Weapon weapon)
    {
        if (!_weapons.Contains(weapon))
        {
            return false;
        }
        else if (Weapon == weapon)
        {
            Weapon = null;
            _weapons.Remove(weapon);
            return true;
        }
        else
        {
            _weapons.Remove(weapon);
            return true;
        }
    }

    // Supplies

    public void AddPotions(Potion item, int count)
    {
        if (count <= 0)
        {
            throw new ArgumentException("Count cannot be zero or negative");
        }
        else if (!_potions.ContainsKey(item))
        {
            _potions.Add(item, count);
        }
        else if (_potions.ContainsKey(item))
        {
            _potions[item] += count;
        }
    }

    public bool UsePotion(Potion item)
    {
        if (!_potions.ContainsKey(item))
        {
            return false;
        }
        _potions[item]--;
        if (_potions[item] == 0)
        {
            _potions.Remove(item);
        }
        return true;
    }

    // Skills

    public bool LearnSkill(string skill)
    {
        if (_skills.Contains(skill))
        {
            return false;
        }
        _skills.Add(skill);
        return true;
    }

    public bool HasSkill(string skill)
    {
        if (_skills.Contains(skill))
        {
            return true;
        }
        return false;
    }

    

    // Trade

    public void BuyItem(Item item)
    {
        if (Money - item.Price >= 0)
        {
            Money -= item.Price;
            if (item is Weapon weapon)
            {
                _weapons.Add(weapon);
            }
            else if (item is Armor armor)
            {
                _armors.Add(armor);
            }
            else if (item is Potion potion)
            {
                AddPotions(potion, 1);
            }
        }
    }

    public void SellItem(Item item)
    {
        if (item is Weapon weapon)
        {
            if (Drop(weapon) == true)
            {
                Money += Convert.ToInt32(weapon.Price * 0.25);
            }
        }
        else if (item is Armor armor)
        {
            if (_armors.Remove(armor) == true)
            {
                Money += Convert.ToInt32(armor.Price * 0.25);
            }
        }
        else if (item is Potion potion)
        {
            if (UsePotion(potion) == true) 
            {
                Money += Convert.ToInt32(potion.Price * 0.25);
            }
        }
    }

}
