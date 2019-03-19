﻿//----------------------------------------------------------------------------------------------
// <copyright file="ClippySetIndexer.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Clippy.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Clippy.Interfaces;
    using Clippy.Models;

    /// <summary>
    /// A concrete implementation of the <see cref="IClippySetIndexer"/> interface.
    /// </summary>
    public class ClippySetIndexer : IClippySetIndexer
    {
        private readonly List<Clippy> allClippies = new List<Clippy>();
        private readonly Dictionary<string, List<Clippy>> clippyKeywordMap = new Dictionary<string, List<Clippy>>();

        /// <inheritdoc />
        public Task IndexClippySetAsync(ClippySet clippySet, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (clippySet == null)
            {
                throw new ArgumentNullException(nameof(clippySet));
            }

            this.allClippies.AddRange(clippySet);

            foreach (var clippy in clippySet)
            {
                foreach (var keyword in clippy.Keywords)
                {
                    var indexedKeyword = keyword.ToLowerInvariant().Trim();
                    if (!this.clippyKeywordMap.TryGetValue(indexedKeyword, out var indexedClippies))
                    {
                        indexedClippies = new List<Clippy>();
                        this.clippyKeywordMap.Add(indexedKeyword, indexedClippies);
                    }

                    indexedClippies.Add(clippy);
                }
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<IEnumerable<Clippy>> FindClippiesByQuery(string query, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Task.FromResult<IEnumerable<Clippy>>(this.allClippies);
            }

            var queryWords = query.Trim().ToLowerInvariant().Split(' ');

            var matchedClippies = this.clippyKeywordMap
                .Where((keyValuePair) => queryWords.Any((word) => keyValuePair.Key.StartsWith(word)))
                .SelectMany((keyValuePair) => keyValuePair.Value)
                .Distinct()
                .ToArray();

            return Task.FromResult<IEnumerable<Clippy>>(matchedClippies);
        }
    }
}
