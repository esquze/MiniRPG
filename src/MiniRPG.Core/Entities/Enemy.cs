using System;
using System.Collections.Generic;
using System.Text;

namespace MiniRPG.Core;

/* 
 * TODO: 
 * PotionDamage, FireDamage
 * Types of enemies
 * 
*/

public class Enemy(string name, int maxHealth, int damage, int armor, 
    int experience, int agility, int attackSpeed, int money)
{

    // Properties

    public string Name { get; } = name;
    public int MaxHealth { get; } = maxHealth > 0 ? maxHealth
        : throw new ArgumentOutOfRangeException(nameof(maxHealth), "MaxHealth cannot be zero or negative");
    public int Damage { get; } = damage;
    public int Armor { get; } = armor >= 0 ? armor
        : throw new ArgumentOutOfRangeException(nameof(armor), "Armor cannot be negative");
    public int Experience { get; } = experience > 0 ? experience 
        : throw new ArgumentOutOfRangeException(nameof(experience), "Experience cannot be zero or negative");
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
        private set => health = Math.Clamp(value, 0, MaxHealth);
    }
    public bool IsAlive => Health > 0;
    private int AttackChance => Random.Shared.Next(1, 11) + AttackSpeed;


    // Methods

    // Health

    public void TakeDamage(int damage)
    {
        if (damage < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(damage), "Damage cannot be negative");
        }
        else if (damage <= Armor)
        {
            return;
        }
        Health -= damage - Armor;
    }
    public void Heal(int amount)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Heal cannot be negative");
        }
        else if (IsAlive)
        {
            Health += amount;
        }
    }

    // Attack

    public void Attack(Character target)
    {
        if (!IsAlive || !target.IsAlive) return;

        if (AttackChance < target.Agility)
        {
            return;
        }
        target.TakeDamage(Damage);
    }
}
