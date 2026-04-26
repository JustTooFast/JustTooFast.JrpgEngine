// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleActorDefinition
{
    public BattleActorDefinition(
        string id,
        string name,
        BattleTeam team,
        BattleActorControlKind controlKind,
        int currentHp,
        int maxHp,
        IReadOnlyList<BattleActionDefinition> allowedActions,
        IReadOnlyList<BattleAbilityDefinition> spells,
        IReadOnlyList<BattleAbilityDefinition> skills,
        BattleActorBehaviorDefinition? automatedBehavior,
        BattleExtendedData extendedData)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Actor id is required.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Actor name is required.", nameof(name));
        }

        if (team is not BattleTeam.Party and not BattleTeam.Enemy)
        {
            throw new ArgumentOutOfRangeException(nameof(team), "Actor team must be Party or Enemy.");
        }

        if (!Enum.IsDefined(controlKind))
        {
            throw new ArgumentOutOfRangeException(nameof(controlKind), "Control kind must be a defined value.");
        }

        if (maxHp <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxHp), "Max HP must be greater than zero.");
        }

        if (currentHp < 0 || currentHp > maxHp)
        {
            throw new ArgumentOutOfRangeException(nameof(currentHp), "Current HP must be between 0 and Max HP.");
        }

        if (allowedActions is null)
        {
            throw new ArgumentNullException(nameof(allowedActions));
        }

        if (spells is null)
        {
            throw new ArgumentNullException(nameof(spells));
        }

        if (skills is null)
        {
            throw new ArgumentNullException(nameof(skills));
        }

        if (extendedData is null)
        {
            throw new ArgumentNullException(nameof(extendedData));
        }

        if (controlKind == BattleActorControlKind.Automated && automatedBehavior is null)
        {
            throw new ArgumentException(
                "Automated actors must provide automated behavior.",
                nameof(automatedBehavior));
        }

        if (allowedActions.Any(static action => action is null))
        {
            throw new ArgumentException("Allowed actions cannot contain null entries.", nameof(allowedActions));
        }

        if (spells.Any(static spell => spell is null))
        {
            throw new ArgumentException("Spells cannot contain null entries.", nameof(spells));
        }

        if (skills.Any(static skill => skill is null))
        {
            throw new ArgumentException("Skills cannot contain null entries.", nameof(skills));
        }

        string[] duplicateAllowedActionIds = allowedActions
            .GroupBy(static action => action.ActionId, StringComparer.Ordinal)
            .Where(static group => group.Count() > 1)
            .Select(static group => group.Key)
            .ToArray();

        if (duplicateAllowedActionIds.Length > 0)
        {
            throw new ArgumentException(
                $"Allowed action ids must be unique per actor: {string.Join(", ", duplicateAllowedActionIds)}.",
                nameof(allowedActions));
        }

        string[] duplicateSpellActionIds = spells
            .GroupBy(static spell => spell.ActionId, StringComparer.Ordinal)
            .Where(static group => group.Count() > 1)
            .Select(static group => group.Key)
            .ToArray();

        if (duplicateSpellActionIds.Length > 0)
        {
            throw new ArgumentException(
                $"Spell action ids must be unique per actor: {string.Join(", ", duplicateSpellActionIds)}.",
                nameof(spells));
        }

        string[] duplicateSkillActionIds = skills
            .GroupBy(static skill => skill.ActionId, StringComparer.Ordinal)
            .Where(static group => group.Count() > 1)
            .Select(static group => group.Key)
            .ToArray();

        if (duplicateSkillActionIds.Length > 0)
        {
            throw new ArgumentException(
                $"Skill action ids must be unique per actor: {string.Join(", ", duplicateSkillActionIds)}.",
                nameof(skills));
        }

        Id = id;
        Name = name;
        Team = team;
        ControlKind = controlKind;
        CurrentHp = currentHp;
        MaxHp = maxHp;
        AllowedActions = new ReadOnlyCollection<BattleActionDefinition>(allowedActions.ToArray());
        Spells = new ReadOnlyCollection<BattleAbilityDefinition>(spells.ToArray());
        Skills = new ReadOnlyCollection<BattleAbilityDefinition>(skills.ToArray());
        AutomatedBehavior = automatedBehavior;
        ExtendedData = extendedData;
    }

    public string Id { get; }

    public string Name { get; }

    public BattleTeam Team { get; }

    public BattleActorControlKind ControlKind { get; }

    public int CurrentHp { get; }

    public int MaxHp { get; }

    public IReadOnlyList<BattleActionDefinition> AllowedActions { get; }

    public IReadOnlyList<BattleAbilityDefinition> Spells { get; }

    public IReadOnlyList<BattleAbilityDefinition> Skills { get; }

    public BattleActorBehaviorDefinition? AutomatedBehavior { get; }

    public BattleExtendedData ExtendedData { get; }
}