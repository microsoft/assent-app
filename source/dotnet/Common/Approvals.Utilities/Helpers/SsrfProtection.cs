// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Utilities.Helpers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

public static class SsrfProtection
{
    public static HttpMessageHandler CreateHandler()
    {
        return new SocketsHttpHandler
        {
            AllowAutoRedirect = true,
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            UseCookies = false,
            UseProxy = false,
            ConnectCallback = ConnectAsync,
        };
    }

    private static async ValueTask<Stream> ConnectAsync(
        SocketsHttpConnectionContext context,
        CancellationToken cancellationToken)
    {
        var endpoint = context.DnsEndPoint;
        var addresses = await ResolveAsync(endpoint.Host, cancellationToken);
        EnsurePublic(endpoint.Host, addresses);
        var failures = new List<Exception>();

        foreach (var address in addresses)
        {
            var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                await socket.ConnectAsync(new IPEndPoint(address, endpoint.Port), cancellationToken);
                return new NetworkStream(socket, ownsSocket: true);
            }
            catch (Exception exception) when (exception is SocketException or IOException)
            {
                failures.Add(exception);
                socket.Dispose();
            }
            catch
            {
                socket.Dispose();
                throw;
            }
        }

        throw new HttpRequestException(
            $"Unable to connect to the validated endpoint '{endpoint.Host}'.",
            new AggregateException(failures));
    }

    private static async ValueTask<IReadOnlyList<IPAddress>> ResolveAsync(
        string host,
        CancellationToken cancellationToken)
    {
        if (IPAddress.TryParse(host, out var address))
        {
            return new[] { address };
        }

        try
        {
            return await Dns.GetHostAddressesAsync(host, cancellationToken);
        }
        catch (Exception exception) when (exception is SocketException or ArgumentException)
        {
            throw new HttpRequestException($"Unable to resolve outbound host '{host}'.", exception);
        }
    }

    private static void EnsurePublic(string host, IReadOnlyList<IPAddress> addresses)
    {
        if (addresses.Count == 0)
        {
            throw new HttpRequestException($"Outbound host '{host}' did not resolve to an IP address.");
        }

        if (addresses.Any(address => !IsPublic(address)))
        {
            throw new HttpRequestException($"Outbound host '{host}' resolves to a restricted IP address.");
        }
    }

    private static bool IsPublic(IPAddress address)
    {
        if (IPAddress.IsLoopback(address))
        {
            return false;
        }

        if (address.IsIPv4MappedToIPv6)
        {
            return IsPublic(address.MapToIPv4());
        }

        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            return IsPublicIpv4(address.GetAddressBytes());
        }

        if (address.AddressFamily != AddressFamily.InterNetworkV6
            || address.Equals(IPAddress.IPv6Any)
            || address.Equals(IPAddress.IPv6None)
            || address.IsIPv6LinkLocal
            || address.IsIPv6Multicast
            || address.IsIPv6SiteLocal)
        {
            return false;
        }

        var bytes = address.GetAddressBytes();
        if (IsIpv4Compatible(bytes) || IsNat64(bytes))
        {
            return IsPublicIpv4(bytes[^4..]);
        }

        if (bytes[0] == 0x20 && bytes[1] == 0x02)
        {
            return IsPublicIpv4(bytes[2..6]);
        }

        return (bytes[0] & 0xE0) == 0x20
            && !HasPrefix(bytes, 0x20, 0x01, 0x00, 0x00)
            && !HasPrefix(bytes, 0x20, 0x01, 0x00, 0x02)
            && !HasPrefix(bytes, 0x20, 0x01, 0x0D, 0xB8)
            && !IsOrchid(bytes)
            && !IsDocumentationPrefix(bytes);
    }

    private static bool IsPublicIpv4(ReadOnlySpan<byte> bytes)
    {
        var first = bytes[0];
        var second = bytes[1];
        var third = bytes[2];

        return first != 0
            && first != 10
            && first != 127
            && !(first == 100 && second is >= 64 and <= 127)
            && !(first == 169 && second == 254)
            && !(first == 172 && second is >= 16 and <= 31)
            && !(first == 192 && second == 0 && third == 0)
            && !(first == 192 && second == 0 && third == 2)
            && !(first == 192 && second == 88 && third == 99)
            && !(first == 192 && second == 168)
            && !(first == 198 && second is 18 or 19)
            && !(first == 198 && second == 51 && third == 100)
            && !(first == 203 && second == 0 && third == 113)
            && first < 224;
    }

    private static bool IsIpv4Compatible(ReadOnlySpan<byte> bytes)
    {
        for (var index = 0; index < 12; index++)
        {
            if (bytes[index] != 0)
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsNat64(ReadOnlySpan<byte> bytes)
    {
        return bytes[0] == 0x00
            && bytes[1] == 0x64
            && bytes[2] == 0xFF
            && bytes[3] == 0x9B
            && bytes[4] == 0x00
            && bytes[5] == 0x00
            && bytes[6] == 0x00
            && bytes[7] == 0x00
            && bytes[8] == 0x00
            && bytes[9] == 0x00
            && bytes[10] == 0x00
            && bytes[11] == 0x00;
    }

    private static bool IsOrchid(ReadOnlySpan<byte> bytes)
    {
        return bytes[0] == 0x20
            && bytes[1] == 0x01
            && bytes[2] == 0x00
            && (bytes[3] & 0xF0) is 0x10 or 0x20;
    }

    private static bool IsDocumentationPrefix(ReadOnlySpan<byte> bytes)
    {
        return bytes[0] == 0x3F
            && bytes[1] == 0xFF
            && (bytes[2] & 0xF0) == 0;
    }

    private static bool HasPrefix(
        ReadOnlySpan<byte> bytes,
        byte first,
        byte second,
        byte third,
        byte fourth)
    {
        return bytes[0] == first
            && bytes[1] == second
            && bytes[2] == third
            && bytes[3] == fourth;
    }
}
