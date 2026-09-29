using System;
using System.Collections.Generic;
using System.Text;

namespace MiniRPG.Core;

public class Enemy(string name, int maxHealth, int strength, int armor)
{
    public string Name { get; } = name;
    public int MaxHealth { get; } = maxHealth > 0 ? maxHealth
        : throw new ArgumentOutOfRangeException(nameof(maxHealth), "MaxHealth cannot be zero nor negative");
    public int Strength { get; } = strength;
    public int Armor { get; } = armor >= 0 ? armor
        : throw new ArgumentOutOfRangeException(nameof(armor), "Armor cannot be negative");
    private int health = maxHealth;
    public int Health
    {
        get => health;
        private set => health = Math.Clamp(value, 0, MaxHealth);
    }
    public bool IsAlive => Health > 0;
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
    public void Heal(int ammout)
    {
        if (IsAlive)
        {
            Health += ammout;
        }
    }
}
