﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GitHubVulnerability2Db.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GitHubVulnerability2Db.GraphQL
{
    public class QueryService : IQueryService
    {
        public const string UserAgent = "NuGet.Jobs.GitHubVulnerability2Db";

        public QueryService(
            InitializationConfiguration initializationConfiguration,
            HttpClient client)
        {
            InitializationConfiguration = initializationConfiguration;
            Client = client;
        }

        public InitializationConfiguration InitializationConfiguration { get; set; }
        public HttpClient Client { get; set; }

        public async Task<QueryResponse> QueryAsync(string query, CancellationToken token)
        {
            var queryJObject = new JObject
            {
                ["query"] = query
            };

            var response = await GetResponseFromQuery(queryJObject.ToString(), token);
            return JsonConvert.DeserializeObject<QueryResponse>(response);
        }

        private async Task<string> GetResponseFromQuery(string query, CancellationToken token)
        {
            using (var request = CreateRequest(query))
            using (var response = await Client.SendAsync(request, HttpCompletionOption.ResponseContentRead, token))
            {
                return await response.Content.ReadAsStringAsync();
            }
        }

        private HttpRequestMessage CreateRequest(string query)
        {
            var message = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = InitializationConfiguration.GitHubGraphQlQueryEndpoint,
                Content = new StringContent(query, Encoding.UTF8, "application/json")
            };

            message.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer", InitializationConfiguration.GitHubPersonalAccessToken);
            message.Headers.UserAgent.TryParseAdd(UserAgent);
            return message;
        }
    }
}
