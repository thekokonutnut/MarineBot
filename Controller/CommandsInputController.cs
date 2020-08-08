using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MarineBot.Threads
{
    internal class CommandsInputController
    {
        Dictionary<ulong, Type> usersInputTracker;

        public CommandsInputController()
        {
            usersInputTracker = new Dictionary<ulong, Type>();
        }

        public bool IsAvailable(ulong userId)
        {
            return !usersInputTracker.ContainsKey(userId);
        }

        public void SetUserAt(ulong userId, MethodBase method)
        {
            usersInputTracker[userId] = method.DeclaringType;
        }

        public void ReleaseUserIfMethod(ulong userId, MethodBase method)
        {
            if (usersInputTracker.ContainsKey(userId) && usersInputTracker[userId] == method.DeclaringType)
                usersInputTracker.Remove(userId);
        }

    }
}
