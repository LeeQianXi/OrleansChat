using System;
using System.Net;
using System.Numerics;

namespace OrleansTest.IGrains
{
    public interface ISessionGrain : IGrainWithIntegerKey
    {
        ValueTask<string> Login(string username, string password);
        ValueTask<string> Login(string _cookie);
        Task Logout(string username, string cookie);
        Task Register(string username, string password);
        ValueTask<int> GetOnlineCount();
    }
}