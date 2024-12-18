﻿// Copyright (c) 2024 Max Run Software (dev@maxrunsoftware.com)
// 
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Net;
using System.Net.Cache;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace MaxRunSoftware.Utilities.Common;

public static partial class Util
{
    /// <summary>
    /// Attempts to get the current local time from the internet
    /// </summary>
    /// <returns>The current local time</returns>
    public static DateTime NetGetInternetDateTime()
    {
        // http://stackoverflow.com/questions/6435099/how-to-get-datetime-from-the-internet

        ServicePointManager.SecurityProtocol = (SecurityProtocolType)(0xc0 | 0x300 | 0xc00);
        // SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

#pragma warning disable SYSLIB0014 // Type or member is obsolete
        var request = (HttpWebRequest)WebRequest.Create("http://worldtimeapi.org/api/timezone/Europe/London.txt");
#pragma warning restore SYSLIB0014 // Type or member is obsolete
        request.Method = "GET";
        request.Accept = "text/html, application/xhtml+xml, */*";
        request.UserAgent = "p2pcopy";
        request.ContentType = "application/x-www-form-urlencoded";
        request.CachePolicy = new(RequestCacheLevel.NoCacheNoStore); //No caching
        var response = (HttpWebResponse)request.GetResponse();
        if (response.StatusCode != HttpStatusCode.OK) throw new WebException(response.StatusCode + ":" + response.StatusDescription);

        var responseStream = response.GetResponseStream();
        responseStream.CheckNotNull(nameof(responseStream));
        var stream = new StreamReader(responseStream);
        var html = stream.ReadToEnd(); //<timestamp time=\"1395772696469995\" delay=\"1395772696469995\"/>
        var time = Regex.Match(html, @"(?<=unixtime: )[^u]*").Value;
        var milliseconds = Convert.ToInt64(time) * 1000.0;
        var dateTime = new DateTime(1970, 1, 1).AddMilliseconds(milliseconds).ToLocalTime();

        return dateTime;
    }

    /// <summary>
    /// Get all of the IP addresses on the current machine
    /// </summary>
    /// <returns>The IP addresses</returns>
    // ReSharper disable once InconsistentNaming
    public static IEnumerable<IPAddress> NetGetIPAddresses() =>
        NetworkInterface.GetAllNetworkInterfaces()
            .Where(o => o.OperationalStatus == OperationalStatus.Up)
            .Select(o => o.GetIPProperties())
            .WhereNotNull()
            .SelectMany(o => o.UnicastAddresses)
            .Select(o => o.Address)
            .WhereNotNull();

    private static bool[] NetGetPortStatusInternal()
    {
        var array = new bool[65536]; // array[0] should not ever be populated
        array.Populate(true);

        var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();

        foreach (var tcpConnectionInformation in ipGlobalProperties.GetActiveTcpConnections()) array[tcpConnectionInformation.LocalEndPoint.Port] = false;

        foreach (var ipEndPoint in ipGlobalProperties.GetActiveTcpListeners()) array[ipEndPoint.Port] = false;

        return array;
    }

    public static IEnumerable<(int port, bool isOpen)> NetGetPortStatus()
    {
        var portStatus = NetGetPortStatusInternal();
        for (var i = 1; i < portStatus.Length; i++) yield return (i, portStatus[i]);
    }

    public static IEnumerable<int> NetGetOpenPorts() => NetGetPortStatus().Where(o => o.isOpen).Select(o => o.port);

    public static IEnumerable<int> NetGetClosedPorts() => NetGetPortStatus().Where(o => !o.isOpen).Select(o => o.port);

    public static bool NetIsPortAvailable(int port) => NetGetPortStatus().Where(o => o.port == port).Select(o => o.isOpen).FirstOrDefault();

    /// <summary>
    /// Tries to find an open port in a range or if none is found a -1 is returned
    /// </summary>
    /// <param name="startInclusive">The inclusive starting port to search for an open port</param>
    /// <param name="endInclusive">The inclusive ending port to search for an open port</param>
    /// <returns>An open port or -1 if none were found</returns>
    public static int NetFindOpenPort(int startInclusive, int endInclusive = 65535)
    {
        if (startInclusive < 1) startInclusive = 1;

        if (endInclusive > 65535) endInclusive = 65535;

        foreach (var portStatus in NetGetPortStatus())
        {
            if (portStatus.port < startInclusive) continue;

            if (portStatus.port > endInclusive) continue;

            if (!portStatus.isOpen) continue;

            return portStatus.port;
        }

        return -1;
    }

    public static IEnumerable<Tuple<string, string>> ParseDistinguishedName(this string value)
    {
        // https://stackoverflow.com/a/53337546

        const int componentString = 1;
        const int quotedString = 2;
        const int escapedCharacter = 3;

        var previousState = componentString;
        var currentState = componentString;
        var currentComponent = new StringBuilder();
        var previousChar = char.MinValue;
        var position = 0;

        Tuple<string, string>? ParseComponent(StringBuilder sb)
        {
            var s = sb.ToString();
            sb.Clear();

            var index = s.IndexOf('=');
            if (index == -1) return null;

            var item1 = s.Substring(0, index).Trim().ToUpper();
            var item2 = s.Substring(index + 1).Trim();

            return Tuple.Create(item1, item2);
        }

        while (position < value.Length)
        {
            var currentChar = value[position];

            switch (currentState)
            {
                case componentString:
                    switch (currentChar)
                    {
                        case ',':
                        case ';':
                            // Separator found, yield parsed component
                            var component = ParseComponent(currentComponent);
                            if (component != null) yield return component;

                            break;

                        case '\\':
                            // Escape character found
                            previousState = currentState;
                            currentState = escapedCharacter;
                            break;

                        case '"':
                            // Quotation mark found
                            if (previousChar == currentChar)
                                // Double quotes inside quoted string produce single quote
                            {
                                currentComponent.Append(currentChar);
                            }

                            currentState = quotedString;
                            break;

                        default:
                            currentComponent.Append(currentChar);
                            break;
                    }

                    break;

                case quotedString:
                    switch (currentChar)
                    {
                        case '\\':
                            // Escape character found
                            previousState = currentState;
                            currentState = escapedCharacter;
                            break;

                        case '"':
                            // Quotation mark found
                            currentState = componentString;
                            break;

                        default:
                            currentComponent.Append(currentChar);
                            break;
                    }

                    break;

                case escapedCharacter:
                    currentComponent.Append(currentChar);
                    currentState = previousState;
                    currentChar = char.MinValue;
                    break;
            }

            previousChar = currentChar;
            position++;
        }

        // Yield last parsed component, if any
        if (currentComponent.Length > 0)
        {
            var component = ParseComponent(currentComponent);
            if (component != null) yield return component;
        }
    }
}
