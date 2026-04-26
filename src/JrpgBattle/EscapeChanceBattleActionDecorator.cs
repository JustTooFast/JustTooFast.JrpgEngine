// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using JustTooFast.JrpgBattle.Abstractions.Contracts;
using JustTooFast.JrpgBattle.Abstractions.Models;

namespace JustTooFast.JrpgBattle;

public sealed class EscapeChanceBattleActionDecorator : IBattleActionResolver
{
    public const string EscapeSuccessChanceKey = "escape.success-chance";

    private readonly IBattleActionResolver _innerResolver;
    private readonly Random _random;

    public EscapeChanceBattleActionDecorator(
        IBattleActionResolver innerResolver,
        int seed)
    {
        _innerResolver = innerResolver ?? throw new ArgumentNullException(nameof(innerResolver));
        _random = new Random(seed);
    }

    public BattleResolution Resolve(BattleResolverContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        BattleResolution inner = _innerResolver.Resolve(context);

        if (!string.Equals(context.Action.ActionId, "escape", StringComparison.Ordinal))
        {
            return inner;
        }

        double escapeSuccessChance = GetEscapeSuccessChance(context.Definition.Configuration);

        var operations = new List<BattleOperation>(inner.Operations.Count + 1);

        foreach (BattleOperation operation in inner.Operations)
        {
            if (operation is EscapeSucceededOperation or EscapeFailedOperation)
            {
                continue;
            }

            operations.Add(operation);
        }

        operations.Add(
            _random.NextDouble() < escapeSuccessChance
                ? new EscapeSucceededOperation()
                : new EscapeFailedOperation());

        return new BattleResolution(operations);
    }

    private static double GetEscapeSuccessChance(BattleConfiguration configuration)
    {
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        if (!configuration.CanEscape)
        {
            return 0.0;
        }

        if (!configuration.ExtendedData.TryGetValue(EscapeSuccessChanceKey, out string? value))
        {
            throw new InvalidOperationException(
                $"Battle configuration extended data is missing required key '{EscapeSuccessChanceKey}'.");
        }

        if (!double.TryParse(
            value,
            System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands,
            System.Globalization.CultureInfo.InvariantCulture,
            out double escapeSuccessChance))
        {
            throw new InvalidOperationException(
                $"Battle configuration extended data key '{EscapeSuccessChanceKey}' must contain a valid double value.");
        }

        if (escapeSuccessChance < 0.0 || escapeSuccessChance > 1.0)
        {
            throw new InvalidOperationException(
                $"Battle configuration extended data key '{EscapeSuccessChanceKey}' must be between 0.0 and 1.0.");
        }

        return escapeSuccessChance;
    }
}