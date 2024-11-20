﻿using NETServer.Core.Network.Packet;
using NETServer.Interfaces.Core.Network;

using System.Reflection;

namespace NETServer.Application.Handlers
{
    internal class CommandHandler
    {
        private static readonly Lazy<Dictionary<Cmd, MethodInfo>> CommandCache = new Lazy<Dictionary<Cmd, MethodInfo>>(() =>
            LoadMethodsWithCommandAttribute("NETServer.Application.Handlers.Client")
        );

        private static Dictionary<Cmd, MethodInfo> LoadMethodsWithCommandAttribute(string targetNamespace)
        {
            var assembly = Assembly.GetExecutingAssembly();

            return assembly.GetTypes()
                           .Where(t => t.Namespace == targetNamespace)
                           .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
                           .Where(m => m.GetCustomAttribute<CommandAttribute>() != null)
                           .ToDictionary(
                               m => m.GetCustomAttribute<CommandAttribute>()!.Command, // Key: Command từ Attribute
                               m => m // Value: MethodInfo
                           );
        }

        private static Dictionary<Cmd, MethodInfo> CommandCacheValue => CommandCache.Value;


        public async ValueTask HandleCommand(ISession session, Packet packet, CancellationToken cancellationToken)
        {
            Packet newPacket = new(session.Id);
            newPacket.SetCommand(Cmd.ERROR);

            if (packet.Command == (short)Cmd.NONE)
            {
                newPacket.SetPayload("Invalid command: Command is null or invalid.");
                await session.Send(newPacket.ToByteArray());
                return;
            }

            var command = (Cmd)packet.Command;

            if (!CommandCacheValue.TryGetValue(command, out var method))
            {
                newPacket.SetPayload($"Unknown command: {command}");
                await session.Send(newPacket.ToByteArray());
                return;
            }

            try
            {
                // Delegate creation
                var func = (Func<ISession, byte[], CancellationToken, ValueTask>)method
                                .CreateDelegate(typeof(Func<ISession, byte[], CancellationToken, ValueTask>),
                                method.IsStatic ? null : this);

                await func(session, packet.Payload.Span.ToArray(), cancellationToken); // ToArray only when necessary
            }
            catch (Exception ex)
            {
                newPacket.SetPayload($"Error executing command: {command}. Exception: {ex.Message}");
                await session.Send(newPacket.ToByteArray());
            }
        }
    }
}