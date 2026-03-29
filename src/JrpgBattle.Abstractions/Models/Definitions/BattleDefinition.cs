// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace JustTooFast.JrpgBattle.Abstractions.Models;

public sealed record BattleDefinition
{
    public BattleDefinition(
        IReadOnlyList<BattleActorDefinition> partyActors,
        IReadOnlyList<BattleActorDefinition> enemyActors,
        BattleConfiguration configuration,
        IReadOnlyList<BattleTeamDefinition> teamDefinitions)
    {
        if (partyActors is null)
        {
            throw new ArgumentNullException(nameof(partyActors));
        }

        if (enemyActors is null)
        {
            throw new ArgumentNullException(nameof(enemyActors));
        }

        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        if (teamDefinitions is null)
        {
            throw new ArgumentNullException(nameof(teamDefinitions));
        }

        if (partyActors.Count == 0)
        {
            throw new ArgumentException("At least one party actor is required.", nameof(partyActors));
        }

        if (enemyActors.Count == 0)
        {
            throw new ArgumentException("At least one enemy actor is required.", nameof(enemyActors));
        }

        if (partyActors.Any(static a => a is null))
        {
            throw new ArgumentException("Party actors cannot contain null entries.", nameof(partyActors));
        }

        if (enemyActors.Any(static a => a is null))
        {
            throw new ArgumentException("Enemy actors cannot contain null entries.", nameof(enemyActors));
        }

        if (teamDefinitions.Any(static d => d is null))
        {
            throw new ArgumentException("Team definitions cannot contain null entries.", nameof(teamDefinitions));
        }

        if (partyActors.Any(static a => a.Team != BattleTeam.Party))
        {
            throw new ArgumentException("All party actors must have Party team.", nameof(partyActors));
        }

        if (enemyActors.Any(static a => a.Team != BattleTeam.Enemy))
        {
            throw new ArgumentException("All enemy actors must have Enemy team.", nameof(enemyActors));
        }

        string[] duplicateActorIds = partyActors
            .Concat(enemyActors)
            .GroupBy(static a => a.Id, StringComparer.Ordinal)
            .Where(static g => g.Count() > 1)
            .Select(static g => g.Key)
            .ToArray();

        if (duplicateActorIds.Length > 0)
        {
            throw new ArgumentException("Actor ids must be unique across the whole battle.", nameof(partyActors));
        }

        string[] duplicateTeams = teamDefinitions
            .GroupBy(static d => d.Team)
            .Where(static g => g.Count() > 1)
            .Select(static g => g.Key.ToString())
            .ToArray();

        if (duplicateTeams.Length > 0)
        {
            throw new ArgumentException(
                $"Team definitions must be unique per team: {string.Join(", ", duplicateTeams)}.",
                nameof(teamDefinitions));
        }

        BattleTeam[] missingTeams = partyActors
            .Concat(enemyActors)
            .Select(static a => a.Team)
            .Distinct()
            .Where(team => !teamDefinitions.Any(d => d.Team == team))
            .ToArray();

        if (missingTeams.Length > 0)
        {
            throw new ArgumentException(
                $"Missing team definitions for teams: {string.Join(", ", missingTeams)}.",
                nameof(teamDefinitions));
        }

        PartyActors = new ReadOnlyCollection<BattleActorDefinition>(partyActors.ToArray());
        EnemyActors = new ReadOnlyCollection<BattleActorDefinition>(enemyActors.ToArray());
        Configuration = configuration;
        TeamDefinitions = new ReadOnlyCollection<BattleTeamDefinition>(teamDefinitions.ToArray());
    }

    public IReadOnlyList<BattleActorDefinition> PartyActors { get; }

    public IReadOnlyList<BattleActorDefinition> EnemyActors { get; }

    public BattleConfiguration Configuration { get; }

    public IReadOnlyList<BattleTeamDefinition> TeamDefinitions { get; }
}