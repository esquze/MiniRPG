using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace MiniRPG.Core;

/* TODO:
 * Damage
 * PosionDamage, FireDamage
 * Change the logic of defense for different armor types
 * Redo skills
*/

public enum Stats
{
    MaxHealth,
    Strength,
    Agility,
    AttackSpeed
}

public class Character (string name, int maxHealth, int strength, 
                        int agility, int attackSpeed, int money)
{
    // PROPERTIES

    public string Name { get; } = name;

    public int MaxHealth { get; private set; } = maxHealth > 0 ? maxHealth 
        : throw new ArgumentOutOfRangeException(nameof(maxHealth), "MaxHealth cannot be zero or negative");
    public int Strength { get; private set; } = strength > 0 ? strength
        : throw new ArgumentOutOfRangeException(nameof(strength), "Strength cannot be zero or negative");
    public int Agility { get; private set; } = agility > 0 ? agility
        : throw new ArgumentOutOfRangeException(nameof(agility), "Agility cannot be zero or negative");
    public int AttackSpeed { get; private set; } = attackSpeed > 0 ? attackSpeed
        : throw new ArgumentOutOfRangeException(nameof(attackSpeed), "AttackSpeed cannot be zero or negative");
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
    private int CurrentDefense;
    public int Defense 
    { 
        get => Armor?.Defense ?? 0; 
    } 
    private int AttackPower => Weapon?.Damage + Strength ?? Strength;
    private int AttackChance => Random.Shared.Next(1, 6) + AttackSpeed;
    private int Experience { get; set; } = 0;
    private int Level { get; set; } = 1;
    private int IsLevelUp => Level * Level * 100;
    private int UpgradePoint { get; set; } = 0;


    // LISTS

    private readonly List<Weapon> _weapons = new();
    public IReadOnlyList<Weapon> Weapons => _weapons.AsReadOnly();
    private readonly List<Armor> _armors = new();
    public IReadOnlyList<Armor> Armors => _armors.AsReadOnly();
    private readonly Dictionary<Potion, int> _potions = new Dictionary<Potion, int>();
    public IReadOnlyDictionary<Potion, int> Potions => _potions.AsReadOnly();
    //private readonly HashSet<string> _skills = new HashSet<string>();
    //public IReadOnlySet<string> Skills => _skills.AsReadOnly();

    // METHODS

    // Health

    public void TakeDamage(int damage)
    {
        if (damage < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(damage), "Damage cannot be negative");
        }
        if (damage > CurrentDefense)
        {
            Health -= damage - CurrentDefense;
            CurrentDefense = 0;
        } else
        {
            CurrentDefense -= damage;
        }
    }

    public void Heal(int amount)
    {
        if(amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Heal cannot be negative");
        } else if (IsAlive)
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
            UpgradePoint += 1;
        }
    }

    public void Upgrade(Stats stat)
    {
        if (UpgradePoint > 0)
        {
            if (stat is Stats.MaxHealth)
            {
                MaxHealth = Convert.ToInt32(MaxHealth / 0.8);
                UpgradePoint -= 1;
            }
            if (stat is Stats.Strength)
            {
                UpgradePoint -= 1;
            }
            if (stat is Stats.Agility)
            {
                Agility += 1;
                UpgradePoint -= 1;
            }
            if (stat is Stats.AttackSpeed)
            {
                AttackSpeed += 1;
                UpgradePoint -= 1;
            }
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

    // Items

    public void PickUp(Item item)
    {
        if (item is Weapon)
        {
            if (Weapon == null)
            {
                Weapon = (Weapon)item;
            }
            _weapons.Add((Weapon)item);
        }
        if (item is Armor)
        {
            if (Armor == null)
            {
                Armor = (Armor)item;
            }
            _armors.Add((Armor)item);
        }
        if (item is Potion)
        {
            AddPotion((Potion)item, 1);
        }
    }

    public void Equip(Item item)
    {
        if (item is Weapon)
        {
            if (!_weapons.Contains(item))
            {
                throw new ArgumentException($"There is no {item} in inventory");
            }
            Weapon = (Weapon)item;
        }
        if (item is Armor)
        {
            if (!_armors.Contains(item))
            {
                throw new ArgumentException($"There is no {item} in inventory");
            }
            Armor = (Armor)item;
            CurrentDefense = Defense;
        }
    }

    public bool Drop(Item item)
    {
        if (item is Weapon)
        {
            if (!_weapons.Contains(item))
            {
                return false;
            }
            else if (Weapon == item)
            {
                Weapon = null;
                _weapons.Remove((Weapon)item);
                return true;
            }
            else
            {
                _weapons.Remove((Weapon)item);
                return true;
            }
        }
        if (item is Armor)
        {
            if (!_armors.Contains(item))
            {
                return false;
            }
            else if (Armor == item)
            {
                Armor = null;
                _armors.Remove((Armor)item);
                CurrentDefense = 0;
                return true;
            }
            else
            {
                _armors.Remove((Armor)item);
                return true;
            }
        }
        if (item is Potion)
        {
            if (UsePotion((Potion)item) == true)
            {
                return true;
            }
            return false;
        }
        else { return false; }
    }

    // Weapons

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

    // Supplies

    public void AddPotion(Potion item, int count)
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

    //public bool LearnSkill(string skill)
    //{
    //    if (_skills.Contains(skill))
    //    {
    //        return false;
    //    }
    //    _skills.Add(skill);
    //    return true;
    //}

    //public bool HasSkill(string skill)
    //{
    //    if (_skills.Contains(skill))
    //    {
    //        return true;
    //    }
    //    return false;
    //}

    

    // Trade

    public bool BuyItem(Item item)
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
                AddPotion(potion, 1);
            }
            return true;
        }
        return false;
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
            if (Drop(armor) == true)
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
