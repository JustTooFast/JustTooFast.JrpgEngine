// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JustTooFast.JrpgEngine.Utils;

public static class JsonFile
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public static T Load<T>(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("JSON file not found.", filePath);
        }

        try
        {
            var json = File.ReadAllText(filePath);
            var value = JsonSerializer.Deserialize<T>(json, SerializerOptions);

            if (value is null)
            {
                throw new InvalidOperationException(
                    $"JSON file '{filePath}' deserialized to null.");
            }

            return value;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Failed to parse JSON file '{filePath}': {ex.Message}",
                ex);
        }
    }
}