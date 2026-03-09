// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Text.Json;

namespace JustTooFast.JrpgEngine.Utils;

public static class JsonFile
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static T Load<T>(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("JSON file path cannot be null or empty.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"JSON file not found: {filePath}", filePath);
        }

        try
        {
            var json = File.ReadAllText(filePath);
            var result = JsonSerializer.Deserialize<T>(json, SerializerOptions);

            if (result is null)
            {
                throw new InvalidOperationException(
                    $"Deserialization returned null for type {typeof(T).Name} from file '{filePath}'.");
            }

            return result;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Failed to parse JSON file '{filePath}': {ex.Message}", ex);
        }
    }
}