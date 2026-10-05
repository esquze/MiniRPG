using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace MiniRPG.Core;

/* TODO:
 * Points to use on upgardes after LVL Up
 * PotionDamage, FireDamage
 * Buying stuff
*/

public class Character (string name, int maxHealth, int strength, 
    int armor, int agility, int attackSpeed, int money)
{
    // PROPERTIES

    public string Name { get; } = name;
    public int MaxHealth { get; } = maxHealth > 0 ? maxHealth 
        : throw new ArgumentOutOfRangeException(nameof(maxHealth), "MaxHealth cannot be zero or negative");
    public int Strength { get; } = strength;
    public int Armor { get; } = armor >= 0 ? armor 
        : throw new ArgumentOutOfRangeException(nameof(armor), "Armor cannot be negative");
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
    public int AttackPower => Weapon?.Damage + Strength ?? Strength;
    public int Experience { get; private set; } = 0;
    public int Level { get; private set; } = 1;
    public int IsLevelUp => Level * Level * 100;
    private int AttackChance => Random.Shared.Next(1, 11) + AttackSpeed;
    public int UpgradePoint { get; private set; } = 0;


    // LISTS

    private readonly List<Weapon> _weapons = new();
    public IReadOnlyList<Weapon> Weapons => _weapons.AsReadOnly();
    private readonly HashSet<string> _skills = new HashSet<string>();
    public IReadOnlySet<string> Skills => _skills.AsReadOnly();
    private readonly Dictionary<string, int> _supplies = new Dictionary<string, int>();
    public IReadOnlyDictionary<string, int> Supplies => _supplies.AsReadOnly();

    // METHODS

    // Health

    public void TakeDamage(int damage)
    {
        if (damage < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(damage),"Damage cannot be negative");
        } else if (damage <= Armor) {
            return;
        }
        Health -= damage - Armor;
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

    // Supplies

    public void AddSupply(string item, int count)
    {
        if (count <= 0)
        {
            throw new ArgumentException("Count cannot be zero or negative");
        }
        else if (!_supplies.ContainsKey(item))
        {
            _supplies.Add(item, count);
        }
        else if (_supplies.ContainsKey(item))
        {
            _supplies[item] += count;
        }
    }

    public int GetSupplyCount(string item)
    {
        _supplies.TryGetValue(item, out int count);
        return count;
    }

    public bool UseSupply(string item)
    {
        if (!_supplies.ContainsKey(item))
        {
            return false;
        }
        _supplies[item]--;
        if (_supplies[item] == 0)
        {
            _supplies.Remove(item);
        }
        return true;
    }

    // Money


}
