﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace raccoonLog
{
    public class UrlLog
    {
        public UrlLog(int port, string path, string host, string scheme,
            IReadOnlyList<KeyValuePair<string, string>> parameters)
        {
            Port = port;
            Path = path;
            Host = host;
            Scheme = scheme;
            Parameters = parameters;
        }

        public int Port { get; private set; }

        public string Path { get; private set; }

        public string Host { get; private set; }

        public string Scheme { get; private set; }

        public string Abosolute => this.ToString();

        public IReadOnlyList<KeyValuePair<string, string>> Parameters { get; private set; }

        public override string ToString()
        {
            return new UriBuilder(Scheme, Host, Port, Path, QueryString.Create(Parameters).Value).Uri.ToString();
        }
    }
}